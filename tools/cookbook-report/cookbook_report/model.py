from __future__ import annotations

from dataclasses import dataclass, field
from pathlib import Path
from typing import Literal


Severity = Literal["error", "warning"]
SchematicStatus = Literal[
    "fresh",
    "invalid",
    "missing",
    "skipped",
    "stale",
]


@dataclass(frozen=True)
class Issue:
    code: str
    severity: Severity
    message: str
    path: str | None = None

    def to_dict(self) -> dict[str, str]:
        result = {
            "code": self.code,
            "severity": self.severity,
            "message": self.message,
        }
        if self.path is not None:
            result["path"] = self.path
        return result


@dataclass
class RecipeReport:
    recipe_id: str
    path: Path
    netlists: list[str] = field(default_factory=list)
    analyses: list[str] = field(default_factory=list)
    measurements: list[str] = field(default_factory=list)
    documented_measurement_rows: int = 0
    displayed_components: int | None = None
    netlist_components: int | None = None
    schematic_status: SchematicStatus = "missing"
    issues: list[Issue] = field(default_factory=list)

    @property
    def error_count(self) -> int:
        return sum(issue.severity == "error" for issue in self.issues)

    @property
    def warning_count(self) -> int:
        return sum(issue.severity == "warning" for issue in self.issues)

    @property
    def status(self) -> str:
        if self.error_count:
            return "error"
        if self.warning_count:
            return "warning"
        return "ok"

    def to_dict(self) -> dict[str, object]:
        schematic: dict[str, object] = {"status": self.schematic_status}
        if self.displayed_components is not None:
            schematic["displayed_components"] = self.displayed_components
        if self.netlist_components is not None:
            schematic["netlist_components"] = self.netlist_components
        return {
            "id": self.recipe_id,
            "status": self.status,
            "netlists": self.netlists,
            "analyses": self.analyses,
            "measurements": self.measurements,
            "documented_measurement_rows": self.documented_measurement_rows,
            "schematic": schematic,
            "issues": [issue.to_dict() for issue in self.issues],
        }


@dataclass
class CookbookReport:
    root: Path
    recipes: list[RecipeReport] = field(default_factory=list)
    issues: list[Issue] = field(default_factory=list)

    @property
    def error_count(self) -> int:
        return sum(issue.severity == "error" for issue in self.issues) + sum(
            recipe.error_count for recipe in self.recipes
        )

    @property
    def warning_count(self) -> int:
        return sum(issue.severity == "warning" for issue in self.issues) + sum(
            recipe.warning_count for recipe in self.recipes
        )

    @property
    def passed_count(self) -> int:
        return sum(recipe.status == "ok" for recipe in self.recipes)

    def to_dict(self) -> dict[str, object]:
        return {
            "schema_version": 1,
            "root": ".",
            "summary": {
                "recipes": len(self.recipes),
                "passed": self.passed_count,
                "warnings": self.warning_count,
                "errors": self.error_count,
            },
            "issues": [issue.to_dict() for issue in self.issues],
            "recipes": [recipe.to_dict() for recipe in self.recipes],
        }

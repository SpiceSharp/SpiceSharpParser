from __future__ import annotations

import json

from .model import CookbookReport


def format_json(report: CookbookReport) -> str:
    return json.dumps(report.to_dict(), indent=2, sort_keys=True) + "\n"


def format_text(report: CookbookReport) -> str:
    summary = (
        f"Cookbook report: {len(report.recipes)} recipes, "
        f"{report.passed_count} passed, {report.warning_count} warnings, "
        f"{report.error_count} errors"
    )
    lines = [summary]
    for issue in report.issues:
        location = f" ({issue.path})" if issue.path else ""
        lines.append(f"{issue.severity.upper()} {issue.code}: {issue.message}{location}")
    for recipe in report.recipes:
        lines.append(
            f"{recipe.status.upper():7} {recipe.recipe_id} "
            f"[{','.join(recipe.analyses) or '-'}; {len(recipe.measurements)} measures; "
            f"schematic={recipe.schematic_status}]"
        )
        for issue in recipe.issues:
            location = f" ({issue.path})" if issue.path else ""
            lines.append(f"  {issue.severity.upper()} {issue.code}: {issue.message}{location}")
    return "\n".join(lines) + "\n"

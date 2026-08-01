from __future__ import annotations

import importlib
import re
import sys
import tempfile
import xml.etree.ElementTree as ET
from pathlib import Path
from types import ModuleType

from .model import CookbookReport, Issue, RecipeReport


REQUIRED_SECTIONS = (
    "Design Targets",
    "Schematic",
    "Runnable Netlist",
    "How It Works",
    "Design Calculations",
    "Simulation Setup",
    "Verified Measurements",
    "Experiments",
    "Model Boundary",
    "Running and Testing",
)
ANALYSIS_DIRECTIVE = re.compile(r"^\s*\.(AC|DC|NOISE|OP|TF|TRAN)\b", re.IGNORECASE)
MEASUREMENT_DIRECTIVE = re.compile(
    r"^\s*\.MEAS(?:URE)?\s+\S+\s+(\S+)", re.IGNORECASE
)
SAVE_DIRECTIVE = re.compile(r"^\s*\.SAVE\b", re.IGNORECASE)
PLOT_DIRECTIVE = re.compile(r"^\s*\.PLOT\b", re.IGNORECASE)
SVG_NAMESPACE = "{http://www.w3.org/2000/svg}"


class AuditEnvironmentError(RuntimeError):
    """Raised when a required repo-local audit dependency is unavailable."""


def default_cookbook_root() -> Path:
    working_candidate = Path.cwd() / "circuits" / "cookbook"
    if working_candidate.is_dir():
        return working_candidate
    return Path(__file__).resolve().parents[3] / "circuits" / "cookbook"


def _relative(path: Path, root: Path) -> str:
    try:
        return path.resolve().relative_to(root.resolve()).as_posix()
    except ValueError:
        return path.resolve().as_posix()


def _issue(
    report: RecipeReport,
    code: str,
    severity: str,
    message: str,
    path: Path | None,
    root: Path,
) -> None:
    report.issues.append(
        Issue(
            code=code,
            severity=severity,  # type: ignore[arg-type]
            message=message,
            path=_relative(path, root) if path is not None else None,
        )
    )


def _load_schematic_modules() -> tuple[ModuleType, ModuleType, type[Exception]]:
    try:
        config = importlib.import_module("cookbook_schematic.config")
        errors = importlib.import_module("cookbook_schematic.errors")
    except ModuleNotFoundError:
        sibling = Path(__file__).resolve().parents[2] / "cookbook-schematic"
        if not sibling.is_dir():
            raise AuditEnvironmentError(
                "cookbook-schematic is unavailable; install both repo-local tools with "
                "python -m pip install -e tools/cookbook-schematic -e tools/cookbook-report"
            ) from None
        sys.path.insert(0, str(sibling))
        try:
            config = importlib.import_module("cookbook_schematic.config")
            errors = importlib.import_module("cookbook_schematic.errors")
        except ModuleNotFoundError as exc:
            raise AuditEnvironmentError(
                "cookbook-schematic could not be imported from tools/cookbook-schematic"
            ) from exc
    return config, errors, errors.SchematicError


def _load_renderer() -> ModuleType:
    try:
        return importlib.import_module("cookbook_schematic.render")
    except ModuleNotFoundError as exc:
        if exc.name == "schemdraw":
            raise AuditEnvironmentError(
                "SchemDraw is unavailable; install the repo-local schematic tool before "
                "running freshness checks, or pass --skip-freshness"
            ) from exc
        raise


def _documented_measurement_rows(readme: str) -> int:
    lines = readme.splitlines()
    for index, line in enumerate(lines):
        if line.strip().casefold() != "## verified measurements":
            continue
        table_lines: list[str] = []
        for candidate in lines[index + 1 :]:
            stripped = candidate.strip()
            if not stripped:
                if table_lines:
                    break
                continue
            if stripped.startswith("|"):
                table_lines.append(stripped)
            elif table_lines:
                break
        return max(0, len(table_lines) - 2)
    return 0


def _check_readme(recipe: RecipeReport, readme_path: Path, netlist: Path, root: Path) -> None:
    if not readme_path.is_file():
        _issue(recipe, "CBR101", "error", "README.md is missing", readme_path, root)
        return
    try:
        content = readme_path.read_text(encoding="utf-8-sig")
    except UnicodeError as exc:
        _issue(recipe, "CBR102", "error", f"README.md is not valid UTF-8: {exc}", readme_path, root)
        return

    headings = {
        match.group(1).strip().casefold()
        for match in re.finditer(r"^##\s+(.+?)\s*$", content, re.MULTILINE)
    }
    for section in REQUIRED_SECTIONS:
        if section.casefold() not in headings:
            _issue(
                recipe,
                "CBR103",
                "error",
                f"README.md is missing the '## {section}' section",
                readme_path,
                root,
            )

    for filename, code in (
        (netlist.name, "CBR104"),
        ("schematic.svg", "CBR105"),
        ("response.svg", "CBR106"),
    ):
        if filename.casefold() not in content.casefold():
            _issue(
                recipe,
                code,
                "error",
                f"README.md does not reference {filename}",
                readme_path,
                root,
            )
    recipe.documented_measurement_rows = _documented_measurement_rows(content)


def _check_netlist(recipe: RecipeReport, netlist: Path, root: Path) -> None:
    try:
        content = netlist.read_text(encoding="utf-8-sig")
    except UnicodeError as exc:
        _issue(recipe, "CBR201", "error", f"Netlist is not valid UTF-8: {exc}", netlist, root)
        return
    lines = content.splitlines()
    recipe.analyses = sorted(
        {match.group(1).upper() for line in lines if (match := ANALYSIS_DIRECTIVE.match(line))}
    )
    recipe.measurements = [
        match.group(1)
        for line in lines
        if (match := MEASUREMENT_DIRECTIVE.match(line))
    ]
    if not recipe.analyses:
        _issue(recipe, "CBR202", "error", "Netlist has no supported analysis directive", netlist, root)
    if not recipe.measurements:
        _issue(recipe, "CBR203", "error", "Netlist has no .MEAS statements", netlist, root)
    if not any(SAVE_DIRECTIVE.match(line) for line in lines):
        _issue(recipe, "CBR204", "error", "Netlist has no .SAVE statement", netlist, root)
    if not any(PLOT_DIRECTIVE.match(line) for line in lines):
        _issue(recipe, "CBR205", "error", "Netlist has no .PLOT statement", netlist, root)


def _check_svg(
    recipe: RecipeReport,
    path: Path,
    root: Path,
    *,
    schematic: bool,
) -> bool:
    if not path.is_file():
        _issue(
            recipe,
            "CBR301" if schematic else "CBR401",
            "error",
            f"{path.name} is missing",
            path,
            root,
        )
        return False
    try:
        svg = ET.fromstring(path.read_bytes())
    except (ET.ParseError, OSError) as exc:
        _issue(
            recipe,
            "CBR302" if schematic else "CBR402",
            "error",
            f"{path.name} is not valid SVG XML: {exc}",
            path,
            root,
        )
        return False
    title = svg.find(f"{SVG_NAMESPACE}title")
    description = svg.find(f"{SVG_NAMESPACE}desc")
    if svg.attrib.get("role") != "img" or title is None or description is None:
        _issue(
            recipe,
            "CBR303" if schematic else "CBR403",
            "error",
            f"{path.name} must have role='img' plus title and desc elements",
            path,
            root,
        )
        return False
    labelled_by = set(svg.attrib.get("aria-labelledby", "").split())
    title_id = title.attrib.get("id")
    description_id = description.attrib.get("id")
    if not title_id or not description_id or not {title_id, description_id}.issubset(labelled_by):
        _issue(
            recipe,
            "CBR304" if schematic else "CBR404",
            "error",
            f"{path.name} aria-labelledby must reference its title and desc IDs",
            path,
            root,
        )
        return False
    return True


def _check_schematic(
    recipe: RecipeReport,
    recipe_path: Path,
    netlist: Path,
    root: Path,
    verify_freshness: bool,
) -> None:
    layout_path = recipe_path / "schematic.toml"
    schematic_path = recipe_path / "schematic.svg"
    svg_valid = _check_svg(recipe, schematic_path, root, schematic=True)
    if not layout_path.is_file():
        _issue(recipe, "CBR305", "error", "schematic.toml is missing", layout_path, root)
        recipe.schematic_status = "missing"
        return

    config, _errors, schematic_error = _load_schematic_modules()
    try:
        layout = config.load_layout(layout_path)
    except schematic_error as exc:
        _issue(recipe, "CBR306", "error", f"Invalid schematic layout: {exc}", layout_path, root)
        recipe.schematic_status = "invalid"
        return

    recipe.displayed_components = len(layout.elements)
    recipe.netlist_components = len(layout.netlist.components)
    if layout.netlist.path.resolve() != netlist.resolve():
        _issue(
            recipe,
            "CBR307",
            "error",
            f"schematic.toml references {layout.netlist.path.name}, expected {netlist.name}",
            layout_path,
            root,
        )
    if layout.output.resolve() != schematic_path.resolve():
        _issue(
            recipe,
            "CBR308",
            "error",
            "schematic.toml output must be schematic.svg in the recipe directory",
            layout_path,
            root,
        )
    for warning in config.layout_warnings(layout):
        _issue(recipe, "CBR309", "warning", warning, layout_path, root)

    if not svg_valid:
        recipe.schematic_status = "invalid" if schematic_path.is_file() else "missing"
        return
    if not verify_freshness:
        recipe.schematic_status = "skipped"
        return

    renderer = _load_renderer()
    with tempfile.TemporaryDirectory(prefix="cookbook-report-") as temporary_name:
        expected_path = Path(temporary_name) / "schematic.svg"
        try:
            render_layout = config.load_layout(layout_path, expected_path)
            renderer.render_layout(render_layout)
        except schematic_error as exc:
            _issue(recipe, "CBR310", "error", f"Could not render schematic: {exc}", layout_path, root)
            recipe.schematic_status = "invalid"
            return
        if expected_path.read_bytes() != schematic_path.read_bytes():
            _issue(
                recipe,
                "CBR311",
                "error",
                "schematic.svg is stale; regenerate it from schematic.toml",
                schematic_path,
                root,
            )
            recipe.schematic_status = "stale"
            return
    recipe.schematic_status = "fresh"


def _audit_recipe(recipe_path: Path, root: Path, verify_freshness: bool) -> RecipeReport:
    recipe = RecipeReport(
        recipe_id=_relative(recipe_path, root),
        path=recipe_path,
    )
    netlists = sorted(recipe_path.glob("*.cir"), key=lambda path: path.name.casefold())
    recipe.netlists = [path.name for path in netlists]
    if len(netlists) != 1:
        _issue(
            recipe,
            "CBR100",
            "error",
            f"Recipe must contain exactly one .cir netlist; found {len(netlists)}",
            recipe_path,
            root,
        )
        if not netlists:
            return recipe
    netlist = netlists[0]
    _check_netlist(recipe, netlist, root)
    _check_readme(recipe, recipe_path / "README.md", netlist, root)
    if recipe.measurements and recipe.documented_measurement_rows != len(recipe.measurements):
        _issue(
            recipe,
            "CBR206",
            "warning",
            "Verified Measurements table has "
            f"{recipe.documented_measurement_rows} rows for {len(recipe.measurements)} .MEAS statements",
            recipe_path / "README.md",
            root,
        )
    _check_schematic(recipe, recipe_path, netlist, root, verify_freshness)
    _check_svg(recipe, recipe_path / "response.svg", root, schematic=False)
    recipe.issues.sort(key=lambda issue: (issue.severity != "error", issue.code, issue.path or ""))
    return recipe


def audit_cookbook(root: Path, *, verify_freshness: bool = True) -> CookbookReport:
    root = root.resolve()
    report = CookbookReport(root=root)
    if not root.is_dir():
        report.issues.append(
            Issue("CBR001", "error", f"Cookbook root does not exist: {root}")
        )
        return report

    recipe_paths = sorted(
        {path.parent for path in root.rglob("*.cir")},
        key=lambda path: _relative(path, root).casefold(),
    )
    if not recipe_paths:
        report.issues.append(Issue("CBR002", "error", "Cookbook contains no .cir recipes"))
        return report

    report.recipes = [
        _audit_recipe(recipe_path, root, verify_freshness)
        for recipe_path in recipe_paths
    ]
    return report

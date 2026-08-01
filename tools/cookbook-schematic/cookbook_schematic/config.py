from __future__ import annotations

import math
import tomllib
from pathlib import Path
from typing import Any, Iterable

from .errors import LayoutError
from .model import (
    ElementLayout,
    JunctionLayout,
    NodeLabelLayout,
    Point,
    SchematicLayout,
    SymbolLayout,
    TextLayout,
    Theme,
    WireLayout,
)
from .netlist import canonical_node, parse_netlist


SCHEMA_VERSION = 1
ELEMENT_KINDS = frozenset(
    {
        "bjt-npn",
        "bjt-pnp",
        "resistor",
        "resistor-iec",
        "capacitor",
        "capacitor-polarized",
        "inductor",
        "diode",
        "zener",
        "schottky",
        "led",
        "source-v",
        "source-i",
        "source-pulse",
        "source-sin",
        "source-controlled-v",
        "source-controlled-i",
        "switch",
        "fuse",
        "buffer",
        "block",
    }
)
LABEL_LOCATIONS = frozenset({"top", "bottom", "left", "right", "center"})
ARROWS = frozenset({"start", "end", "both"})
SYMBOL_KINDS = frozenset({"ground", "ground-signal", "port", "test-point"})


def _table(raw: dict[str, Any], name: str, *, required: bool = False) -> dict[str, Any]:
    value = raw.get(name)
    if value is None:
        if required:
            raise LayoutError(f"Missing [{name}] table")
        return {}
    if not isinstance(value, dict):
        raise LayoutError(f"[{name}] must be a table")
    return value


def _array_tables(raw: dict[str, Any], name: str) -> list[dict[str, Any]]:
    value = raw.get(name, [])
    if not isinstance(value, list) or any(not isinstance(item, dict) for item in value):
        raise LayoutError(f"[[{name}]] must be an array of tables")
    return value


def _string(table: dict[str, Any], key: str, context: str, *, default: str | None = None) -> str:
    value = table.get(key, default)
    if not isinstance(value, str):
        raise LayoutError(f"{context}.{key} must be a string")
    return value


def _optional_string(table: dict[str, Any], key: str, context: str) -> str | None:
    value = table.get(key)
    if value is not None and not isinstance(value, str):
        raise LayoutError(f"{context}.{key} must be a string")
    return value


def _number(table: dict[str, Any], key: str, context: str, default: float) -> float:
    value = table.get(key, default)
    if isinstance(value, bool) or not isinstance(value, (int, float)):
        raise LayoutError(f"{context}.{key} must be a number")
    value = float(value)
    if not math.isfinite(value):
        raise LayoutError(f"{context}.{key} must be finite")
    return value


def _boolean(table: dict[str, Any], key: str, context: str, default: bool = False) -> bool:
    value = table.get(key, default)
    if not isinstance(value, bool):
        raise LayoutError(f"{context}.{key} must be true or false")
    return value


def _point(value: Any, context: str) -> Point:
    if not isinstance(value, list) or len(value) != 2:
        raise LayoutError(f"{context} must be [x, y]")
    if any(isinstance(item, bool) or not isinstance(item, (int, float)) for item in value):
        raise LayoutError(f"{context} coordinates must be numbers")
    point = (float(value[0]), float(value[1]))
    if not all(math.isfinite(item) for item in point):
        raise LayoutError(f"{context} coordinates must be finite")
    return point


def _location(value: str, context: str) -> str:
    if value not in LABEL_LOCATIONS:
        choices = ", ".join(sorted(LABEL_LOCATIONS))
        raise LayoutError(f"{context} must be one of: {choices}")
    return value


def _default_terminals(component_nodes: tuple[str, ...], component_name: str) -> tuple[str, str]:
    if len(component_nodes) != 2:
        raise LayoutError(
            f"Element {component_name} has {len(component_nodes)} SPICE nodes; "
            "set terminals = [\"start-node\", \"end-node\"] to choose the two "
            "nodes represented by this schematic symbol"
        )
    return component_nodes[0], component_nodes[1]


def _display_value(kind: str, value: str) -> str:
    if not value:
        return ""
    suffixes = {
        "resistor": "Ω",
        "resistor-iec": "Ω",
        "capacitor": "F",
        "capacitor-polarized": "F",
        "inductor": "H",
    }
    suffix = suffixes.get(kind)
    if suffix and "(" not in value and "=" not in value:
        return f"{value} {suffix}"
    if kind == "source-v" and _looks_numeric(value):
        return f"{value} V"
    if kind == "source-i" and _looks_numeric(value):
        return f"{value} A"
    return value


def _looks_numeric(value: str) -> bool:
    return bool(value) and value[0] in "+-.0123456789"


def _segments(points: tuple[Point, ...]) -> Iterable[tuple[Point, Point]]:
    return zip(points, points[1:])


def _axis_aligned(start: Point, end: Point) -> bool:
    return math.isclose(start[0], end[0]) or math.isclose(start[1], end[1])


def _point_on_segment(point: Point, start: Point, end: Point) -> bool:
    if not _axis_aligned(start, end):
        return False
    if math.isclose(start[0], end[0]):
        return math.isclose(point[0], start[0]) and min(start[1], end[1]) <= point[1] <= max(start[1], end[1])
    return math.isclose(point[1], start[1]) and min(start[0], end[0]) <= point[0] <= max(start[0], end[0])


def _segments_intersect(first: tuple[Point, Point], second: tuple[Point, Point]) -> bool:
    a, b = first
    c, d = second
    if not _axis_aligned(a, b) or not _axis_aligned(c, d):
        return False
    if math.isclose(a[0], b[0]) and math.isclose(c[0], d[0]):
        return math.isclose(a[0], c[0]) and max(min(a[1], b[1]), min(c[1], d[1])) <= min(max(a[1], b[1]), max(c[1], d[1]))
    if math.isclose(a[1], b[1]) and math.isclose(c[1], d[1]):
        return math.isclose(a[1], c[1]) and max(min(a[0], b[0]), min(c[0], d[0])) <= min(max(a[0], b[0]), max(c[0], d[0]))
    vertical = (a, b) if math.isclose(a[0], b[0]) else (c, d)
    horizontal = (c, d) if vertical == (a, b) else (a, b)
    x = vertical[0][0]
    y = horizontal[0][1]
    return (
        min(vertical[0][1], vertical[1][1]) <= y <= max(vertical[0][1], vertical[1][1])
        and min(horizontal[0][0], horizontal[1][0]) <= x <= max(horizontal[0][0], horizontal[1][0])
    )


class _UnionFind:
    def __init__(self, size: int):
        self.parent = list(range(size))

    def find(self, item: int) -> int:
        while self.parent[item] != item:
            self.parent[item] = self.parent[self.parent[item]]
            item = self.parent[item]
        return item

    def union(self, first: int, second: int) -> None:
        first_root = self.find(first)
        second_root = self.find(second)
        if first_root != second_root:
            self.parent[second_root] = first_root


def _validate_connectivity(elements: tuple[ElementLayout, ...], wires: tuple[WireLayout, ...]) -> None:
    terminals: dict[str, list[Point]] = {}
    for element in elements:
        terminals.setdefault(element.terminals[0], []).append(element.start)
        terminals.setdefault(element.terminals[1], []).append(element.end)

    wires_by_node: dict[str, list[WireLayout]] = {}
    for wire in wires:
        wires_by_node.setdefault(wire.node, []).append(wire)

    for node, points in terminals.items():
        if len(points) < 2:
            continue
        node_wires = wires_by_node.get(node, [])
        union = _UnionFind(len(points) + len(node_wires))
        for first in range(len(points)):
            for second in range(first + 1, len(points)):
                if points[first] == points[second]:
                    union.union(first, second)
        for point_index, point in enumerate(points):
            for wire_index, wire in enumerate(node_wires, start=len(points)):
                if any(_point_on_segment(point, start, end) for start, end in _segments(wire.points)):
                    union.union(point_index, wire_index)
        for first in range(len(node_wires)):
            for second in range(first + 1, len(node_wires)):
                if any(
                    _segments_intersect(a, b)
                    for a in _segments(node_wires[first].points)
                    for b in _segments(node_wires[second].points)
                ):
                    union.union(len(points) + first, len(points) + second)
        roots = {union.find(index) for index in range(len(points))}
        if len(roots) != 1:
            formatted = ", ".join(f"({x:g}, {y:g})" for x, y in points)
            raise LayoutError(
                f"Node {node!r} is geometrically disconnected across terminals: {formatted}"
            )


def load_layout(path: Path, output_override: Path | None = None) -> SchematicLayout:
    path = path.resolve()
    if not path.is_file():
        raise LayoutError(f"Layout does not exist: {path}")
    try:
        raw = tomllib.loads(path.read_text(encoding="utf-8-sig"))
    except tomllib.TOMLDecodeError as exc:
        raise LayoutError(f"Invalid TOML in {path}: {exc}") from exc

    version = raw.get("version")
    if version != SCHEMA_VERSION:
        raise LayoutError(f"version must be {SCHEMA_VERSION}, got {version!r}")

    schematic = _table(raw, "schematic", required=True)
    netlist_value = _string(schematic, "netlist", "schematic")
    netlist = parse_netlist((path.parent / netlist_value).resolve())
    output_value = _string(schematic, "output", "schematic", default="schematic.svg")
    output = output_override.resolve() if output_override else (path.parent / output_value).resolve()
    title = _string(schematic, "title", "schematic", default=netlist.title)
    description = _string(schematic, "description", "schematic")
    subtitle = _optional_string(schematic, "subtitle", "schematic")

    theme_table = _table(raw, "theme")
    theme = Theme(
        line_color=_string(theme_table, "line_color", "theme", default=Theme.line_color),
        text_color=_string(theme_table, "text_color", "theme", default=Theme.text_color),
        secondary_text_color=_string(theme_table, "secondary_text_color", "theme", default=Theme.secondary_text_color),
        signal_color=_string(theme_table, "signal_color", "theme", default=Theme.signal_color),
        accent_color=_string(theme_table, "accent_color", "theme", default=Theme.accent_color),
        block_fill=_string(theme_table, "block_fill", "theme", default=Theme.block_fill),
        background=_string(theme_table, "background", "theme", default=Theme.background),
        font_family=_string(theme_table, "font_family", "theme", default=Theme.font_family),
        font_size=_number(theme_table, "font_size", "theme", Theme.font_size),
        line_width=_number(theme_table, "line_width", "theme", Theme.line_width),
        unit=_number(theme_table, "unit", "theme", Theme.unit),
        padding=_number(theme_table, "padding", "theme", Theme.padding),
    )

    elements: list[ElementLayout] = []
    seen_components: set[str] = set()
    for index, item in enumerate(_array_tables(raw, "elements"), start=1):
        context = f"elements[{index}]"
        component_id = _string(item, "id", context)
        key = component_id.casefold()
        if key in seen_components:
            raise LayoutError(f"{context}.id duplicates component {component_id}")
        try:
            component = netlist.component(component_id)
        except KeyError as exc:
            raise LayoutError(f"{context}.id references unknown component {component_id}") from exc
        seen_components.add(key)
        kind = _string(item, "kind", context, default=component.kind)
        if kind not in ELEMENT_KINDS:
            choices = ", ".join(sorted(ELEMENT_KINDS))
            raise LayoutError(f"{context}.kind must be one of: {choices}")
        start = _point(item.get("start"), f"{context}.start")
        end = _point(item.get("end"), f"{context}.end")
        if start == end:
            raise LayoutError(f"{context} start and end must differ")
        if not _axis_aligned(start, end):
            raise LayoutError(f"{context} must be horizontal or vertical")
        terminal_value = item.get("terminals")
        if terminal_value is None:
            terminals = _default_terminals(component.nodes, component.name)
        else:
            if not isinstance(terminal_value, list) or len(terminal_value) != 2 or any(not isinstance(node, str) for node in terminal_value):
                raise LayoutError(f"{context}.terminals must contain two node names")
            terminals = tuple(canonical_node(node) for node in terminal_value)
            missing = [node for node in terminals if node not in component.nodes]
            if missing:
                raise LayoutError(
                    f"{context}.terminals contains nodes not present on {component.name}: "
                    + ", ".join(missing)
                )
        label_location = _location(_string(item, "label_location", context, default="top"), f"{context}.label_location")
        value_location = _location(_string(item, "value_location", context, default="bottom"), f"{context}.value_location")
        elements.append(
            ElementLayout(
                component=component,
                kind=kind,
                start=start,
                end=end,
                terminals=(terminals[0], terminals[1]),
                label=_string(item, "label", context, default=component.name),
                value=_string(item, "value", context, default=_display_value(kind, component.value)),
                label_location=label_location,
                value_location=value_location,
                color=_optional_string(item, "color", context),
                fill=_optional_string(item, "fill", context),
                reverse=_boolean(item, "reverse", context),
                flip=_boolean(item, "flip", context),
            )
        )

    wires: list[WireLayout] = []
    for index, item in enumerate(_array_tables(raw, "wires"), start=1):
        context = f"wires[{index}]"
        node = canonical_node(_string(item, "node", context))
        if node not in netlist.nodes:
            raise LayoutError(f"{context}.node references unknown node {node}")
        raw_points = item.get("points")
        if not isinstance(raw_points, list) or len(raw_points) < 2:
            raise LayoutError(f"{context}.points must contain at least two points")
        points = tuple(_point(point, f"{context}.points") for point in raw_points)
        for start, end in _segments(points):
            if start == end:
                raise LayoutError(f"{context}.points contains a zero-length segment")
            if not _axis_aligned(start, end):
                raise LayoutError(f"{context}.points must use orthogonal segments")
        arrow = _optional_string(item, "arrow", context)
        if arrow is not None and arrow not in ARROWS:
            raise LayoutError(f"{context}.arrow must be one of: {', '.join(sorted(ARROWS))}")
        wires.append(
            WireLayout(
                node=node,
                points=points,
                color=_optional_string(item, "color", context),
                arrow=arrow,
            )
        )

    junctions: list[JunctionLayout] = []
    for index, item in enumerate(_array_tables(raw, "junctions"), start=1):
        context = f"junctions[{index}]"
        node = canonical_node(_string(item, "node", context))
        if node not in netlist.nodes:
            raise LayoutError(f"{context}.node references unknown node {node}")
        junctions.append(JunctionLayout(node=node, at=_point(item.get("at"), f"{context}.at")))

    node_labels: list[NodeLabelLayout] = []
    for index, item in enumerate(_array_tables(raw, "node_labels"), start=1):
        context = f"node_labels[{index}]"
        node = canonical_node(_string(item, "node", context))
        if node not in netlist.nodes:
            raise LayoutError(f"{context}.node references unknown node {node}")
        node_labels.append(
            NodeLabelLayout(
                node=node,
                at=_point(item.get("at"), f"{context}.at"),
                location=_location(_string(item, "location", context, default="top"), f"{context}.location"),
                text=_optional_string(item, "text", context),
            )
        )

    texts: list[TextLayout] = []
    for index, item in enumerate(_array_tables(raw, "texts"), start=1):
        context = f"texts[{index}]"
        texts.append(
            TextLayout(
                text=_string(item, "text", context),
                at=_point(item.get("at"), f"{context}.at"),
                location=_location(_string(item, "location", context, default="center"), f"{context}.location"),
                size=_number(item, "size", context, theme.font_size) if "size" in item else None,
                color=_optional_string(item, "color", context),
                weight=_string(item, "weight", context, default="normal"),
            )
        )

    symbols: list[SymbolLayout] = []
    for index, item in enumerate(_array_tables(raw, "symbols"), start=1):
        context = f"symbols[{index}]"
        kind = _string(item, "kind", context)
        if kind not in SYMBOL_KINDS:
            raise LayoutError(f"{context}.kind must be one of: {', '.join(sorted(SYMBOL_KINDS))}")
        node_value = _optional_string(item, "node", context)
        node = canonical_node(node_value) if node_value is not None else None
        if node is not None and node not in netlist.nodes:
            raise LayoutError(f"{context}.node references unknown node {node}")
        symbols.append(
            SymbolLayout(
                kind=kind,
                at=_point(item.get("at"), f"{context}.at"),
                node=node,
                label=_optional_string(item, "label", context),
            )
        )

    element_tuple = tuple(elements)
    wire_tuple = tuple(wires)
    _validate_connectivity(element_tuple, wire_tuple)

    return SchematicLayout(
        path=path,
        version=version,
        netlist=netlist,
        output=output,
        title=title,
        description=description,
        subtitle=subtitle,
        theme=theme,
        elements=element_tuple,
        wires=wire_tuple,
        junctions=tuple(junctions),
        node_labels=tuple(node_labels),
        texts=tuple(texts),
        symbols=tuple(symbols),
        raw=raw,
    )


def layout_warnings(layout: SchematicLayout) -> list[str]:
    shown = {element.component.name.casefold() for element in layout.elements}
    warnings = [
        f"Netlist component {component.name} is not shown"
        for key, component in layout.netlist.components.items()
        if key not in shown
    ]
    wired_nodes = {wire.node for wire in layout.wires}
    terminal_nodes = {node for element in layout.elements for node in element.terminals}
    for wire in layout.wires:
        if wire.node not in terminal_nodes:
            warnings.append(f"Wire for node {wire.node} does not touch a displayed component terminal")
    for symbol in layout.symbols:
        if symbol.node is not None and symbol.node not in wired_nodes and symbol.node not in terminal_nodes:
            warnings.append(f"Symbol for node {symbol.node} is not attached to displayed wiring")
    return warnings

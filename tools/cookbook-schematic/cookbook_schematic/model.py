from __future__ import annotations

from dataclasses import dataclass, field
from pathlib import Path
from typing import Any

Point = tuple[float, float]


@dataclass(frozen=True)
class SpiceComponent:
    name: str
    kind: str
    nodes: tuple[str, ...]
    value: str
    source_line: int
    statement: str


@dataclass(frozen=True)
class Netlist:
    path: Path
    title: str
    components: dict[str, SpiceComponent]
    nodes: frozenset[str]

    def component(self, name: str) -> SpiceComponent:
        return self.components[name.casefold()]


@dataclass(frozen=True)
class ElementLayout:
    component: SpiceComponent
    kind: str
    start: Point
    end: Point
    terminals: tuple[str, str]
    label: str
    value: str
    label_location: str = "top"
    value_location: str = "bottom"
    color: str | None = None
    fill: str | None = None
    reverse: bool = False
    flip: bool = False


@dataclass(frozen=True)
class WireLayout:
    node: str
    points: tuple[Point, ...]
    color: str | None = None
    arrow: str | None = None


@dataclass(frozen=True)
class JunctionLayout:
    node: str
    at: Point


@dataclass(frozen=True)
class NodeLabelLayout:
    node: str
    at: Point
    location: str = "top"
    text: str | None = None


@dataclass(frozen=True)
class TextLayout:
    text: str
    at: Point
    location: str = "center"
    size: float | None = None
    color: str | None = None
    weight: str = "normal"


@dataclass(frozen=True)
class SymbolLayout:
    kind: str
    at: Point
    node: str | None = None
    label: str | None = None


@dataclass(frozen=True)
class Theme:
    line_color: str = "#243247"
    text_color: str = "#172235"
    secondary_text_color: str = "#57667a"
    signal_color: str = "#1976d2"
    accent_color: str = "#c23b52"
    block_fill: str = "#f4f8fd"
    background: str = "#ffffff"
    font_family: str = "Segoe UI, DejaVu Sans, sans-serif"
    font_size: float = 13
    line_width: float = 2.2
    unit: float = 3.0
    padding: float = 0.35


@dataclass(frozen=True)
class SchematicLayout:
    path: Path
    version: int
    netlist: Netlist
    output: Path
    title: str
    description: str
    subtitle: str | None
    theme: Theme
    elements: tuple[ElementLayout, ...] = field(default_factory=tuple)
    wires: tuple[WireLayout, ...] = field(default_factory=tuple)
    junctions: tuple[JunctionLayout, ...] = field(default_factory=tuple)
    node_labels: tuple[NodeLabelLayout, ...] = field(default_factory=tuple)
    texts: tuple[TextLayout, ...] = field(default_factory=tuple)
    symbols: tuple[SymbolLayout, ...] = field(default_factory=tuple)
    raw: dict[str, Any] = field(default_factory=dict, repr=False)

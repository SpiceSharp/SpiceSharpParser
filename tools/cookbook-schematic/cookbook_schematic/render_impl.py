from __future__ import annotations

import math
import xml.etree.ElementTree as ET
from typing import Callable

import schemdraw
import schemdraw.elements as elm
import schemdraw.logic as logic
from schemdraw.elements import Element2Term
from schemdraw.segments import Segment

from .errors import LayoutError
from .model import ElementLayout, Point, SchematicLayout, TextLayout, Theme


SVG_NAMESPACE = "http://www.w3.org/2000/svg"


class FunctionalBlock(Element2Term):
    """A compact two-terminal rectangular functional block."""

    def __init__(self, **kwargs):
        super().__init__(**kwargs)
        width = 1.4
        half_height = 0.55
        gap = (math.nan, math.nan)
        self.segments.append(
            Segment(
                [
                    (0, 0),
                    (0, half_height),
                    (width, half_height),
                    (width, -half_height),
                    (0, -half_height),
                    (0, 0),
                    gap,
                    (width, 0),
                ]
            )
        )


_ELEMENT_FACTORIES: dict[str, Callable[[], elm.Element]] = {
    "resistor": elm.Resistor,
    "resistor-iec": elm.ResistorIEC,
    "capacitor": elm.Capacitor,
    "capacitor-polarized": lambda: elm.Capacitor(polar=True),
    "inductor": elm.Inductor,
    "diode": elm.Diode,
    "zener": elm.Zener,
    "schottky": elm.Schottky,
    "led": elm.LED,
    "source-v": elm.SourceV,
    "source-i": elm.SourceI,
    "source-pulse": elm.SourcePulse,
    "source-sin": elm.SourceSin,
    "source-controlled-v": elm.SourceControlledV,
    "source-controlled-i": elm.SourceControlledI,
    "switch": elm.Switch,
    "fuse": elm.Fuse,
    "buffer": logic.Buf,
    "block": FunctionalBlock,
}


def symbol_names() -> tuple[str, ...]:
    return tuple(sorted(_ELEMENT_FACTORIES))


def _styled(element: elm.Element, color: str, fill: str | None = None) -> elm.Element:
    element.color(color)
    if fill is not None:
        element.fill(fill)
    return element


def _make_element(layout: ElementLayout, theme: Theme) -> elm.Element:
    try:
        element = _ELEMENT_FACTORIES[layout.kind]()
    except KeyError as exc:
        raise LayoutError(f"Unsupported renderer symbol: {layout.kind}") from exc
    color = layout.color or theme.line_color
    fill = layout.fill
    if fill is None and layout.kind in {"buffer", "block"}:
        fill = theme.block_fill
    _styled(element, color, fill)
    if layout.reverse:
        element.reverse()
    if layout.flip:
        element.flip()
    element.endpoints(layout.start, layout.end)
    if layout.label:
        element.label(
            layout.label,
            loc=layout.label_location,
            color=theme.text_color,
            fontsize=theme.font_size,
        )
    if layout.value:
        element.label(
            layout.value,
            loc=layout.value_location,
            color=theme.secondary_text_color,
            fontsize=max(theme.font_size - 1, 8),
        )
    return element


def _wire_arrow(arrow: str | None, index: int, segment_count: int) -> str | None:
    if arrow is None:
        return None
    at_start = arrow in {"start", "both"} and index == 0
    at_end = arrow in {"end", "both"} and index == segment_count - 1
    if at_start and at_end:
        return "<->"
    if at_start:
        return "<-"
    if at_end:
        return "->"
    return None


def _label_at(
    drawing: schemdraw.Drawing,
    text: str,
    at: Point,
    *,
    location: str,
    color: str,
    size: float,
    font: str,
) -> None:
    label = elm.Label().at(at)
    label.label(
        text,
        loc=location,
        color=color,
        fontsize=size,
        font=font,
        halign="left" if location == "right" else None,
    )
    drawing.add(label)


def _all_points(layout: SchematicLayout) -> list[Point]:
    points = [point for element in layout.elements for point in (element.start, element.end)]
    points.extend(point for wire in layout.wires for point in wire.points)
    points.extend(junction.at for junction in layout.junctions)
    points.extend(label.at for label in layout.node_labels)
    points.extend(text.at for text in layout.texts)
    points.extend(symbol.at for symbol in layout.symbols)
    return points or [(0, 0)]


def _draw_title(drawing: schemdraw.Drawing, layout: SchematicLayout) -> None:
    points = _all_points(layout)
    min_x = min(point[0] for point in points)
    max_y = max(point[1] for point in points)
    _label_at(
        drawing,
        layout.title,
        (min_x, max_y + 1.55),
        location="right",
        color=layout.theme.text_color,
        size=layout.theme.font_size + 7,
        font=layout.theme.font_family,
    )
    if layout.subtitle:
        _label_at(
            drawing,
            layout.subtitle,
            (min_x, max_y + 0.85),
            location="right",
            color=layout.theme.secondary_text_color,
            size=layout.theme.font_size - 1,
            font=layout.theme.font_family,
        )


def _draw_wires(drawing: schemdraw.Drawing, layout: SchematicLayout) -> None:
    for wire in layout.wires:
        segment_count = len(wire.points) - 1
        for index, (start, end) in enumerate(zip(wire.points, wire.points[1:])):
            line = elm.Line(arrow=_wire_arrow(wire.arrow, index, segment_count))
            line.at(start).to(end).color(wire.color or layout.theme.line_color)
            drawing.add(line)


def _draw_elements(drawing: schemdraw.Drawing, layout: SchematicLayout) -> None:
    for element in layout.elements:
        drawing.add(_make_element(element, layout.theme))


def _draw_junctions_and_labels(drawing: schemdraw.Drawing, layout: SchematicLayout) -> None:
    for junction in layout.junctions:
        drawing.add(elm.Dot().at(junction.at).color(layout.theme.line_color))
    for node_label in layout.node_labels:
        _label_at(
            drawing,
            node_label.text or node_label.node,
            node_label.at,
            location=node_label.location,
            color=layout.theme.signal_color,
            size=max(layout.theme.font_size - 1, 8),
            font=layout.theme.font_family,
        )


def _draw_text(drawing: schemdraw.Drawing, text: TextLayout, theme: Theme) -> None:
    _label_at(
        drawing,
        text.text,
        text.at,
        location=text.location,
        color=text.color or theme.secondary_text_color,
        size=text.size or theme.font_size,
        font=theme.font_family,
    )


def _draw_symbols(drawing: schemdraw.Drawing, layout: SchematicLayout) -> None:
    for symbol in layout.symbols:
        if symbol.kind == "ground":
            item = elm.Ground().at(symbol.at)
        elif symbol.kind == "ground-signal":
            item = elm.GroundSignal().at(symbol.at)
        elif symbol.kind == "port":
            item = elm.Terminal().at(symbol.at)
        elif symbol.kind == "test-point":
            item = elm.Dot(open=True).at(symbol.at)
        else:
            raise LayoutError(f"Unsupported renderer symbol: {symbol.kind}")
        item.color(layout.theme.line_color)
        if symbol.label:
            item.label(symbol.label, color=layout.theme.text_color)
        drawing.add(item)


def _add_accessibility(svg: bytes, layout: SchematicLayout) -> bytes:
    ET.register_namespace("", SVG_NAMESPACE)
    root = ET.fromstring(svg)
    root.set("role", "img")
    root.set("aria-labelledby", "title desc")
    root.set("data-generator", "spicesharp-cookbook-schematic 0.1.0")
    title = ET.Element(f"{{{SVG_NAMESPACE}}}title", {"id": "title"})
    title.text = layout.title
    description = ET.Element(f"{{{SVG_NAMESPACE}}}desc", {"id": "desc"})
    description.text = layout.description
    root.insert(0, description)
    root.insert(0, title)
    ET.indent(root, space="  ")
    return ET.tostring(root, encoding="utf-8", xml_declaration=False) + b"\n"


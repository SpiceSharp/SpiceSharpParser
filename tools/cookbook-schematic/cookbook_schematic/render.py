from __future__ import annotations

import os
import tempfile

import schemdraw

from .model import SchematicLayout
from .render_impl import (
    _add_accessibility,
    _draw_elements,
    _draw_junctions_and_labels,
    _draw_symbols,
    _draw_text,
    _draw_title,
    _draw_wires,
    symbol_names,
)


def render_layout(layout: SchematicLayout):
    schemdraw.use("svg")
    schemdraw.svgconfig.text = "text"

    drawing = schemdraw.Drawing(show=False, canvas="svg")
    drawing.config(
        unit=layout.theme.unit,
        fontsize=layout.theme.font_size,
        font=layout.theme.font_family,
        color=layout.theme.line_color,
        lw=layout.theme.line_width,
        bgcolor=layout.theme.background,
        margin=layout.theme.padding,
    )
    _draw_wires(drawing, layout)
    _draw_elements(drawing, layout)
    _draw_junctions_and_labels(drawing, layout)
    _draw_symbols(drawing, layout)
    for text in layout.texts:
        _draw_text(drawing, text, layout.theme)
    _draw_title(drawing, layout)

    rendered = _add_accessibility(drawing.get_imagedata("svg"), layout)
    output = layout.output
    output.parent.mkdir(parents=True, exist_ok=True)
    handle, temporary_name = tempfile.mkstemp(
        prefix=f".{output.stem}.", suffix=".tmp", dir=output.parent
    )
    try:
        with os.fdopen(handle, "wb") as temporary:
            temporary.write(rendered)
        os.replace(temporary_name, output)
    except BaseException:
        try:
            os.unlink(temporary_name)
        except FileNotFoundError:
            pass
        raise
    return output

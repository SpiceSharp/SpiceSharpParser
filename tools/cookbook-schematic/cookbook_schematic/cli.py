from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Sequence

from .config import ELEMENT_KINDS, load_layout, layout_warnings
from .errors import SchematicError


def _parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="cookbook-schematic",
        description="Render validated SchemDraw SVGs from SPICE netlists and TOML layouts.",
    )
    subparsers = parser.add_subparsers(dest="command", required=True)

    check = subparsers.add_parser("check", help="validate a layout without rendering")
    check.add_argument("layout", type=Path)
    check.add_argument("--quiet", action="store_true")

    render = subparsers.add_parser("render", help="validate and render a layout")
    render.add_argument("layout", type=Path)
    render.add_argument("--output", "-o", type=Path)
    render.add_argument("--quiet", action="store_true")

    symbols = subparsers.add_parser("symbols", help="list supported element kinds")
    symbols.add_argument("--json", action="store_true")
    return parser


def _print_warnings(warnings: list[str]) -> None:
    for warning in warnings:
        print(f"warning: {warning}", file=sys.stderr)


def _check(args: argparse.Namespace) -> int:
    layout = load_layout(args.layout)
    warnings = layout_warnings(layout)
    _print_warnings(warnings)
    if not args.quiet:
        print(
            f"OK: {layout.path} ({len(layout.elements)} displayed components, "
            f"{len(layout.netlist.components)} netlist components, {len(warnings)} warnings)"
        )
    return 0


def _render(args: argparse.Namespace) -> int:
    layout = load_layout(args.layout, args.output)
    warnings = layout_warnings(layout)
    _print_warnings(warnings)
    try:
        from .render import render_layout
    except ModuleNotFoundError as exc:
        if exc.name == "schemdraw":
            raise SchematicError(
                "SchemDraw is not installed. Run: python -m pip install -e tools/cookbook-schematic"
            ) from exc
        raise
    output = render_layout(layout)
    if not args.quiet:
        print(f"Rendered {output}")
    return 0


def _symbols(args: argparse.Namespace) -> int:
    symbols = sorted(ELEMENT_KINDS)
    if args.json:
        print(json.dumps(symbols, indent=2))
    else:
        print("\n".join(symbols))
    return 0


def main(argv: Sequence[str] | None = None) -> int:
    parser = _parser()
    args = parser.parse_args(argv)
    try:
        if args.command == "check":
            return _check(args)
        if args.command == "render":
            return _render(args)
        if args.command == "symbols":
            return _symbols(args)
        parser.error(f"unknown command: {args.command}")
    except SchematicError as exc:
        print(f"error: {exc}", file=sys.stderr)
        return 2
    return 2

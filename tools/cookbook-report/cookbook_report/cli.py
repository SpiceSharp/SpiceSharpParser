from __future__ import annotations

import argparse
import sys
from pathlib import Path
from typing import Sequence

from .audit import AuditEnvironmentError, audit_cookbook, default_cookbook_root
from .formatters import format_json, format_text


def _parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        prog="cookbook-report",
        description="Audit SpiceSharp cookbook recipes and generated SVG assets.",
    )
    parser.add_argument(
        "root",
        nargs="?",
        type=Path,
        help="cookbook root (default: repository circuits/cookbook)",
    )
    parser.add_argument("--format", choices=("text", "json"), default="text")
    parser.add_argument("--output", "-o", type=Path, help="write the report to a file")
    parser.add_argument(
        "--skip-freshness",
        action="store_true",
        help="validate layouts and SVGs without rendering for byte comparison",
    )
    parser.add_argument(
        "--warnings-as-errors",
        action="store_true",
        help="return exit code 1 when the report contains warnings",
    )
    return parser


def main(argv: Sequence[str] | None = None) -> int:
    args = _parser().parse_args(argv)
    root = args.root if args.root is not None else default_cookbook_root()
    try:
        report = audit_cookbook(root, verify_freshness=not args.skip_freshness)
    except AuditEnvironmentError as exc:
        print(f"error: {exc}", file=sys.stderr)
        return 2

    rendered = format_json(report) if args.format == "json" else format_text(report)
    if args.output is None:
        print(rendered, end="")
    else:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8", newline="\n")
        print(f"Wrote {args.output}", file=sys.stderr)

    if report.error_count or (args.warnings_as_errors and report.warning_count):
        return 1
    return 0

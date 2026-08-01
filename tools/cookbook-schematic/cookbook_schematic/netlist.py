from __future__ import annotations

import shlex
from pathlib import Path

from .errors import NetlistError
from .model import Netlist, SpiceComponent


_NODE_COUNTS = {
    "R": 2,
    "C": 2,
    "L": 2,
    "D": 2,
    "V": 2,
    "I": 2,
    "B": 2,
    "E": 4,
    "G": 4,
    "F": 2,
    "H": 2,
    "Q": 3,
    "J": 3,
    "M": 4,
    "S": 4,
    "W": 4,
    "T": 4,
    "A": 8,
}

_KINDS = {
    "R": "resistor",
    "C": "capacitor",
    "L": "inductor",
    "D": "diode",
    "V": "source-v",
    "I": "source-i",
    "B": "behavioral-source",
    "E": "vcvs",
    "G": "vccs",
    "F": "cccs",
    "H": "ccvs",
    "Q": "bjt-npn",
    "J": "jfet-n",
    "M": "mosfet-n",
    "S": "switch",
    "W": "switch",
    "T": "transmission-line",
    "A": "functional-block",
}


def canonical_node(node: str) -> str:
    return "0" if node.casefold() in {"0", "gnd", "ground"} else node


def _logical_statements(lines: list[str]) -> list[tuple[int, str]]:
    statements: list[tuple[int, str]] = []
    current_line = 0
    current = ""

    for line_number, raw in enumerate(lines, start=1):
        stripped = raw.strip()
        if not stripped or stripped.startswith("*"):
            continue
        if stripped.startswith("+"):
            if not current:
                raise NetlistError(
                    f"Line {line_number}: continuation has no preceding statement"
                )
            current += " " + stripped[1:].strip()
            continue
        if current:
            statements.append((current_line, current))
        current_line = line_number
        current = stripped

    if current:
        statements.append((current_line, current))
    return statements


def _tokens(statement: str, line_number: int) -> list[str]:
    try:
        return shlex.split(statement, posix=True)
    except ValueError as exc:
        raise NetlistError(f"Line {line_number}: {exc}") from exc


def _source_kind(prefix: str, tokens: list[str]) -> str:
    if prefix != "V" or len(tokens) < 4:
        return _KINDS[prefix]
    source = " ".join(tokens[3:]).lstrip().casefold()
    if source.startswith("pulse"):
        return "source-pulse"
    if source.startswith("sin"):
        return "source-sin"
    return "source-v"


def _display_value(prefix: str, tokens: list[str], node_count: int) -> str:
    value_tokens = tokens[1 + node_count :]
    if not value_tokens:
        return ""

    if prefix in {"V", "I", "B"}:
        return " ".join(value_tokens)
    if prefix in {"E", "G"}:
        return value_tokens[-1]
    if prefix in {"F", "H"}:
        return " ".join(value_tokens[-2:])
    if prefix == "A":
        return value_tokens[0]
    return value_tokens[0]


def parse_netlist(path: Path) -> Netlist:
    path = path.resolve()
    if not path.is_file():
        raise NetlistError(f"Netlist does not exist: {path}")

    text = path.read_text(encoding="utf-8-sig")
    lines = text.splitlines()
    title_index = next((i for i, line in enumerate(lines) if line.strip()), None)
    title = lines[title_index].strip() if title_index is not None else path.stem
    body = lines[title_index + 1 :] if title_index is not None else []
    components: dict[str, SpiceComponent] = {}
    nodes: set[str] = set()

    for relative_line, statement in _logical_statements(body):
        line_number = relative_line + (title_index or 0) + 1
        if statement.startswith("."):
            continue
        tokens = _tokens(statement, line_number)
        if not tokens:
            continue
        name = tokens[0]
        prefix = name[0].upper()
        if prefix == "K":
            continue
        if prefix == "X":
            raise NetlistError(
                f"Line {line_number}: subcircuit instance {name} needs an explicit "
                "layout-only declaration; automatic X-device pin parsing is ambiguous"
            )
        node_count = _NODE_COUNTS.get(prefix)
        if node_count is None:
            raise NetlistError(
                f"Line {line_number}: unsupported component prefix {prefix!r} in {name}"
            )
        if len(tokens) < node_count + 2:
            raise NetlistError(
                f"Line {line_number}: {name} requires at least {node_count} nodes"
            )

        component_nodes = tuple(
            canonical_node(token) for token in tokens[1 : 1 + node_count]
        )
        key = name.casefold()
        if key in components:
            raise NetlistError(f"Line {line_number}: duplicate component {name}")
        component = SpiceComponent(
            name=name,
            kind=_source_kind(prefix, tokens),
            nodes=component_nodes,
            value=_display_value(prefix, tokens, node_count),
            source_line=line_number,
            statement=statement,
        )
        components[key] = component
        nodes.update(component_nodes)

    return Netlist(
        path=path,
        title=title,
        components=components,
        nodes=frozenset(nodes),
    )

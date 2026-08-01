from pathlib import Path
from tempfile import TemporaryDirectory
import unittest

from cookbook_schematic.config import load_layout, layout_warnings
from cookbook_schematic.errors import LayoutError


NETLIST = """Example
V1 input 0 PULSE(0 5 0 1n 1n 5u 10u)
R1 input output 1k
C1 output 0 1u
.END
"""


class ConfigTests(unittest.TestCase):
    def write_layout(self, layout_source: str) -> Path:
        temporary = TemporaryDirectory()
        self.addCleanup(temporary.cleanup)
        directory = Path(temporary.name)
        (directory / "example.cir").write_text(NETLIST, encoding="utf-8")
        layout = directory / "schematic.toml"
        layout.write_text(layout_source, encoding="utf-8")
        return layout

    def test_loads_connected_layout_and_formats_values(self):
        path = self.write_layout(
            """version = 1
[schematic]
netlist = "example.cir"
title = "Example"
description = "An example."

[[elements]]
id = "V1"
start = [0, 3]
end = [0, 0]
value = "pulse"

[[elements]]
id = "R1"
start = [0, 3]
end = [3, 3]

[[elements]]
id = "C1"
start = [3, 3]
end = [3, 0]

[[wires]]
node = "0"
points = [[0, 0], [3, 0]]
"""
        )

        layout = load_layout(path)

        self.assertEqual(layout.elements[1].value, "1k Ω")
        self.assertEqual(layout.elements[2].value, "1u F")
        self.assertEqual(layout_warnings(layout), [])

    def test_rejects_disconnected_node_geometry(self):
        path = self.write_layout(
            """version = 1
[schematic]
netlist = "example.cir"
description = "An example."

[[elements]]
id = "V1"
start = [0, 3]
end = [0, 0]

[[elements]]
id = "R1"
start = [1, 3]
end = [3, 3]
"""
        )

        with self.assertRaisesRegex(LayoutError, "geometrically disconnected"):
            load_layout(path)

    def test_rejects_terminal_not_on_component(self):
        path = self.write_layout(
            """version = 1
[schematic]
netlist = "example.cir"
description = "An example."

[[elements]]
id = "R1"
kind = "buffer"
start = [0, 0]
end = [3, 0]
terminals = ["input", "missing"]
"""
        )

        with self.assertRaisesRegex(LayoutError, "not present on R1"):
            load_layout(path)


if __name__ == "__main__":
    unittest.main()

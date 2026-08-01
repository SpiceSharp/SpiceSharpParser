from pathlib import Path
from tempfile import TemporaryDirectory
import unittest
import xml.etree.ElementTree as ET

from cookbook_schematic.config import load_layout
from cookbook_schematic.render import render_layout


NETLIST = """Accessible SVG example
V1 input 0 5
R1 input output 1k
RLOAD output 0 10k
.END
"""

LAYOUT = """version = 1
[schematic]
netlist = "example.cir"
output = "schematic.svg"
title = "Accessible example"
description = "A voltage source drives a divider."

[[elements]]
id = "V1"
start = [0, 3]
end = [0, 0]

[[elements]]
id = "R1"
start = [0, 3]
end = [3, 3]

[[elements]]
id = "RLOAD"
start = [3, 3]
end = [3, 0]

[[wires]]
node = "0"
points = [[0, 0], [3, 0]]

[[symbols]]
kind = "ground"
node = "0"
at = [3, 0]
"""


class RenderTests(unittest.TestCase):
    def test_renders_deterministic_accessible_svg(self):
        with TemporaryDirectory() as temporary_name:
            directory = Path(temporary_name)
            (directory / "example.cir").write_text(NETLIST, encoding="utf-8")
            layout_path = directory / "schematic.toml"
            layout_path.write_text(LAYOUT, encoding="utf-8")
            layout = load_layout(layout_path)

            output = render_layout(layout)
            first_render = output.read_bytes()
            render_layout(layout)
            second_render = output.read_bytes()

            self.assertEqual(first_render, second_render)
            root = ET.fromstring(first_render)
            namespace = {"svg": "http://www.w3.org/2000/svg"}
            self.assertEqual(root.attrib["role"], "img")
            self.assertEqual(root.attrib["aria-labelledby"], "title desc")
            self.assertEqual(
                root.attrib["data-generator"],
                "spicesharp-cookbook-schematic 0.1.0",
            )
            self.assertEqual(root.findtext("svg:title", namespaces=namespace), "Accessible example")
            self.assertEqual(
                root.findtext("svg:desc", namespaces=namespace),
                "A voltage source drives a divider.",
            )
            self.assertIn("RLOAD", "".join(root.itertext()))


if __name__ == "__main__":
    unittest.main()


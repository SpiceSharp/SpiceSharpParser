from pathlib import Path
from tempfile import TemporaryDirectory
import unittest

from cookbook_schematic.config import load_layout, layout_warnings
from cookbook_schematic.render import render_layout


NETLIST = """NPN common-emitter stage
VCC vcc 0 12
RBASE vcc base 82k
Q1 collector base emitter Q2N3904
RC vcc collector 4.7k
RE emitter 0 1k
.MODEL Q2N3904 NPN(Bf=200)
.END
"""

LAYOUT = """version = 1
[schematic]
netlist = "example.cir"
description = "An NPN transistor with bias, collector, and emitter connections."

[[elements]]
id = "VCC"
start = [4, 4]
end = [4, 0]

[[elements]]
id = "RBASE"
start = [1.2, 4]
end = [1.2, 2]

[[elements]]
id = "Q1"
start = [2, 1]
end = [2, 3]
terminals = ["emitter", "collector"]

[[elements]]
id = "RC"
start = [2, 4]
end = [2, 3]

[[elements]]
id = "RE"
start = [2, 1]
end = [2, 0]

[[wires]]
node = "vcc"
points = [[1.2, 4], [4, 4]]

[[wires]]
node = "base"
points = [[1.2, 2], [1.47, 2]]

[[wires]]
node = "0"
points = [[2, 0], [4, 0]]
"""


class TransistorTests(unittest.TestCase):
    def test_renders_netlist_inferred_npn_symbol(self):
        with TemporaryDirectory() as temporary_name:
            directory = Path(temporary_name)
            (directory / "example.cir").write_text(NETLIST, encoding="utf-8")
            layout_path = directory / "schematic.toml"
            layout_path.write_text(LAYOUT, encoding="utf-8")

            layout = load_layout(layout_path)
            output = render_layout(layout)

            self.assertEqual(layout.elements[2].kind, "bjt-npn")
            self.assertEqual(layout.elements[2].terminals, ("emitter", "collector"))
            self.assertEqual(layout_warnings(layout), [])
            self.assertIn("Q1", output.read_text(encoding="utf-8"))


if __name__ == "__main__":
    unittest.main()

from pathlib import Path
from tempfile import TemporaryDirectory
import unittest

from cookbook_schematic.errors import NetlistError
from cookbook_schematic.netlist import parse_netlist


class NetlistTests(unittest.TestCase):
    def parse(self, source: str):
        temporary = TemporaryDirectory()
        self.addCleanup(temporary.cleanup)
        path = Path(temporary.name) / "test.cir"
        path.write_text(source, encoding="utf-8")
        return parse_netlist(path)

    def test_parses_values_waveforms_and_continuations(self):
        netlist = self.parse(
            "Example\n"
            "VPULSE in 0 PULSE(0 5 0 1n 1n 5u 10u)\n"
            "R1 in out 1k\n"
            "E1 buffered 0 out 0 1\n"
            "APHASE a b 0 0 0 0 output 0 PHASEDET\n"
            "+ Iout=50u\n"
            ".END\n"
        )

        self.assertEqual(netlist.title, "Example")
        self.assertEqual(netlist.component("vpulse").kind, "source-pulse")
        self.assertEqual(netlist.component("R1").nodes, ("in", "out"))
        self.assertEqual(netlist.component("R1").value, "1k")
        self.assertEqual(
            netlist.component("E1").nodes, ("buffered", "0", "out", "0")
        )
        self.assertEqual(netlist.component("APHASE").value, "PHASEDET")

    def test_rejects_ambiguous_subcircuit_instances(self):
        with self.assertRaisesRegex(NetlistError, "automatic X-device pin parsing"):
            self.parse("Example\nX1 in out gain PARAM=1\n.END\n")


if __name__ == "__main__":
    unittest.main()

from pathlib import Path
from tempfile import TemporaryDirectory
import unittest

from cookbook_report.audit import audit_cookbook
from cookbook_report.formatters import format_json


NETLIST = """Example recipe
V1 input 0 5
R1 input output 1k
R2 output 0 1k
.TRAN 1u 1m
.MEAS TRAN output_average AVG V(output) FROM=500u TO=1m
.SAVE V(input) V(output)
.PLOT TRAN V(input) V(output)
.END
"""

README = """# Example

## Design Targets
Target.
## Schematic
![Schematic](schematic.svg)
## Runnable Netlist
[example.cir](example.cir)
## How It Works
Explanation.
## Design Calculations
Calculation.
## Simulation Setup
Setup.
![Response](response.svg)
## Verified Measurements
| Measurement | Expected range | Verified result |
| --- | ---: | ---: |
| Output average | 2 to 3 V | 2.5 V |
## Experiments
Experiment.
## Model Boundary
Boundary.
## Running and Testing
Command.
"""

LAYOUT = """version = 1
[schematic]
netlist = "example.cir"
output = "schematic.svg"
title = "Example"
description = "A voltage divider."

[[elements]]
id = "V1"
start = [0, 3]
end = [0, 0]

[[elements]]
id = "R1"
start = [0, 3]
end = [3, 3]

[[elements]]
id = "R2"
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

RESPONSE = """<svg xmlns="http://www.w3.org/2000/svg" role="img" aria-labelledby="title desc">
<title id="title">Response</title><desc id="desc">Response summary.</desc>
</svg>
"""


class AuditTests(unittest.TestCase):
    def _recipe(self, directory: Path) -> Path:
        recipe = directory / "pure-spice" / "example"
        recipe.mkdir(parents=True)
        (recipe / "example.cir").write_text(NETLIST, encoding="utf-8")
        (recipe / "README.md").write_text(README, encoding="utf-8")
        (recipe / "schematic.toml").write_text(LAYOUT, encoding="utf-8")
        (recipe / "response.svg").write_text(RESPONSE, encoding="utf-8")
        return recipe

    def test_valid_recipe_passes_and_json_is_stable(self):
        from cookbook_schematic.config import load_layout
        from cookbook_schematic.render import render_layout

        with TemporaryDirectory() as temporary_name:
            root = Path(temporary_name)
            recipe = self._recipe(root)
            render_layout(load_layout(recipe / "schematic.toml"))

            report = audit_cookbook(root)
            first_json = format_json(report)
            second_json = format_json(audit_cookbook(root))

            self.assertEqual(report.error_count, 0)
            self.assertEqual(report.warning_count, 0)
            self.assertEqual(report.recipes[0].schematic_status, "fresh")
            self.assertEqual(report.recipes[0].measurements, ["output_average"])
            self.assertEqual(first_json, second_json)
            self.assertNotIn(temporary_name.replace("\\", "/"), first_json)

    def test_reports_contract_and_stale_schematic_errors(self):
        from cookbook_schematic.config import load_layout
        from cookbook_schematic.render import render_layout

        with TemporaryDirectory() as temporary_name:
            root = Path(temporary_name)
            recipe = self._recipe(root)
            render_layout(load_layout(recipe / "schematic.toml"))
            schematic = recipe / "schematic.svg"
            schematic.write_text(
                schematic.read_text(encoding="utf-8").replace("R1", "RX", 1),
                encoding="utf-8",
            )
            (recipe / "README.md").write_text("# Incomplete\n", encoding="utf-8")

            report = audit_cookbook(root)
            codes = {issue.code for issue in report.recipes[0].issues}

            self.assertIn("CBR103", codes)
            self.assertIn("CBR311", codes)
            self.assertEqual(report.recipes[0].schematic_status, "stale")
            self.assertGreater(report.error_count, 0)

    def test_skip_freshness_still_validates_layout_and_svg(self):
        with TemporaryDirectory() as temporary_name:
            root = Path(temporary_name)
            recipe = self._recipe(root)
            (recipe / "schematic.svg").write_text(RESPONSE, encoding="utf-8")

            report = audit_cookbook(root, verify_freshness=False)

            self.assertEqual(report.error_count, 0)
            self.assertEqual(report.recipes[0].schematic_status, "skipped")


if __name__ == "__main__":
    unittest.main()

from contextlib import redirect_stdout
from io import StringIO
from pathlib import Path
from tempfile import TemporaryDirectory
import unittest

from cookbook_report.cli import main


class CliTests(unittest.TestCase):
    def test_empty_cookbook_returns_failure_and_json(self):
        with TemporaryDirectory() as temporary_name:
            output = StringIO()
            with redirect_stdout(output):
                exit_code = main([temporary_name, "--format", "json", "--skip-freshness"])

            self.assertEqual(exit_code, 1)
            self.assertIn('"code": "CBR002"', output.getvalue())

    def test_missing_root_returns_failure_not_environment_error(self):
        with TemporaryDirectory() as temporary_name:
            missing = Path(temporary_name) / "missing"
            output = StringIO()
            with redirect_stdout(output):
                exit_code = main([str(missing), "--skip-freshness"])

            self.assertEqual(exit_code, 1)
            self.assertIn("CBR001", output.getvalue())


if __name__ == "__main__":
    unittest.main()

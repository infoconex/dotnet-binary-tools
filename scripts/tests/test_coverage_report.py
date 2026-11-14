"""Behavior tests for the repository-owned Cobertura coverage report."""
from __future__ import annotations

import sys
import tempfile
import unittest
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from coverage_report import build_report, find_aggregated_report, write_report  # noqa: E402


SAMPLE = """<?xml version="1.0"?>
<coverage lines-covered="2" lines-valid="3" branches-covered="1" branches-valid="2">
  <packages><package name="Infoconex.DotNetBinary.Domain"><classes>
    <class name="Example&lt;T&gt;" filename="/runner/src/Infoconex.DotNetBinary.Domain/Example.cs">
      <methods>
        <method name="Hit" signature="()"><lines>
          <line number="2" hits="2" branch="False" />
          <line number="3" hits="1" branch="True" condition-coverage="50% (1/2)" />
        </lines></method>
        <method name="Miss" signature="()"><lines>
          <line number="8" hits="0" branch="False" />
        </lines></method>
      </methods>
      <lines>
        <line number="2" hits="2" branch="False" />
        <line number="3" hits="1" branch="True" condition-coverage="50% (1/2)" />
        <line number="8" hits="0" branch="False" />
      </lines>
    </class>
  </classes></package></packages>
</coverage>"""


class CoverageReportTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        source = self.root / "src/Infoconex.DotNetBinary.Domain/Example.cs"
        source.parent.mkdir(parents=True)
        source.write_text("\n".join(("class Example {", "    public void Hit() {}", "    if (true) {}", "", "", "", "", "    public void Miss() {}", "}")), encoding="utf-8")
        (source.parent / "Infoconex.DotNetBinary.Domain.csproj").write_text("<Project />", encoding="utf-8")
        self.xml = self.root / "sample.cobertura.xml"
        self.xml.write_text(SAMPLE, encoding="utf-8")

    def test_counts_lines_branches_and_unexecuted_methods(self):
        report = build_report(self.xml, self.root)
        domain = report.projects["Infoconex.DotNetBinary.Domain"]
        self.assertEqual((2, 3), (domain.covered_lines, domain.total_lines))
        self.assertEqual((1, 2), (domain.covered_branches, domain.total_branches))
        self.assertEqual((1, 2), (domain.executed_methods, domain.total_methods))
        self.assertEqual(["Miss"], [m.name for m in domain.methods if not m.executed])

    def test_missing_branch_points_are_not_reported_as_full_coverage(self):
        self.xml.write_text(SAMPLE.replace(' condition-coverage="50% (1/2)"', '').replace(' branches-covered="1" branches-valid="2"', ''), encoding="utf-8")
        report = build_report(self.xml, self.root)
        domain = report.projects["Infoconex.DotNetBinary.Domain"]
        self.assertEqual(0, domain.total_branches)
        self.assertIsNone(domain.branch_percent)

    def test_renders_complete_html_and_markdown_without_injecting_markup(self):
        report = build_report(self.xml, self.root)
        output = self.root / "report"
        write_report(report, output, artifact_name="coverage-linux")
        html = (output / "index.html").read_text(encoding="utf-8")
        summary = (output / "summary.md").read_text(encoding="utf-8")
        self.assertIn("Unexecuted methods", html)
        self.assertIn("Miss", html)
        self.assertIn("Example&lt;T&gt;", html)
        self.assertIn("coverage-linux", summary)
        self.assertIn("2 / 3", summary)

    def test_reports_unmeasured_production_projects_honestly(self):
        other = self.root / "src/Infoconex.DotNetBinary.Application/Infoconex.DotNetBinary.Application.csproj"
        other.parent.mkdir(parents=True)
        other.write_text("<Project />", encoding="utf-8")
        report = build_report(self.xml, self.root)
        application = report.projects["Infoconex.DotNetBinary.Application"]
        self.assertEqual(0, application.total_lines)
        self.assertFalse(application.measured)

    def test_selects_aggregated_report_over_collector_intermediates(self):
        raw = self.root / "raw"
        aggregate = raw / "87de2eb7-250b-44c9-930e-a31083f3f9c4" / "runner.cobertura.xml"
        aggregate.parent.mkdir(parents=True)
        aggregate.write_text(SAMPLE, encoding="utf-8")
        intermediate = raw / "_runner_2026" / "In" / "runner"
        intermediate.mkdir(parents=True)
        (intermediate / "runner.cobertura.xml").write_text(SAMPLE, encoding="utf-8")
        (intermediate / "runner.cobertura[1].xml").write_text(SAMPLE, encoding="utf-8")
        self.assertEqual(aggregate, find_aggregated_report(raw))

    def test_rejects_multiple_final_reports_instead_of_double_counting(self):
        raw = self.root / "raw"
        for folder in ("a", "b"):
            target = raw / folder / "runner.cobertura.xml"
            target.parent.mkdir(parents=True)
            target.write_text(SAMPLE, encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "exactly one aggregated"):
            find_aggregated_report(raw)


if __name__ == "__main__":
    unittest.main()

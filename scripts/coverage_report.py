#!/usr/bin/env python3
"""Render an honest, dependency-free HTML report from Microsoft Cobertura XML.

Only instrumented source lines, branch points, and methods are measured. A
production assembly absent from Cobertura is *unmeasured*, not 100% covered.
"""
from __future__ import annotations

import argparse
import html
import json
import re
import sys
from dataclasses import dataclass, field
from datetime import datetime, timezone
from pathlib import Path
from xml.etree import ElementTree


ROOT = Path(__file__).resolve().parent.parent
BRANCH_COUNTS = re.compile(r"\((\d+)\s*/\s*(\d+)\)")
SOURCE_PATH = re.compile(r"(?:^|/)src/([^/]+)/(.+)$")


@dataclass(frozen=True)
class Line:
    number: int
    hits: int
    covered_branches: int = 0
    total_branches: int = 0


@dataclass(frozen=True)
class Method:
    class_name: str
    name: str
    signature: str
    filename: str
    lines: tuple[Line, ...]

    @property
    def measured(self) -> bool:
        return bool(self.lines)

    @property
    def executed(self) -> bool:
        return any(line.hits > 0 for line in self.lines)

    @property
    def display_name(self) -> str:
        return f"{self.class_name}.{self.name}{self.signature}"


@dataclass
class Project:
    name: str
    files: dict[str, dict[int, Line]] = field(default_factory=dict)
    methods: list[Method] = field(default_factory=list)

    @property
    def total_lines(self) -> int:
        return sum(len(lines) for lines in self.files.values())

    @property
    def covered_lines(self) -> int:
        return sum(line.hits > 0 for lines in self.files.values() for line in lines.values())

    @property
    def total_branches(self) -> int:
        return sum(line.total_branches for lines in self.files.values() for line in lines.values())

    @property
    def covered_branches(self) -> int:
        return sum(line.covered_branches for lines in self.files.values() for line in lines.values())

    @property
    def total_methods(self) -> int:
        return sum(method.measured for method in self.methods)

    @property
    def executed_methods(self) -> int:
        return sum(method.measured and method.executed for method in self.methods)

    @property
    def measured(self) -> bool:
        return self.total_lines > 0

    @property
    def line_percent(self) -> float | None:
        return percentage(self.covered_lines, self.total_lines)

    @property
    def branch_percent(self) -> float | None:
        return percentage(self.covered_branches, self.total_branches)


@dataclass
class Report:
    projects: dict[str, Project]
    repository_root: Path

    @property
    def covered_lines(self) -> int:
        return sum(project.covered_lines for project in self.projects.values())

    @property
    def total_lines(self) -> int:
        return sum(project.total_lines for project in self.projects.values())

    @property
    def covered_branches(self) -> int:
        return sum(project.covered_branches for project in self.projects.values())

    @property
    def total_branches(self) -> int:
        return sum(project.total_branches for project in self.projects.values())

    @property
    def executed_methods(self) -> int:
        return sum(project.executed_methods for project in self.projects.values())

    @property
    def total_methods(self) -> int:
        return sum(project.total_methods for project in self.projects.values())


def percentage(covered: int, total: int) -> float | None:
    return 100 * covered / total if total else None


def _line(element: ElementTree.Element) -> Line:
    try:
        number = int(element.attrib["number"])
        hits = int(element.attrib["hits"])
    except (ValueError, KeyError) as exc:
        raise ValueError(f"Invalid Cobertura line counters: {element.attrib}") from exc
    if number < 1 or hits < 0:
        raise ValueError(f"Negative or invalid Cobertura line: {element.attrib}")
    condition = BRANCH_COUNTS.search(element.get("condition-coverage", ""))
    branch_covered, branch_total = (map(int, condition.groups()) if condition else (0, 0))
    if branch_covered > branch_total:
        raise ValueError(f"Invalid Cobertura branch counters: {element.attrib}")
    return Line(number, hits, branch_covered, branch_total)


def _relative_source(filename: str) -> tuple[str, str] | None:
    match = SOURCE_PATH.search(filename.replace("\\", "/"))
    if not match or ".." in match.group(2).split("/"):
        return None
    project, relative = match.groups()
    return project, f"src/{project}/{relative}"


def build_report(coverage_xml: Path, repository_root: Path = ROOT) -> Report:
    """Read a single, already aggregated Cobertura report for production code."""
    projects = {
        path.stem: Project(path.stem)
        for path in (repository_root / "src").glob("*/*.csproj")
    }
    if not projects:
        raise ValueError("No production projects found in src/; cannot map coverage")
    try:
        coverage = ElementTree.parse(coverage_xml).getroot()
    except (ElementTree.ParseError, OSError) as exc:
        raise ValueError(f"Cannot read Cobertura XML {coverage_xml}: {exc}") from exc
    if coverage.tag != "coverage":
        raise ValueError(f"Expected Cobertura <coverage> root in {coverage_xml}")

    for source_class in coverage.findall("./packages/package/classes/class"):
        location = _relative_source(source_class.get("filename", ""))
        if location is None:
            continue
        project_name, filename = location
        project = projects.get(project_name)
        if project is None:
            continue

        file_lines = project.files.setdefault(filename, {})
        for element in source_class.findall("./lines/line"):
            line = _line(element)
            prior = file_lines.get(line.number)
            if prior is None or line.hits > prior.hits:
                file_lines[line.number] = line

        for element in source_class.findall("./methods/method"):
            method = Method(
                class_name=source_class.get("name", "Unknown"),
                name=element.get("name", "Unknown"),
                signature=element.get("signature", ""),
                filename=filename,
                lines=tuple(_line(item) for item in element.findall("./lines/line")),
            )
            project.methods.append(method)

    return Report(dict(sorted(projects.items())), repository_root)


def _measure(covered: int, total: int) -> str:
    percent = percentage(covered, total)
    return "Not measured" if percent is None else f"{percent:.1f}%"


def _bar(covered: int, total: int) -> str:
    value = percentage(covered, total)
    if value is None:
        return '<span class="muted">Not measured</span>'
    state = "poor" if value < 50 else "partial" if value < 80 else "good"
    return (
        f'<div class="meter"><span class="fill {state}" style="width:{value:.1f}%"></span></div>'
        f'<span class="metric">{value:.1f}% <small>{covered} / {total}</small></span>'
    )


def _source_markup(report: Report, filename: str, file_lines: dict[int, Line]) -> str:
    path = (report.repository_root / filename).resolve()
    src_root = (report.repository_root / "src").resolve()
    if not path.is_relative_to(src_root) or not path.is_file():
        return '<p class="muted">Source was unavailable; line counters remain listed below.</p>'
    try:
        contents = path.read_text(encoding="utf-8").splitlines()
    except (OSError, UnicodeError):
        return '<p class="muted">Source was unavailable; line counters remain listed below.</p>'
    rows = []
    for number, source in enumerate(contents, start=1):
        line = file_lines.get(number)
        state = "unmeasured" if line is None else "covered" if line.hits else "missed"
        hits = "" if line is None else str(line.hits)
        rows.append(
            f'<div class="source-line {state}"><span class="number">{number}</span>'
            f'<span class="hits">{hits}</span><code>{html.escape(source)}</code></div>'
        )
    return '<div class="source">' + "\n".join(rows) + "</div>"


def _report_html(report: Report) -> str:
    project_rows = []
    file_sections = []
    missed_rows = []
    for project in report.projects.values():
        name = html.escape(project.name)
        state = "Measured" if project.measured else "Not observed"
        project_rows.append(
            "<tr>"
            f"<td><strong>{name}</strong><small class='muted'>{state}</small></td>"
            f"<td>{_bar(project.covered_lines, project.total_lines)}</td>"
            f"<td>{_bar(project.covered_branches, project.total_branches)}</td>"
            f"<td>{_measure(project.executed_methods, project.total_methods)}</td>"
            "</tr>"
        )
        for method in project.methods:
            if method.measured and not method.executed:
                missed_rows.append(
                    "<tr>"
                    f"<td>{name}</td><td><code>{html.escape(method.display_name)}</code></td>"
                    f"<td>{html.escape(method.filename)}</td>"
                    "</tr>"
                )
        for number, (filename, lines) in enumerate(sorted(project.files.items())):
            methods = sorted((m for m in project.methods if m.filename == filename), key=lambda m: (m.name, m.signature))
            method_rows = []
            for method in methods:
                status = "Executed" if method.executed else "Unexecuted" if method.measured else "Unknown"
                method_rows.append(
                    "<tr>"
                    f"<td><code>{html.escape(method.display_name)}</code></td>"
                    f"<td><span class='status'>{status}</span></td>"
                    f"<td>{_measure(sum(line.hits > 0 for line in method.lines), len(method.lines))}</td>"
                    "</tr>"
                )
            counted = sum(line.hits > 0 for line in lines.values())
            file_sections.append(
                "<details class='file'>"
                f"<summary><span>{html.escape(filename)}</span><strong>{_measure(counted, len(lines))}</strong></summary>"
                "<div class='file-body'>"
                "<h4>Instrumented methods</h4>"
                "<table><thead><tr><th>Method</th><th>Execution</th><th>Line coverage</th></tr></thead>"
                f"<tbody>{''.join(method_rows) or '<tr><td colspan=3>No method details emitted</td></tr>'}</tbody></table>"
                "<h4>Source lines <small class='muted'>Hit count at left; gray lines were not measured</small></h4>"
                f"{_source_markup(report, filename, lines)}"
                "</div></details>"
            )

    unexecuted = ''.join(missed_rows) or '<tr><td colspan="3">No unexecuted instrumented methods were reported.</td></tr>'
    generated = datetime.now(timezone.utc).strftime("%Y-%m-%d %H:%M UTC")
    return f"""<!doctype html>
<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>.NET Binary Tools — Coverage</title>
<style>
:root{{--bg:#f4f7fb;--ink:#18253a;--muted:#65758c;--line:#dce4ed;--accent:#286ad4;--good:#13a786;--mid:#e0a437;--bad:#d26969}}
*{{box-sizing:border-box}}body{{margin:0;background:var(--bg);font:15px/1.5 system-ui,-apple-system,Segoe UI,sans-serif;color:var(--ink)}}
.hero{{background:linear-gradient(120deg,#12213c,#204d80);color:white;padding:42px max(24px,calc((100vw - 1150px)/2)) 48px}}
.hero h1{{font-size:34px;margin:8px 0}}.hero p{{color:#d2e2f3;max-width:850px}}
.badge{{display:inline-block;border:1px solid #8db8ef;border-radius:999px;padding:3px 12px;color:#ddebfc;font-size:12px;letter-spacing:.07em;text-transform:uppercase}}
main{{max-width:1200px;margin:-25px auto 65px;padding:0 22px}}.cards{{display:grid;grid-template-columns:repeat(3,minmax(0,1fr));gap:18px}}
.card,.panel{{border:1px solid var(--line);background:white;border-radius:14px;box-shadow:0 8px 22px #21395c0b}}
.card{{padding:20px}}.card .value{{font-size:30px;font-weight:750}}.card .caption{{font-size:13px;color:var(--muted)}}
.panel{{margin-top:22px;padding:22px}}h2{{font-size:21px;margin:0 0 15px}}h4{{margin:18px 0 8px}}p{{margin-top:6px}}
table{{border-collapse:collapse;width:100%;font-size:14px}}th{{text-align:left;background:#f5f8fc;font-weight:650;color:#4c6078}}
th,td{{border-bottom:1px solid var(--line);padding:11px 13px;vertical-align:top}}td small{{display:block}}
.muted{{color:var(--muted)}}.meter{{width:125px;height:8px;background:#e7edf4;border-radius:20px;overflow:hidden;margin:3px 0 5px}}
.fill{{display:block;height:100%;border-radius:20px}}.good{{background:var(--good)}}.partial{{background:var(--mid)}}.poor{{background:var(--bad)}}
.metric small{{display:inline;color:var(--muted);font-size:11px;margin-left:5px}}code{{font-family:ui-monospace,SFMono-Regular,Consolas,monospace;font-size:12px;overflow-wrap:anywhere}}
.file{{margin:12px 0;border:1px solid var(--line);border-radius:9px;overflow:hidden}}.file summary{{padding:13px 16px;cursor:pointer;display:flex;justify-content:space-between;gap:10px;background:#f8fafc}}
.file-body{{padding:10px 16px 20px}}.source{{overflow-x:auto;border:1px solid var(--line);border-radius:8px;background:#fcfdff;max-height:620px;overflow-y:auto}}
.source-line{{display:grid;grid-template-columns:54px 45px minmax(500px,1fr);white-space:pre;min-height:21px}}
.source-line .number,.source-line .hits{{font:12px/21px ui-monospace,Consolas,monospace;text-align:right;padding-right:12px;color:#70839a;border-right:1px solid #edf1f5}}
.source-line code{{font:12px/21px ui-monospace,Consolas,monospace;padding-left:12px}}.source-line.covered{{background:#e8f7ef}}.source-line.missed{{background:#fff0ef}}
.note{{font-size:13px;color:#526982}}.status{{font-weight:600}}footer{{max-width:1150px;margin:auto;padding:0 22px 45px;color:var(--muted);font-size:12px}}
@media(max-width:750px){{.cards{{grid-template-columns:1fr}}.panel{{overflow-x:auto}}.hero h1{{font-size:27px}}}}
</style></head><body>
<header class="hero"><span class="badge">Measured baseline · no threshold</span><h1>Code coverage</h1>
<p>.NET Binary Tools · NUnit + Microsoft Code Coverage. This report shows instrumented code only; absent projects and uninstrumented lines are not assumed covered.</p></header>
<main><div class="cards">
<div class="card"><div class="caption">Line coverage</div><div class="value">{_measure(report.covered_lines, report.total_lines)}</div><div class="caption">{report.covered_lines} / {report.total_lines} instrumented lines</div></div>
<div class="card"><div class="caption">Branch coverage</div><div class="value">{_measure(report.covered_branches, report.total_branches)}</div><div class="caption">{report.covered_branches} / {report.total_branches} reported branch outcomes</div></div>
<div class="card"><div class="caption">Method execution</div><div class="value">{_measure(report.executed_methods, report.total_methods)}</div><div class="caption">{report.executed_methods} / {report.total_methods} instrumented methods</div></div>
</div>
<section class="panel"><h2>Project coverage</h2><table><thead><tr><th>Project</th><th>Lines</th><th>Branches</th><th>Methods</th></tr></thead>
<tbody>{''.join(project_rows)}</tbody></table></section>
<section class="panel"><h2>Unexecuted methods</h2><p class="note">Methods are considered unexecuted only when their emitted source lines all have zero hits. Missing methods are not inferred from this report.</p>
<table><thead><tr><th>Project</th><th>Method</th><th>File</th></tr></thead><tbody>{unexecuted}</tbody></table></section>
<section class="panel"><h2>File and source details</h2><p class="note">Expand any file to inspect method results and color-coded source lines. Gray means not instrumented, not necessarily untested.</p>
{''.join(file_sections) or '<p>No instrumented source files were reported.</p>'}</section>
</main><footer>Generated {generated}. Method, branch, and line data originate from Microsoft Cobertura output; no paid reporting feature is required.</footer>
</body></html>"""


def _markdown(report: Report, artifact_name: str) -> str:
    lines = [
        "## NUnit coverage — measured baseline",
        "",
        f"**Lines:** {_measure(report.covered_lines, report.total_lines)} ({report.covered_lines} / {report.total_lines}) · "
        f"**Branches:** {_measure(report.covered_branches, report.total_branches)} ({report.covered_branches} / {report.total_branches}) · "
        f"**Methods:** {_measure(report.executed_methods, report.total_methods)} ({report.executed_methods} / {report.total_methods})",
        "",
        "| Production project | Lines | Branches | Methods |",
        "| --- | ---: | ---: | ---: |",
    ]
    for project in report.projects.values():
        lines.append(
            f"| `{project.name}` | {_measure(project.covered_lines, project.total_lines)} "
            f"({project.covered_lines} / {project.total_lines}) | "
            f"{_measure(project.covered_branches, project.total_branches)} "
            f"({project.covered_branches} / {project.total_branches}) | "
            f"{_measure(project.executed_methods, project.total_methods)} "
            f"({project.executed_methods} / {project.total_methods}) |"
        )
    missed = report.total_methods - report.executed_methods
    lines.extend([
        "",
        f"**Unexecuted instrumented methods:** {missed} — see `report/index.html` for names and source detail.",
        f"**Complete downloadable report:** `{artifact_name}` Actions artifact, containing `report/index.html`, Cobertura XML and TRX results.",
        "",
        "> No enforcement threshold is set. Not observed and uninstrumented code is not treated as fully covered.",
        "",
    ])
    return "\n".join(lines)


def write_report(report: Report, output: Path, artifact_name: str = "coverage") -> None:
    """Write independent local HTML, Markdown and JSON artifacts."""
    output.mkdir(parents=True, exist_ok=True)
    (output / "index.html").write_text(_report_html(report), encoding="utf-8")
    (output / "summary.md").write_text(_markdown(report, artifact_name), encoding="utf-8")
    data = {
        "lines": {"covered": report.covered_lines, "total": report.total_lines},
        "branches": {"covered": report.covered_branches, "total": report.total_branches},
        "methods": {"executed": report.executed_methods, "total": report.total_methods},
        "projects": {
            name: {
                "measured": project.measured,
                "lines": {"covered": project.covered_lines, "total": project.total_lines},
                "branches": {"covered": project.covered_branches, "total": project.total_branches},
                "methods": {"executed": project.executed_methods, "total": project.total_methods},
            }
            for name, project in report.projects.items()
        },
    }
    (output / "coverage.json").write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")


def find_aggregated_report(directory: Path) -> Path:
    """Select the merged collector output, ignoring its per-testhost intermediates.

    VSTest puts its aggregate in ``raw/<guid>/*.cobertura.xml`` and retains
    intermediate outputs under a separate ``raw/_runner.../In/...`` tree.
    A single report at a different location is also accepted for local use.
    """
    candidates = sorted(
        path for path in directory.rglob("*.cobertura.xml")
        if "In" not in path.relative_to(directory).parts
    )
    if len(candidates) != 1:
        raise ValueError(
            f"Expected exactly one aggregated Cobertura XML under {directory}; "
            f"found {len(candidates)}: {', '.join(str(item) for item in candidates)}"
        )
    return candidates[0]


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, required=True, help="directory of raw Cobertura attachments")
    parser.add_argument("--output", type=Path, required=True, help="report output directory")
    parser.add_argument("--artifact-name", default="coverage", help="CI artifact name for the summary")
    parser.add_argument("--repo", type=Path, default=ROOT, help="repository root")
    args = parser.parse_args(argv)
    try:
        report = build_report(find_aggregated_report(args.input), args.repo)
        if report.total_lines == 0:
            raise ValueError("Cobertura contains no instrumented production source lines")
        write_report(report, args.output, args.artifact_name)
    except (ValueError, OSError) as exc:
        print(f"Coverage report failed: {exc}", file=sys.stderr)
        return 1
    print(f"Coverage report: {args.output / 'index.html'}")
    print(f"Line coverage: {report.covered_lines}/{report.total_lines}; branch coverage: {report.covered_branches}/{report.total_branches}")
    return 0


if __name__ == "__main__":
    sys.exit(main())

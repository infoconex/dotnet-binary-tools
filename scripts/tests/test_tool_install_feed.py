"""Guard the isolated Native AOT .NET tool installation feed contract."""

import re
import unittest
import xml.etree.ElementTree as ET
from pathlib import Path


SCRIPT = Path(__file__).resolve().parents[1] / "validate-tool-package.ps1"


class ToolInstallFeedTests(unittest.TestCase):
    def test_staged_infoconex_packages_cannot_resolve_from_nuget_org(self):
        script = SCRIPT.read_text(encoding="utf-8")
        match = re.search(r'@"\n(<\?xml.*?</configuration>)\n"@', script, re.DOTALL)
        self.assertIsNotNone(match, "Expected self-contained NuGet tool config")
        config = ET.fromstring(match.group(1).replace("$escapedSource", "/staged"))

        sources = {item.attrib["key"]: item.attrib["value"]
                   for item in config.findall("./packageSources/add")}
        self.assertEqual({
            "staged": "/staged",
            "nuget.org": "https://api.nuget.org/v3/index.json",
        }, sources)
        mappings = {
            item.attrib["key"]: {pattern.attrib["pattern"] for pattern in item.findall("package")}
            for item in config.findall("./packageSourceMapping/packageSource")
        }
        self.assertEqual({
            "staged": {"Infoconex.DotNetBinary.Tools", "Infoconex.DotNetBinary.Tools.*"},
            "nuget.org": {"Microsoft.NETCore.App.Host.*"},
        }, mappings)
        self.assertIn("--configfile $nugetConfig --no-cache", script)
        self.assertIn("$env:NUGET_PACKAGES = Join-Path $installationPath 'nuget-cache'", script)


if __name__ == "__main__":
    unittest.main()

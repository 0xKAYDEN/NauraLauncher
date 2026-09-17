"""Source-level XAML checks; run with python3 -m unittest discover -s tests.

These checks do not replace compiling and launching the WPF app on Windows.
"""
from pathlib import Path
import re
import unittest
import xml.etree.ElementTree as ET


PROJECT = Path(__file__).resolve().parents[1] / "NauraLauncher"
XAML = "{http://schemas.microsoft.com/winfx/2006/xaml}"


def local_name(name):
    return name.rsplit("}", 1)[-1]


class XamlTests(unittest.TestCase):
    def test_properties_are_not_assigned_twice(self):
        for path in PROJECT.rglob("*.xaml"):
            with self.subTest(file=path.relative_to(PROJECT)):
                root = ET.parse(path).getroot()
                for element in root.iter():
                    assigned = {local_name(name) for name in element.attrib}
                    for child in element:
                        name = local_name(child.tag)
                        if "." not in name:
                            continue
                        owner, prop = name.rsplit(".", 1)
                        # Attached properties retain their owner prefix.
                        key = prop if owner == local_name(element.tag) else name
                        self.assertNotIn(key, assigned,
                                         f"{path.name}: {element.tag} repeats {key}")
                        assigned.add(key)

    def test_inline_label_styles_keep_the_base_style(self):
        ns = {"w": "http://schemas.microsoft.com/winfx/2006/xaml/presentation"}
        count = 0
        for path in (PROJECT / "Views").glob("*.xaml"):
            for style in ET.parse(path).findall(".//w:TextBlock.Style/w:Style", ns):
                self.assertEqual(style.get("BasedOn"), "{StaticResource Label}")
                count += 1
        self.assertEqual(count, 6)

    def test_reported_converter_resources_implement_value_converter(self):
        resources = ET.parse(PROJECT / "Themes/Converters.xaml").getroot()
        source = (PROJECT / "Common/Converters.cs").read_text()
        for key in ("PctGrid", "HexBrush"):
            matches = [r for r in resources if r.get(XAML + "Key") == key]
            self.assertEqual(len(matches), 1)
            self.assertRegex(source, rf"public class {re.escape(local_name(matches[0].tag))}\s*:\s*IValueConverter\b")


if __name__ == "__main__":
    unittest.main()

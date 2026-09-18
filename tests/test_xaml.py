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
        # After Conquer Online overhaul, we have many more label overrides (at least 6)
        self.assertGreaterEqual(count, 6, f"Expected at least 6 inline label styles, found {count}")

    def test_reported_converter_resources_implement_value_converter(self):
        resources = ET.parse(PROJECT / "Themes/Converters.xaml").getroot()
        source = (PROJECT / "Common/Converters.cs").read_text()
        for key in ("PctGrid", "HexBrush"):
            matches = [r for r in resources if r.get(XAML + "Key") == key]
            self.assertEqual(len(matches), 1)
            self.assertRegex(source, rf"public class {re.escape(local_name(matches[0].tag))}\s*:\s*IValueConverter\b")

    def test_conquer_online_assets_exist(self):
        # Verify new Conquer Online themed assets exist
        assets_dir = PROJECT / "Assets"
        required_assets = [
            "hero_conquer.png",
            "card_trojan.png",
            "card_warrior.png",
            "card_archer.png",
            "card_taoist.png",
            "card_ninja.png",
            "avatar_conquer.png"
        ]
        for asset in required_assets:
            with self.subTest(asset=asset):
                self.assertTrue((assets_dir / asset).exists(), f"Missing Conquer Online asset: {asset}")

    def test_new_pages_exist(self):
        # Verify new pages for Conquer Online features exist
        views_dir = PROJECT / "Views"
        required_pages = [
            "AuthPage.xaml",
            "ProfilePage.xaml",
            "InventoryPage.xaml",
            "FriendsPage.xaml",
            "MarketplacePage.xaml",
            "AuctionPage.xaml"
        ]
        for page in required_pages:
            with self.subTest(page=page):
                self.assertTrue((views_dir / page).exists(), f"Missing page: {page}")

    def test_domain_entities_exist(self):
        # Verify clean architecture domain entities
        domain_dir = PROJECT / "Domain" / "Entities"
        required_entities = [
            "User.cs",
            "InventoryItem.cs",
            "MarketplaceListing.cs",
            "AuctionLot.cs",
            "Friend.cs",
            "VoiceCall.cs"
        ]
        for entity in required_entities:
            with self.subTest(entity=entity):
                self.assertTrue((domain_dir / entity).exists(), f"Missing domain entity: {entity}")

    def test_mysql_schema_compatible(self):
        # Verify MySQL 5.6 compatible schema exists and doesn't use JSON type
        schema_path = Path(__file__).resolve().parents[1] / "Database" / "schema.sql"
        self.assertTrue(schema_path.exists(), "Missing MySQL schema")
        content = schema_path.read_text()
        # Should not use JSON type (MySQL 5.7+)
        self.assertNotIn("`payload` JSON", content, "Schema uses JSON type which is MySQL 5.7+ only")
        # Should use InnoDB
        self.assertIn("ENGINE=InnoDB", content, "Schema should use InnoDB")
        # Should have Conquer Online tables
        self.assertIn("CREATE TABLE `users`", content)
        self.assertIn("CREATE TABLE `wallets`", content)
        self.assertIn("CREATE TABLE `inventory_items`", content)
        self.assertIn("CREATE TABLE `marketplace_listings`", content)
        self.assertIn("CREATE TABLE `auction_lots`", content)
        self.assertIn("CREATE TABLE `friendships`", content)
        self.assertIn("CREATE TABLE `voice_channels`", content)


if __name__ == "__main__":
    unittest.main()

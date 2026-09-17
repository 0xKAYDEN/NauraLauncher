"""Clean architecture and production readiness validation suite.
Run with: python3 -m unittest discover -s tests
"""
import os
from pathlib import Path
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[1]
LAUNCHER = ROOT / "NauraLauncher"
SERVER = ROOT / "server"


class ArchitectureTests(unittest.TestCase):
    def test_clean_architecture_directories_exist(self):
        expected_dirs = [
            LAUNCHER / "Core" / "Entities",
            LAUNCHER / "Core" / "Interfaces",
            LAUNCHER / "Infrastructure" / "Security",
            LAUNCHER / "Infrastructure" / "Network",
            LAUNCHER / "Infrastructure" / "Services",
            LAUNCHER / "Infrastructure" / "Database",
            LAUNCHER / "Infrastructure" / "DI"
        ]
        for d in expected_dirs:
            self.assertTrue(d.is_dir(), f"Missing clean architecture directory: {d}")

    def test_core_interfaces_defined(self):
        interfaces = [
            "IAuthService.cs",
            "IApiClient.cs",
            "IRealtimeWebSocketService.cs",
            "IGameLibraryService.cs",
            "IGameProcessLauncher.cs",
            "IDownloadPatcherService.cs",
            "IAuctionService.cs",
            "ISettingsService.cs",
            "ISecurityService.cs",
            "IMySqlService.cs"
        ]
        for iface in interfaces:
            path = LAUNCHER / "Core" / "Interfaces" / iface
            self.assertTrue(path.is_file(), f"Missing interface: {iface}")
            content = path.read_text(encoding="utf-8")
            self.assertIn(f"interface {iface.replace('.cs', '')}", content)

    def test_demo_prototype_code_removed_from_viewmodels(self):
        home_vm = (LAUNCHER / "ViewModels" / "HomeViewModel.cs").read_text(encoding="utf-8")
        self.assertNotIn("() => IsLaunching = !IsLaunching", home_vm, "HomeViewModel still has prototype toggle")

        auction_vm = (LAUNCHER / "ViewModels" / "AuctionViewModel.cs").read_text(encoding="utf-8")
        self.assertIn("ServiceContainer.Auction.PlaceBidAsync", auction_vm, "AuctionViewModel does not call auction service")
        self.assertIn("ServiceContainer.Auction.BidBroadcastReceived", auction_vm, "AuctionViewModel does not subscribe to real-time bids")

        marketplace_vm = (LAUNCHER / "ViewModels" / "MarketplaceViewModel.cs").read_text(encoding="utf-8")
        self.assertIn("ServiceContainer.Library.ClaimOrPurchaseGameAsync", marketplace_vm)

        settings_vm = (LAUNCHER / "ViewModels" / "SettingsViewModel.cs").read_text(encoding="utf-8")
        self.assertIn("ServiceContainer.Settings.SaveAsync", settings_vm)

        main_vm = (LAUNCHER / "ViewModels" / "MainViewModel.cs").read_text(encoding="utf-8")
        self.assertIn("ServiceContainer.WebSocket.LatencyUpdated", main_vm)
        self.assertIn("ServiceContainer.Auth.CreditsChanged", main_vm)

    def test_database_schema_and_seed_exist(self):
        schema_path = SERVER / "db" / "schema.sql"
        seed_path = SERVER / "db" / "seed.sql"
        self.assertTrue(schema_path.is_file(), "Missing schema.sql")
        self.assertTrue(seed_path.is_file(), "Missing seed.sql")

        schema_content = schema_path.read_text(encoding="utf-8")
        for table in ["users", "user_sessions", "games", "game_manifests", "user_library", "auction_lots", "auction_bids"]:
            self.assertIn(f"CREATE TABLE IF NOT EXISTS `{table}`", schema_content)

    def test_tls_certificates_exist(self):
        crt = SERVER / "certs" / "server.crt"
        key = SERVER / "certs" / "server.key"
        self.assertTrue(crt.is_file(), "Missing TLS certificate")
        self.assertTrue(key.is_file(), "Missing TLS private key")

    def test_backend_integration_test_suite_passes(self):
        res = subprocess.run(
            ["node", str(SERVER / "tests" / "server.test.js")],
            cwd=str(ROOT),
            capture_output=True,
            text=True,
            timeout=30
        )
        self.assertEqual(res.returncode, 0, f"Backend test suite failed:\n{res.stdout}\n{res.stderr}")
        self.assertIn("ALL BACKEND INTEGRATION TESTS PASSED", res.stdout)


if __name__ == "__main__":
    unittest.main()

import importlib.util
from pathlib import Path
import tempfile
import unittest
import zipfile

spec = importlib.util.spec_from_file_location("store", Path(__file__).parents[2] / "scripts/store_submission.py")
store = importlib.util.module_from_spec(spec)
spec.loader.exec_module(store)


class FakeClient:
    def __init__(self, pending=False, status="PreProcessing"):
        self.pending = pending
        self.status = status
        self.calls = []
        self.uploaded = False

    def request(self, method, path, value=None):
        self.calls.append((method, path, value))
        if method == "GET" and path.endswith("/status"):
            return {"status": self.status}
        if method == "GET":
            return {"pendingApplicationSubmission": {"id": "old"} if self.pending else None,
                    "lastPublishedApplicationSubmission": {"id": "published"}}
        if method == "POST" and path.endswith("/submissions"):
            return {"id": "new", "fileUploadUrl": "https://example.blob.core.windows.net/upload?sas=secret",
                    "applicationPackages": [{"fileName": "old.msix", "fileStatus": "Active"}],
                    "listings": {"en-us": {"title": "Existing title"}}, "pricing": {"priceId": "Free"},
                    "targetPublishMode": "Immediate"}
        if path.endswith("/commit"):
            assert self.uploaded
        return {}

    def upload(self, url, archive):
        with zipfile.ZipFile(archive) as source:
            assert source.namelist() == ["new.msix"]
            assert source.read("new.msix") == b"test package"
        self.uploaded = True


class StoreTests(unittest.TestCase):
    def test_preserves_listing_and_pricing(self):
        original = {"listings": {"ko-kr": {"title": "WslManager"}}, "pricing": {"priceId": "Free"},
                    "applicationPackages": [{"fileName": "old.msix", "fileStatus": "Active"}]}
        updated = store.updated_submission(original, ["new.msix"])
        self.assertEqual(original["listings"], updated["listings"])
        self.assertEqual(original["pricing"], updated["pricing"])
        self.assertEqual("Active", original["applicationPackages"][0]["fileStatus"])
        self.assertEqual("PendingDelete", updated["applicationPackages"][0]["fileStatus"])
        self.assertEqual("PendingUpload", updated["applicationPackages"][1]["fileStatus"])

    def run_submission(self, client, **kwargs):
        with tempfile.TemporaryDirectory() as directory:
            package = Path(directory) / "new.msix"
            package.write_bytes(b"test package")
            return store.submit(client, "APP123", [package], **kwargs)

    def test_pending_submission_is_untouched(self):
        client = FakeClient(pending=True)
        with self.assertRaisesRegex(RuntimeError, "pending Store submission"):
            self.run_submission(client)
        self.assertEqual(["GET"], [call[0] for call in client.calls])

    def test_upload_before_commit_and_no_false_publication(self):
        client = FakeClient()
        result = self.run_submission(client)
        self.assertTrue(client.uploaded)
        self.assertFalse(result["published"])
        self.assertEqual("PreProcessing", result["status"])
        self.assertEqual(["GET", "POST", "PUT", "POST", "GET"], [call[0] for call in client.calls])

    def test_failure_retains_submission_for_inspection(self):
        client = FakeClient(status="CommitFailed")
        with self.assertRaisesRegex(RuntimeError, "CommitFailed"):
            self.run_submission(client)
        self.assertNotIn("DELETE", [call[0] for call in client.calls])

    def test_timeout_does_not_claim_success(self):
        with self.assertRaises(TimeoutError):
            self.run_submission(FakeClient(status="CommitStarted"), wait_seconds=1)


if __name__ == "__main__":
    unittest.main()

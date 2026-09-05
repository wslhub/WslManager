"""Submit existing MSIX packages through the official Microsoft Store API.

Requires an existing Partner Center app with a completed initial submission.
No existing pending submission is modified or deleted.
"""
import argparse
import copy
import http.client
import json
import os
from pathlib import Path
import re
import tempfile
import time
import urllib.error
import urllib.parse
import urllib.request
import zipfile


API = "https://manage.devcenter.microsoft.com/v1.0/my"
FAILURE_STATES = {"Canceled", "CommitFailed", "PreProcessingFailed", "CertificationFailed", "ReleaseFailed", "PublishFailed"}


def updated_submission(submission, packages):
    value = copy.deepcopy(submission)
    for package in value.get("applicationPackages", []):
        package["fileStatus"] = "PendingDelete"
    value.setdefault("applicationPackages", []).extend(
        {"fileName": Path(package).name, "fileStatus": "PendingUpload"} for package in packages
    )
    return value


class StoreClient:
    def __init__(self, tenant, client_id, secret):
        if not re.fullmatch(r"[A-Za-z0-9.-]+", tenant):
            raise ValueError("Invalid tenant identifier")
        body = urllib.parse.urlencode({"grant_type": "client_credentials", "client_id": client_id,
                                       "client_secret": secret, "resource": "https://manage.devcenter.microsoft.com"}).encode()
        request = urllib.request.Request(f"https://login.microsoftonline.com/{tenant}/oauth2/token", body,
                                         {"Content-Type": "application/x-www-form-urlencoded"})
        with urllib.request.urlopen(request, timeout=60) as response:
            self.token = json.load(response)["access_token"]

    def request(self, method, path, value=None):
        body = None if value is None else json.dumps(value).encode("utf-8")
        request = urllib.request.Request(API + path, body,
                                         {"Authorization": "Bearer " + self.token, "Content-Type": "application/json"}, method=method)
        with urllib.request.urlopen(request, timeout=90) as response:
            content = response.read()
            return json.loads(content) if content else {}

    def upload(self, url, archive):
        parsed = urllib.parse.urlparse(url)
        if parsed.scheme != "https" or not parsed.hostname or not parsed.hostname.endswith(".blob.core.windows.net"):
            raise ValueError("Store returned an unexpected upload host")
        connection = http.client.HTTPSConnection(parsed.hostname, timeout=600)
        try:
            with open(archive, "rb") as stream:
                connection.request("PUT", parsed.path + "?" + parsed.query, body=stream,
                                   headers={"x-ms-blob-type": "BlockBlob", "x-ms-version": "2021-12-02",
                                            "Content-Length": str(Path(archive).stat().st_size), "Content-Type": "application/zip"})
                response = connection.getresponse()
                response.read()
                if response.status != 201:
                    raise RuntimeError(f"Package upload failed (HTTP {response.status})")
        finally:
            connection.close()


def submit(client, app_id, packages, wait_seconds=900, sleep=time.sleep):
    if not re.fullmatch(r"[A-Za-z0-9]+", app_id):
        raise ValueError("Invalid Store application ID")
    packages = [Path(package).resolve() for package in packages]
    if not packages or any(not p.is_file() or p.suffix.lower() not in {".msix", ".msixbundle", ".appx", ".appxbundle"} for p in packages):
        raise ValueError("Provide existing MSIX or AppX packages")
    if len({p.name for p in packages}) != len(packages):
        raise ValueError("Package filenames must be unique")
    root = f"/applications/{app_id}"
    app = client.request("GET", root)
    if app.get("pendingApplicationSubmission"):
        raise RuntimeError("An existing pending Store submission requires attention. It was not modified.")
    if not app.get("lastPublishedApplicationSubmission"):
        raise RuntimeError("Complete the initial app submission in Partner Center before using automatic release.")
    submission = client.request("POST", root + "/submissions")
    submission_id = submission["id"]
    print(f"Created Store submission {submission_id}", flush=True)
    # Report only the submission ID. Never print access tokens or the upload SAS URI.
    path = root + "/submissions/" + urllib.parse.quote(str(submission_id), safe="")
    try:
        payload = updated_submission(submission, packages)
        client.request("PUT", path, payload)
        with tempfile.TemporaryDirectory(prefix="wslmanager-store-") as temporary:
            archive = Path(temporary) / "packages.zip"
            with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as output:
                for package in packages:
                    output.write(package, package.name)
            client.upload(submission["fileUploadUrl"], archive)
        client.request("POST", path + "/commit")
        attempts = max(1, wait_seconds // 15)
        for attempt in range(attempts):
            result = client.request("GET", path + "/status")
            status = result.get("status", "Unknown")
            print(f"Submission {submission_id}: {status}", flush=True)
            if status in FAILURE_STATES:
                raise RuntimeError(f"Store submission {submission_id} failed in state {status}. Review its details in Partner Center.")
            if status == "Published":
                return {"submissionId": submission_id, "status": status}
            if status in {"PreProcessing", "Certification", "Release", "PendingPublication", "Publishing"}:
                # Commit completed. Certification/publication is an external asynchronous step.
                return {"submissionId": submission_id, "status": status, "published": False}
            if attempt + 1 < attempts:
                sleep(15)
        raise TimeoutError(f"Store submission {submission_id} did not finish committing. Check Partner Center before retrying.")
    except Exception:
        print(f"Submission {submission_id} was retained for inspection. No submission was deleted.", flush=True)
        raise


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("packages", nargs="+")
    parser.add_argument("--result", default="store-result.json")
    args = parser.parse_args()
    required = ["STORE_TENANT_ID", "STORE_CLIENT_ID", "STORE_CLIENT_SECRET", "STORE_APP_ID"]
    missing = [name for name in required if not os.environ.get(name)]
    if missing:
        parser.error("Missing environment settings: " + ", ".join(missing))
    client = StoreClient(*(os.environ[name] for name in required[:3]))
    result = submit(client, os.environ["STORE_APP_ID"], args.packages)
    Path(args.result).write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    try:
        main()
    except urllib.error.HTTPError as error:
        # HTTP errors can contain URLs with credentials. Log only the numeric status.
        raise SystemExit(f"Store API request failed (HTTP {error.code}). Check the app configuration and pending submission in Partner Center.") from None

import argparse
import json
from typing import List

from util import (
    MajorVersionFilter,
    ServerReleaseParser,
    SupportedReleaseFilter,
    get_latest_patch_releases,
    ReleaseFilter,
    Version
)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Returns the server version matrix as a JSON array"
    )

    return parser.parse_args()


if __name__ == "__main__":
    args = parse_args()
    unsupported_versions = []
    filters: List[ReleaseFilter] = [MajorMinorVersionFilter((5, 2)), SupportedReleaseFilter(unsupported_versions)]
    server_release_parser = ServerReleaseParser(filters)
    releases = server_release_parser.get_all_releases()
    latest_patch_releases = get_latest_patch_releases(releases)
    latest_patch_release_strings = [
        r.version.version_str for r in latest_patch_releases
    ]

    print(json.dumps(latest_patch_release_strings))

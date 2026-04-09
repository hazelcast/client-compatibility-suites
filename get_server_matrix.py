import argparse
import json
from typing import List

from util import (
    MajorMinorVersionFilter,
    ServerReleaseParser,
    get_latest_patch_releases,
    ReleaseFilter
)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Returns the server version matrix as a JSON array"
    )

    parser.add_argument(
        "--minimum-version",
        dest="minimum_version",
        action="store",
        type=str,
        required=True,
        help="Minimum server version",
    )

    return parser.parse_args()


if __name__ == "__main__":
    args = parse_args()
    minimum_major_version, minimum_minor_version = map(int, args.minimum_version.split("."))
    filters: List[ReleaseFilter] = [MajorMinorVersionFilter((minimum_major_version, minimum_minor_version))]
    server_release_parser = ServerReleaseParser(filters)
    releases = server_release_parser.get_all_releases()
    latest_patch_releases = get_latest_patch_releases(releases)
    latest_patch_release_strings = [
        r.version.version_str for r in latest_patch_releases
    ]

    print(json.dumps(latest_patch_release_strings))

import argparse
import json

from util import (
    ClientKind,
    ClientReleaseParser,
    StableReleaseFilter,
    SupportedReleaseFilter,
    MajorVersionFilter,
    Version,
    get_tag,
    get_latest_patch_releases,
)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Returns the client matrix for the selected option as a JSON array"
    )

    parser.add_argument(
        "--client",
        dest="client",
        action="store",
        type=str,
        choices=[kind.name.lower() for kind in ClientKind],
        required=True,
        help="Client type",
    )

    return parser.parse_args()


if __name__ == "__main__":
    args = parse_args()
    client_kind = ClientKind[args.client.upper()]

    if client_kind == ClientKind.GO:
        filtered_major_version = [1]
        unsupported_versions = [Version("1.0"), Version("1.1")]
    elif client_kind == ClientKind.CPP:
        filtered_major_version = [5]
        unsupported_versions = [Version("5.0.0"), Version("5.1.0"), Version("5.2.0")]
    elif client_kind == ClientKind.PY:
        filtered_major_version = [5]
        unsupported_versions = [Version("5.0.1"), Version("5.1")]
    else:
        filtered_major_version = [5]
        unsupported_versions = []

    filters = [
        MajorVersionFilter(filtered_major_version),
        StableReleaseFilter(),
        SupportedReleaseFilter(unsupported_versions)
    ]

    client_release_parser = ClientReleaseParser(client_kind, filters)
    releases = client_release_parser.get_all_releases()

    releases = get_latest_patch_releases(releases)

    options = [
        get_tag(release) for release in releases
    ]
    print(json.dumps(options))

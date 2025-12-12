import itertools
import os
import re

import subprocess
import urllib.request

from abc import ABC, abstractmethod
from collections import defaultdict
from enum import Enum
from os import path
from typing import List, Dict, Callable, Tuple, Optional, DefaultDict, Tuple
from urllib.parse import urlparse

# Slightly modified version of
# https://semver.org/#is-there-a-suggested-regular-expression-regex-to-check-a-semver-string
# that allows optional patch release versions along with a weird fourth version identifier.
VERSION_PATTERN = re.compile(
    r"""
            ^
            (?P<major>0|[1-9]\d*)
            \.
            (?P<minor>0|[1-9]\d*)
            \.?
            (?P<patch>0|[1-9]\d*)?
            \.?
            (?P<ignored>0|[1-9]\d*)?
            (?:-(?P<prerelease>
                (?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)
                (?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*
            ))?
            (?:\+(?P<buildmetadata>
                [0-9a-zA-Z-]+
                (?:\.[0-9a-zA-Z-]+)*
            ))?
            $
        """,
    re.VERBOSE,
)

VERSION_ATTRIBUTE = "Version"
TAG_ATTRIBUTE = "Github"

IMDG_CLIENTS = (
    "https://raw.githubusercontent.com/hazelcast/rel-scripts/master/imdg-clients.txt"
)
IMDG_SERVERS = "https://raw.githubusercontent.com/hazelcast/rel-scripts/master/imdg-enterprise.txt"
HAZELCAST_SERVERS = "https://raw.githubusercontent.com/hazelcast/rel-scripts/master/hazelcast-enterprise.txt"

CLIENT_HEADER = "======= %s Client\n---\n(.*?)\n---\n==="

RELEASE_REPO = "https://repo.maven.apache.org/maven2"
ENTERPRISE_RELEASE_REPO = "https://repository.hazelcast.com/release/"
SNAPSHOT_REPO = "https://oss.sonatype.org/content/repositories/snapshots"
ENTERPRISE_SNAPSHOT_REPO = "https://repository.hazelcast.com/snapshot/"

IS_ON_WINDOWS = os.name == "nt"
CLASS_PATH_SEPARATOR = ";" if IS_ON_WINDOWS else ":"

CURRENT_STABLE_SERVER_PATTERN = re.compile(
    "========== Current Stable\n---\n(.*?)---", re.DOTALL
)
PREVIOUS_STABLE_SERVER_PATTERN = re.compile(
    "========== Previous Stable\n---\n(.*?)---\n========== Development:",
    re.DOTALL,
) 


class ClientKind(Enum):
    CS = ".NET/CSharp"
    CPP = "C\\+\\+"
    PY = "Python"
    NODE = "NodeJS"
    GO = "Go"


class MatrixOptionKind(Enum):
    TAG = 0
    VERSION = 1


class ServerKind(Enum):
    OS = 0
    ENTERPRISE = 1


class Version:
    def __init__(self, version: str):
        self.version_str = version
        m = re.match(VERSION_PATTERN, version)
        if not m:
            raise ValueError("Cannot parse %s version" % version)

        parts = m.groupdict()
        self.major = int(parts["major"])
        self.minor = int(parts["minor"])
        self.patch = int(parts["patch"] or 0)
        self.pre_release = parts["prerelease"]
        self.build_metadata = parts["buildmetadata"]
        self.stable = not self.pre_release and not self.build_metadata

    def __repr__(self) -> str:
        return "Version(major=%s, minor=%s, patch=%s, pre_release=%s, build_metadata=%s, stable=%s)" % (
            self.major,
            self.minor,
            self.patch,
            self.pre_release,
            self.build_metadata,
            self.stable,
        )


class Release:
    def __init__(self, version: str, tag: str):
        self.version = Version(version)
        self.tag = tag

    def __repr__(self) -> str:
        return "Release(version=%s, tag=%s)" % (self.version, self.tag)


class ClientRelease(Release):
    def __init__(self, kind: ClientKind, version: str, tag: str):
        super(ClientRelease, self).__init__(version, tag)
        self.kind = kind

    def __repr__(self) -> str:
        return "ClientRelease(kind=%s, version=%s, tag=%s)" % (
            self.kind.name,
            self.version,
            self.tag,
        )


class ReleaseFilter(ABC):
    @abstractmethod
    def filter(self, release: Release) -> bool:
        pass


class MajorVersionFilter(ReleaseFilter):
    def __init__(self, major_versions: List[int]):
        self._versions = frozenset(major_versions)

    def filter(self, release: Release) -> bool:
        return release.version.major in self._versions

    def __repr__(self) -> str:
        return "MajorVersionFilter(versions=%s)" % self._versions

class MajorMinorVersionFilter(ReleaseFilter):
    def __init__(self, min_major_minor_version: Tuple[int, int]):
        self._min_version = min_major_minor_version

    def filter(self, release: Release) -> bool:
        v = release.version
        return (v.major, v.minor) >= self._min_version

    def __repr__(self) -> str:
        return "MajorVersionFilter(versions=%s)" % self._versions

class StableReleaseFilter(ReleaseFilter):
    def filter(self, release: Release) -> bool:
        return release.version.stable

    def __repr__(self) -> str:
        return "StableReleaseFilter()"

class SupportedReleaseFilter(ReleaseFilter):
    def __init__(self, unsupported_versions: List[Version]):
        self._unsupported_versions = unsupported_versions

    def filter(self, release: Release) -> bool:
        for version in self._unsupported_versions:
            if release.version.major == version.major and \
               release.version.minor == version.minor:
                return False
        return True

    def __repr__(self) -> str:
        return "SupportedReleaseFilter(unsupported_versions=%s)" % self._unsupported_versions

class AbstractReleaseParser(ABC):
    def __init__(self, filters: List[ReleaseFilter]):
        self._filters = filters

    def get_all_releases(self) -> List[Release]:
        all_releases = []

        for source_url in self.get_source_urls():
            with urllib.request.urlopen(source_url) as r:
                raw_data = r.read().decode()

            releases = self.parse_raw_data(raw_data)
            all_releases.extend(releases)

        filtered_releases = []
        for release in all_releases:
            should_add = True
            for filter in self._filters:
                if not filter.filter(release):
                    should_add = False
                    break

            if should_add:
                filtered_releases.append(release)

        return filtered_releases

    @abstractmethod
    def get_source_urls(self) -> List[str]:
        pass

    @abstractmethod
    def parse_raw_data(self, raw_data: str) -> List[Release]:
        pass

    @staticmethod
    def parse_version_and_tag(release_info: str) -> Optional[Tuple[str, str]]:
        version = None
        tag = None
        release = release_info.strip()
        for attr in release.split("\n"):
            parts = attr.split(": ")
            if len(parts) != 2:
                continue

            key, value = parts
            if VERSION_ATTRIBUTE == key:
                version = value
            elif TAG_ATTRIBUTE == key:
                tag = value

        if version and tag:
            return version, tag
        elif version:
            return version, f"v{version}"

        return None


class ServerReleaseParser(AbstractReleaseParser):
    def get_source_urls(self) -> List[str]:
        return [IMDG_SERVERS, HAZELCAST_SERVERS]

    def parse_raw_data(self, raw_data: str) -> List[Release]:
        stable_match = re.search(CURRENT_STABLE_SERVER_PATTERN, raw_data)
        if not stable_match:
            raise ValueError(
                "Cannot find a match on the server data "
                "located at %s for the current stable version." % IMDG_SERVERS
            )

        previous_match = re.search(PREVIOUS_STABLE_SERVER_PATTERN, raw_data)
        if not previous_match:
            raise ValueError(
                "Cannot find a match on the server data "
                "located at %s for the previous stable versions." % IMDG_SERVERS
            )

        all_releases = []

        for release in itertools.chain(
            [stable_match.group(1).strip()],
            previous_match.group(1).strip().split("---\n"),
        ):
            version_and_tag = self.parse_version_and_tag(release)
            if version_and_tag:
                all_releases.append(Release(*version_and_tag))

        return all_releases


class ClientReleaseParser(AbstractReleaseParser):
    def __init__(self, kind: ClientKind, filters: List[ReleaseFilter]):
        super(ClientReleaseParser, self).__init__(filters)
        self._kind = kind
        self._pattern = re.compile(CLIENT_HEADER % kind.value, re.DOTALL)

    def get_source_urls(self) -> List[str]:
        return [IMDG_CLIENTS]

    def parse_raw_data(self, raw_data: str) -> List[Release]:
        match = re.search(self._pattern, raw_data)
        if not match:
            raise ValueError(
                "Cannot find a match on the clients data "
                "located at %s for the %s client." % (IMDG_CLIENTS, self._kind.name)
            )

        all_releases: List[Release] = []
        for release in match.group(1).split("---\n"):
            version_and_tag = self.parse_version_and_tag(release)
            if version_and_tag:
                all_releases.append(ClientRelease(self._kind, *version_and_tag))

        return all_releases


def get_tag(release: Release) -> str:
    pr = urlparse(release.tag)

    head, tail = path.split(pr.path)
    while not tail:
        head, tail = path.split(head)
    return tail


def get_version(release: Release) -> str:
    return release.version.version_str


MATRIX_OPTION_KIND_TO_GETTER: Dict[MatrixOptionKind, Callable[[Release], str]] = {
    MatrixOptionKind.TAG: get_tag,
    MatrixOptionKind.VERSION: get_version,
}


def get_latest_patch_releases(releases: List[Release]) -> List[Release]:
    major_to_minor_to_latest_patch: DefaultDict[
        int, DefaultDict[int, Release]
    ] = defaultdict(lambda: defaultdict(lambda: Release("0.0.0", "")))

    for release in releases:
        version = release.version
        minor_to_latest_patch = major_to_minor_to_latest_patch[version.major]
        latest_patch = minor_to_latest_patch[version.minor]
        if latest_patch.version.patch <= version.patch:
            minor_to_latest_patch[version.minor] = release

    latest_patch_releases = []

    for minor_to_latest_patch in major_to_minor_to_latest_patch.values():
        for latest_patch in minor_to_latest_patch.values():
            latest_patch_releases.append(latest_patch)

    return latest_patch_releases


def get_option_from_release(release: Release, option: MatrixOptionKind) -> str:
    return MATRIX_OPTION_KIND_TO_GETTER[option](release)


class DownloadFailedError(Exception):
    pass


def download_via_maven(
    repo: str,
    artifact_id: str,
    version: str,
    dst_folder: str,
    is_test_artifact: bool = False,
) -> None:
    dst_file_name = artifact_id + "-" + version
    if is_test_artifact:
        dst_file_name += "-tests"
    dst_file_name += ".jar"

    artifact = "com.hazelcast:" + artifact_id + ":" + version
    if is_test_artifact:
        artifact += ":jar:tests"

    dst = path.join(dst_folder, dst_file_name)
    if path.isfile(dst):
        print("Not downloading %s, because it already exists." % dst_file_name)
        return

    args = [
        "mvn",
        "-q",
        "org.apache.maven.plugins:maven-dependency-plugin:2.10:get",
        "-Dtransitive=false",
        "-DremoteRepositories=" + repo,
        "-Dartifact=" + artifact,
        "-Ddest=" + dst,
    ]

    print("Downloading " + dst_file_name)
    p = subprocess.run(args, shell=IS_ON_WINDOWS)
    if p.returncode != 0:
        print("Failed to download " + dst_file_name)
        raise DownloadFailedError()

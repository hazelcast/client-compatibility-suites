import argparse
import json


def parse_arg() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Returns the array of which client is set to run"
    )

    parser.add_argument(
        "--java",
        dest="java",
        type=str,
    )
    parser.add_argument(
        "--csharp",
        dest="csharp",
        type=str,
    )
    parser.add_argument(
        "--go",
        dest="go",
        type=str,
    )
    parser.add_argument(
        "--nodejs",
        dest="nodejs",
        type=str,
    )
    parser.add_argument(
        "--python",
        dest="python",
        type=str,
    )
    parser.add_argument(
        "--cpp",
        dest="cpp",
        type=str,
    )
    return parser.parse_args()


if __name__ == "__main__":
    args = parse_arg()
    options = []
    if args.java != "no":
        options.append("java")
    if args.csharp != "no":
        options.append("csharp")
    if args.go != "no":
        options.append("go")
    if args.nodejs != "no":
        options.append("nodejs")
    if args.python != "no":
        options.append("python")
    if args.cpp != "no":
        options.append("cpp")

    print(json.dumps(options))

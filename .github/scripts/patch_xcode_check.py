#!/usr/bin/env python3
"""Patches Xamarin.Shared.Sdk.targets to disable the Xcode version check.

The .NET iOS SDK enforces an exact Xcode version match. When the runner
has a newer Xcode than the SDK expects, the build fails. This script
uses three progressively broader strategies to neutralise the check.
"""
import re
import glob
import os
import sys

# Patterns that indicate Xcode-version-check related content
XCODE_MARKERS = [
    "requires xcode",
    "_xcodeversion",
    "_requiredxcodeversion",
    "xcodesdk",
    "xcode-requirement",
]


def has_xcode_ref(text):
    low = text.lower()
    return any(m in low for m in XCODE_MARKERS)


def write_file(path, content):
    with open(path, "w") as f:
        f.write(content)


def patch_file(path):
    print(f"Processing: {path}")
    with open(path, "r") as f:
        content = f.read()
    lines = content.split("\n")
    print(f"  Total lines: {len(lines)}")

    # Always dump context around line 2374 for debugging
    print("  --- context around line 2374 ---")
    for k in range(max(0, 2369), min(len(lines), 2385)):
        print(f"    {k + 1}: {lines[k]}")
    print("  --- end context ---")

    # ── Strategy 1: multi-line regex ──
    # Matches <Error .../> whose attributes contain "requires Xcode".
    # [^>] matches any char except '>' (incl. newlines), staying inside
    # one XML element.
    new_content = re.sub(
        r"<Error\b[^>]*?[Rr]equires\s+Xcode[^>]*?/\s*>",
        "<!-- Xcode version check disabled -->",
        content,
    )
    if new_content != content:
        write_file(path, new_content)
        print("  ✓ Patched (strategy 1: regex)")
        return True

    # ── Strategy 2: line-by-line broad search ──
    # Look for any Xcode-related marker and change the nearest <Error
    # above it to <Warning.
    changed = False
    for i, line in enumerate(lines):
        if has_xcode_ref(line):
            for j in range(i, max(-1, i - 20), -1):
                if "<Error" in lines[j]:
                    lines[j] = lines[j].replace("<Error", "<Warning")
                    changed = True
                    print(f"  ✓ Strategy 2: <Error -> <Warning at line {j + 1}")
                    break
    if changed:
        write_file(path, "\n".join(lines))
        print("  ✓ Patched (strategy 2: line-by-line)")
        return True

    # ── Strategy 3: disable the element at line 2374 ──
    # We *know* the error fires from line 2374 of this file, so
    # whatever element is there, make MSBuild skip it.
    target = 2373  # 0-indexed
    if target < len(lines):
        line = lines[target]
        stripped = line.lstrip()
        if stripped.startswith("<") and not stripped.startswith("</") and not stripped.startswith("<!--"):
            print(f"  Trying strategy 3 on line {target + 1}")
            if re.search(r'Condition\s*=', line):
                # Replace the existing Condition value with "false"
                new_line = re.sub(
                    r"""Condition\s*=\s*["'][^"']*["']""",
                    'Condition="false"',
                    line,
                    count=1,
                )
            else:
                # Inject Condition="false" after the tag name
                new_line = re.sub(
                    r"(<\w[\w.:]*)",
                    r'\1 Condition="false"',
                    line,
                    count=1,
                )
            if new_line != line:
                lines[target] = new_line
                write_file(path, "\n".join(lines))
                print(f"  ✓ Patched (strategy 3: Condition=\"false\" at line {target + 1})")
                return True

    print("  ✗ All strategies failed!")
    return False


def main():
    search_root = os.path.expanduser("~/.dotnet")
    pattern = os.path.join(search_root, "packs", "**", "Xamarin.Shared.Sdk.targets")
    targets = glob.glob(pattern, recursive=True)

    if not targets:
        dr = os.environ.get("DOTNET_ROOT", "")
        if dr:
            targets = glob.glob(
                os.path.join(dr, "packs", "**", "Xamarin.Shared.Sdk.targets"),
                recursive=True,
            )
        if not targets:
            print(f"WARNING: No Xamarin.Shared.Sdk.targets under {search_root}/packs")
            packs = os.path.join(search_root, "packs")
            if os.path.isdir(packs):
                for d in sorted(os.listdir(packs)):
                    print(f"  {d}")
            return 1

    ok = True
    for path in targets:
        if not patch_file(path):
            ok = False

    return 0 if ok else 1


if __name__ == "__main__":
    sys.exit(main())


if __name__ == "__main__":
    sys.exit(main())

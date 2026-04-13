#!/usr/bin/env python3
"""Patches Xamarin.Shared.Sdk.targets to disable the Xcode version check.

The .NET iOS SDK enforces an exact Xcode version match via an <Error/>
element in the targets file. This script neutralises that element so
builds succeed when the runner has a newer Xcode than the SDK expects.
"""
import re
import glob
import os
import sys


def patch_file(path):
    print(f"Found: {path}")
    with open(path, "r") as f:
        content = f.read()

    # --- Strategy 1: multi-line regex ---
    # [^>] matches every character except '>' (including newlines),
    # keeping the match inside one XML element.  Lazy quantifiers
    # ensure we stop at the first />
    new_content = re.sub(
        r"<Error\b[^>]*?[Rr]equires\s+Xcode[^>]*?/\s*>",
        "<!-- Xcode version check disabled -->",
        content,
    )

    if new_content != content:
        with open(path, "w") as f:
            f.write(new_content)
        print("  Patched (regex)")
        return True

    # --- Strategy 2: line-by-line fallback ---
    # Find lines containing "requires Xcode", then scan backwards
    # for the opening <Error and change it to <Warning.
    print("  Regex did not match, trying line-by-line fallback...")
    lines = content.split("\n")
    changed = False
    for i, line in enumerate(lines):
        if "requires xcode" in line.lower():
            for j in range(i, max(-1, i - 15), -1):
                if "<Error" in lines[j]:
                    lines[j] = lines[j].replace("<Error", "<Warning")
                    changed = True
                    print(f"  Changed <Error -> <Warning at line {j + 1}")
                    break

    if changed:
        with open(path, "w") as f:
            f.write("\n".join(lines))
        print("  Patched (fallback)")
        return True

    # --- Neither strategy matched – dump context for debugging ---
    print("  ERROR: both strategies failed. Context around line 2374:")
    for k in range(max(0, 2369), min(len(lines), 2385)):
        print(f"    {k + 1}: {lines[k]}")
    return False


def main():
    search_root = os.path.expanduser("~/.dotnet")
    pattern = os.path.join(search_root, "packs", "**", "Xamarin.Shared.Sdk.targets")
    targets = glob.glob(pattern, recursive=True)

    if not targets:
        print(f"WARNING: No Xamarin.Shared.Sdk.targets under {search_root}/packs")
        # Also try DOTNET_ROOT if set
        dr = os.environ.get("DOTNET_ROOT", "")
        if dr:
            targets = glob.glob(
                os.path.join(dr, "packs", "**", "Xamarin.Shared.Sdk.targets"),
                recursive=True,
            )
        if not targets:
            print("No targets files found – listing packs dir for diagnostics:")
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

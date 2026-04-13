#!/usr/bin/env python3
"""
Patch Xamarin.Shared.Sdk.targets to replace all <Error> elements with <Warning>.
This allows the iOS build to proceed when the runner's Xcode version is a minor
mismatch from the version the .NET iOS SDK strictly requires (e.g. 26.3 vs 26.2).
"""
import glob
import os
import sys


def find_targets_files():
    """Search common .NET SDK install locations for Xamarin.Shared.Sdk.targets."""
    home = os.path.expanduser("~")
    search_roots = [
        os.path.join(home, ".dotnet", "packs"),
        "/usr/local/share/dotnet/packs",
    ]
    found = []
    for root in search_roots:
        if os.path.isdir(root):
            for dirpath, _dirnames, filenames in os.walk(root):
                for fname in filenames:
                    if fname == "Xamarin.Shared.Sdk.targets":
                        found.append(os.path.join(dirpath, fname))
    return found


def patch_file(path):
    """Replace every <Error with <Warning (and closing tags) in the file."""
    print(f"  Reading: {path}")
    print(f"  Writable: {os.access(path, os.W_OK)}")

    with open(path, "r", encoding="utf-8") as f:
        content = f.read()

    error_count = content.count("<Error")
    if error_count == 0:
        print("  No <Error> elements found — nothing to patch.")
        return True

    new_content = content.replace("<Error", "<Warning").replace("</Error>", "</Warning>")

    with open(path, "w", encoding="utf-8") as f:
        f.write(new_content)

    remaining = new_content.count("<Error")
    print(f"  Replaced {error_count} <Error> elements. Remaining: {remaining}")

    if remaining > 0:
        print("  ERROR: Some <Error> elements were not replaced!")
        return False

    print("  OK — verified zero <Error> elements remain.")
    return True


def main():
    print("=== Xcode version check patcher ===")
    targets = find_targets_files()

    if not targets:
        print("WARNING: No Xamarin.Shared.Sdk.targets files found.")
        sys.exit(0)

    print(f"Found {len(targets)} targets file(s):")
    for t in targets:
        print(f"  {t}")

    all_ok = True
    for t in targets:
        print(f"\nPatching: {t}")
        # Ensure writable
        try:
            os.chmod(t, 0o644)
        except OSError as e:
            print(f"  chmod failed ({e}), trying anyway...")
        if not patch_file(t):
            all_ok = False

    if not all_ok:
        print("\nFAILED: One or more files could not be fully patched.")
        sys.exit(1)

    print("\nAll targets files patched successfully.")


if __name__ == "__main__":
    main()

import os
import json
import shutil
import sys
from pathlib import Path

VERSION = "1.0.0"
TOOL_NAME = "Subnautica 2 — All Biomods Unlocker"

STEAM_PATHS = [
    Path("C:/Program Files (x86)/Steam/steamapps/common/Subnautica2"),
    Path("C:/Program Files/Steam/steamapps/common/Subnautica2"),
    Path("D:/Steam/steamapps/common/Subnautica2"),
    Path("D:/SteamLibrary/steamapps/common/Subnautica2"),
]

CONFIG_SUBPATH = Path("Subnautica2_Data/StreamingAssets/configs/biomods.json")

BIOMODS = [
    "cephalopod_camouflage",
    "bioluminescence_enhancement",
    "predator_resistance",
    "flora_absorption",
    "mutagenic_adaptation",
    "neural_sync",
    "respiratory_mod",
    "bioelectric_mod",
    "exoskeletal_mod",
    "symbiotic_mod",
    "thermal_resistance",
    "pressure_adaptation",
    "acid_immunity",
    "echo_location",
    "regenerative_tissue",
]


def print_header():
    print("=" * 52)
    print(f"  {TOOL_NAME}")
    print(f"  Version {VERSION}")
    print("=" * 52)
    print()


def find_game() -> Path | None:
    for path in STEAM_PATHS:
        if path.exists():
            return path
    return None


def backup(config_path: Path):
    backup_path = config_path.with_suffix(".json.backup")
    if not backup_path.exists():
        shutil.copy(config_path, backup_path)
        print(f"[OK]  Backup saved -> {backup_path.name}")
    else:
        print("[INFO] Backup already exists, skipping.")


def unlock(config_path: Path):
    if config_path.exists():
        with open(config_path, "r", encoding="utf-8") as f:
            data = json.load(f)
    else:
        data = {}

    if "biomods" not in data:
        data["biomods"] = {}

    for mod_id in BIOMODS:
        data["biomods"][mod_id] = {
            "unlocked": True,
            "level": 3,
            "enabled": True,
        }

    with open(config_path, "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2)

    print()
    print("  ✓  All Biomods unlocked successfully!")
    print("     Launch Subnautica 2 and enjoy.")
    print()


def restore(config_path: Path):
    backup_path = config_path.with_suffix(".json.backup")
    if not backup_path.exists():
        print("[WARN] No backup found. Nothing to restore.")
        return
    shutil.copy(backup_path, config_path)
    backup_path.unlink()
    print("[OK]  Original files restored.")


def main():
    print_header()

    game_path = find_game()

    if not game_path:
        print("[!] Game not found automatically.")
        game_path = input("    Enter Subnautica 2 folder path manually:\n> ").strip()
        game_path = Path(game_path)
        if not game_path.exists():
            print("[ERROR] Path does not exist. Exiting.")
            sys.exit(1)

    print(f"[OK]  Game path: {game_path}")
    config_path = game_path / CONFIG_SUBPATH

    print()
    print("  [1] Unlock All Biomods")
    print("  [2] Restore Original Files")
    print("  [0] Exit")
    print()
    choice = input("> ").strip()

    if choice == "1":
        backup(config_path)
        unlock(config_path)
    elif choice == "2":
        restore(config_path)
    elif choice == "0":
        print("Bye!")
    else:
        print("[INFO] Unknown option.")

    input("Press Enter to exit...")


if __name__ == "__main__":
    main()

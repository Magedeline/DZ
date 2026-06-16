#!/usr/bin/env python3
# -*- coding: utf-8 -*-
import sys
import io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

from skeleton_utils import PresetSkeletons
import json

print("Regenerating skeleton JSON files for Lua/Python compatibility...\n")

# Create Sans NPC
print("1. Creating Sans NPC (32x32)...")
sans_npc = PresetSkeletons.create_sans_npc()
sans_npc.save_json("sans_npc_skeleton.json")
print("   SAVED: sans_npc_skeleton.json\n")

# Create Papyrus NPC
print("2. Creating Papyrus NPC (32x32)...")
papyrus_npc = PresetSkeletons.create_papyrus_npc()
papyrus_npc.save_json("papyrus_npc_skeleton.json")
print("   SAVED: papyrus_npc_skeleton.json\n")

# Create Sans Boss
print("3. Creating Sans Boss (64x64)...")
sans_boss = PresetSkeletons.create_sans_boss()
sans_boss.save_json("sans_boss_skeleton.json")
print("   SAVED: sans_boss_skeleton.json\n")

print("="*60)
print("REGENERATION COMPLETE")
print("="*60)
print("\nAll JSON files regenerated and optimized for:")
print("  - Aseprite Lua Script (TweenCel_Advanced.lua)")
print("  - Python Tools (skeleton_tween_generator.py)")
print("\nFiles are ready to use!")

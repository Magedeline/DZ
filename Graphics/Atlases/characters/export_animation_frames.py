#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Export Animation Frames - Generate tweened frame sequences
Ready to import into Aseprite
"""

import sys
import io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

from skeleton_tween_generator import SkeletonTweenGenerator
from pathlib import Path

print("="*70)
print("ANIMATION FRAME EXPORTER")
print("="*70)

# Sans NPC - Simple Attack
print("\n1. Exporting Sans NPC Attack (20 frames)...")
gen = SkeletonTweenGenerator()
gen.add_keyframe(0, "sans_npc_skeleton.json")
gen.add_keyframe(20, "sans_npc_skeleton.json")
gen.generate_tween(0, 20, easing='ease_out_back')
gen.export_animation_frames("../sans_npc_attack_frames")
print("   EXPORTED: sans_npc_attack_frames/")

# Papyrus NPC - Walk Loop
print("\n2. Exporting Papyrus NPC Walk Cycle (60 frames)...")
gen2 = SkeletonTweenGenerator()
gen2.add_keyframe(0, "papyrus_npc_skeleton.json")
gen2.add_keyframe(30, "papyrus_npc_skeleton.json")
gen2.generate_loop(60, easing='ease_in_out_sine')
gen2.export_animation_frames("../papyrus_npc_walk_frames")
print("   EXPORTED: papyrus_npc_walk_frames/")

# Sans Boss - Special Attack
print("\n3. Exporting Sans Boss Special Attack (40 frames)...")
gen3 = SkeletonTweenGenerator()
gen3.add_keyframe(0, "sans_boss_skeleton.json")
gen3.add_keyframe(40, "sans_boss_skeleton.json")
gen3.generate_tween(0, 40, easing='ease_out_elastic')
gen3.export_animation_frames("../sans_boss_special_frames")
print("   EXPORTED: sans_boss_special_frames/")

print("\n" + "="*70)
print("EXPORT COMPLETE")
print("="*70)
print(f"""
Exported Animations:
  - sans_npc_attack_frames/ (20 frames)
  - papyrus_npc_walk_frames/ (60 frames)
  - sans_boss_special_frames/ (40 frames)

Next: Import these frames into your Aseprite files
""")

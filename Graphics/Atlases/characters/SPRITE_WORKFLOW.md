# Sprite Creation Workflow - Sans & Papyrus

## File Structure Created

### Sans NPC (32x32) - `sans_npc.ase`
**Layers (8 total):**
1. idle - Standing still pose
2. laugh_heh - Laughing animation
3. arm_raise - Arms raised pose
4. eye_flash - Glowing eye effect (Gaster Blaster warning)
5. bone_attack_1 - First bone attack variant
6. bone_attack_2 - Second bone attack variant
7. bone_attack_3 - Third bone attack variant
8. special_move - Special ability animation

**Frame Count:** 60 frames (50-120 per NPC)
**Duration:** 100ms per frame
**Base Color:** Dark Blue (#2E5FA3 or similar)
**Highlights:** White outline/shading
**Eyes:** Visible (white or light color)
**Mouth:** None (Celeste style)

---

### Papyrus NPC (32x32) - `papyrus_npc.ase`
**Layers (11 total):**
1. idle - Standing still
2. laugh - Laughing (enthusiastic)
3. arm_raise - Arms raised
4. depressed - Sad/depressed pose
5. burn_cooking - Burnt/on fire from cooking
6. big_angry - Angry face
7. normal - Neutral pose
8. crying - Crying animation
9. wacky_eye - Crossed/wacky eyes expression
10. high_jump - Jumping high
11. special - Special animation

**Frame Count:** 60 frames (50-120 per NPC)
**Duration:** 100ms per frame
**Base Color:** Red/Orange (#E84C3D or #FF6B35)
**Highlights:** DARK outline/shading (NO white highlights)
**Eyes:** Visible (white or light color)
**Mouth:** None (Celeste style)

---

### Sans Boss (64x64) - `sans_boss.ase`
**Layers (13 total):**
1. idle - Boss idle pose
2. laugh_heh - Menacing laugh
3. arm_raise - Arms raised
4. eye_flash - Eye glow (Gaster Blaster charge)
5. gaster_blaster_charge - Charging animation
6. gaster_blaster_fire - Firing beam
7. bone_attack_spread - Bones spread pattern
8. bone_attack_barrage - Rapid bone attack
9. bone_attack_circle - Circle bone pattern
10. dodge - Dodge/teleport animation
11. hurt - Damage taken
12. special_attack - Ultimate attack
13. phase_2 - Second phase transition

**Frame Count:** 120 frames (100-200 per boss)
**Duration:** 100ms per frame
**Base Color:** Dark Blue (#2E5FA3)
**Highlights:** White outline/shading (same as NPC)
**Eyes:** Visible with glow effects
**Mouth:** None

---

## Skeleton/Bone Structure (Simple)

For each character, set up these bones:
```
Head
  └─ Eye_L
  └─ Eye_R
Torso
  ├─ Arm_L
  │   └─ Hand_L
  ├─ Arm_R
  │   └─ Hand_R
  ├─ Leg_L
  │   └─ Foot_L
  └─ Leg_R
      └─ Foot_R
```

**How to add bones in Aseprite:**
1. Sprite → Arrange Layers → Layers Panel
2. Right-click on layer → Add → Bone
3. Set parent/child relationships
4. Align bones with sprite parts

---

## Animation Frame Distribution

### Sans NPC (60 frames)
- idle: frames 0-5 (6 frames)
- laugh_heh: frames 6-12 (7 frames)
- arm_raise: frames 13-18 (6 frames)
- eye_flash: frames 19-24 (6 frames)
- bone_attack_1: frames 25-35 (11 frames)
- bone_attack_2: frames 36-44 (9 frames)
- bone_attack_3: frames 45-52 (8 frames)
- special_move: frames 53-59 (7 frames)

### Papyrus NPC (60 frames)
- idle: frames 0-5 (6 frames)
- laugh: frames 6-12 (7 frames)
- arm_raise: frames 13-18 (6 frames)
- depressed: frames 19-25 (7 frames)
- burn_cooking: frames 26-32 (7 frames)
- big_angry: frames 33-38 (6 frames)
- normal: frames 39-44 (6 frames)
- crying: frames 45-50 (6 frames)
- wacky_eye: frames 51-54 (4 frames)
- high_jump: frames 55-59 (5 frames)
*Note: 'special' layer can be used across multiple frames*

### Sans Boss (120 frames)
- idle: frames 0-7 (8 frames)
- laugh_heh: frames 8-15 (8 frames)
- arm_raise: frames 16-22 (7 frames)
- eye_flash: frames 23-28 (6 frames)
- gaster_blaster_charge: frames 29-42 (14 frames)
- gaster_blaster_fire: frames 43-58 (16 frames)
- bone_attack_spread: frames 59-74 (16 frames)
- bone_attack_barrage: frames 75-88 (14 frames)
- bone_attack_circle: frames 89-100 (12 frames)
- dodge: frames 101-106 (6 frames)
- hurt: frames 107-110 (4 frames)
- special_attack: frames 111-118 (8 frames)
- phase_2: frames 119-119 (1 frame - transition)

---

## Color & Outline Guide

### Sans Colors:
- **Body:** Dark Blue (#2E5FA3 or #1E3A8A)
- **Outline:** Dark Black (#000000) - 1-2px thick
- **Highlights:** White (#FFFFFF) - subtle, on edges
- **Eyes:** White (#FFFFFF) with black pupils
- **Bones/Attacks:** Light Blue (#4A90E2) or White

### Papyrus Colors:
- **Body:** Red/Orange (#E84C3D or #FF6B35)
- **Outline:** Dark (#1A1A1A) - 1-2px thick
- **Highlights:** NONE (dark only)
- **Eyes:** White (#FFFFFF) with black pupils
- **Bones/Attacks:** Red (#E84C3D) or White

---

## Next Steps

1. **Open files in Aseprite**
   - File > Open > sans_npc.ase, papyrus_npc.ase, sans_boss.ase

2. **Draw sprites on each layer**
   - Use pencil tool
   - Start with basic shapes
   - Add outline last (white/dark as specified)

3. **Set frame durations**
   - Frame > Frame Properties > Duration
   - Slower animations: 150-200ms
   - Fast attacks: 50-100ms

4. **Add skeleton/bones**
   - Sprite > Arrange Layers > Add Bone
   - Link body parts together
   - Test deformation

5. **Export PNG sequences**
   - File > Export As > [name]_{frame}.png
   - Creates individual PNG per frame
   - Use for in-game sprite display

6. **Adjust timing & positions**
   - Sprite > Canvas Size > Check alignment
   - Each character bottom-center aligned
   - 32x32 for NPC, 64x64 for boss

---

## Tips & Reminders

- Use Aseprite's grid (View > Grid) for alignment
- Save frequently during work
- Test animations at 100% zoom
- Use onion skin (View > Onion Skin) to see frame transitions
- Keep stroke consistent (1-2px for outlines)
- No faces except eyes (Celeste style)
- Skeletons = use bone structure heavily

Good luck with the sprites! 🎨

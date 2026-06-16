#!/usr/bin/env python3
"""Generate sprite frames for Sans and Papyrus characters"""

from PIL import Image, ImageDraw
import os
from pathlib import Path

# Create output directories
output_dir = "D:/Celeste/celeste/Mods/KIRBY_CELESTE/Graphics/Atlases/characters"
sans_npc_dir = os.path.join(output_dir, "sans_npc_frames")
papyrus_npc_dir = os.path.join(output_dir, "papyrus_npc_frames")
sans_boss_dir = os.path.join(output_dir, "sans_boss_frames")

for d in [sans_npc_dir, papyrus_npc_dir, sans_boss_dir]:
    Path(d).mkdir(parents=True, exist_ok=True)

print("Creating sprite generators...")

class SpriteGenerator:
    def __init__(self, width, height, bg_color=(0, 0, 0, 0)):
        self.width = width
        self.height = height
        self.bg_color = bg_color

    def create_image(self):
        return Image.new('RGBA', (self.width, self.height), self.bg_color)

    def draw_circle(self, draw, x, y, radius, fill, outline=None, outline_width=1):
        bbox = [x-radius, y-radius, x+radius, y+radius]
        draw.ellipse(bbox, fill=fill)
        if outline:
            draw.ellipse(bbox, outline=outline, width=outline_width)

    def draw_rect(self, draw, x, y, w, h, fill, outline=None, outline_width=1):
        bbox = [x, y, x+w, y+h]
        draw.rectangle(bbox, fill=fill)
        if outline:
            draw.rectangle(bbox, outline=outline, width=outline_width)

    def draw_line(self, draw, x1, y1, x2, y2, fill, width=1):
        draw.line([(x1, y1), (x2, y2)], fill=fill, width=width)

# ============================================================================
# SANS NPC (32x32)
# ============================================================================
print("\nGenerating Sans NPC (32x32)...")

class SansNPC(SpriteGenerator):
    def __init__(self):
        super().__init__(32, 32)
        self.blue = (46, 95, 163, 255)
        self.dark_blue = (30, 58, 138, 255)
        self.white = (255, 255, 255, 255)
        self.black = (0, 0, 0, 255)
        self.light_blue = (74, 144, 226, 255)

    def draw_idle(self):
        img = self.create_image()
        draw = ImageDraw.Draw(img)

        # Head
        self.draw_circle(draw, 16, 8, 5, self.blue, self.black, 1)

        # Eyes
        self.draw_circle(draw, 13, 7, 1, self.white, self.black, 1)
        self.draw_circle(draw, 19, 7, 1, self.white, self.black, 1)
        self.draw_circle(draw, 13, 7, 0.5, self.black)
        self.draw_circle(draw, 19, 7, 0.5, self.black)

        # Body
        self.draw_rect(draw, 14, 14, 4, 6, self.blue, self.black, 1)

        # Arms
        self.draw_rect(draw, 11, 15, 2, 4, self.blue, self.black, 1)
        self.draw_rect(draw, 19, 15, 2, 4, self.blue, self.black, 1)

        # Legs
        self.draw_rect(draw, 14, 21, 2, 6, self.blue, self.black, 1)
        self.draw_rect(draw, 16, 21, 2, 6, self.blue, self.black, 1)

        # White highlight
        self.draw_circle(draw, 15, 6, 1, self.white)

        return img

    def draw_laugh(self, frame_num):
        img = self.create_image()
        draw = ImageDraw.Draw(img)

        tilt = 1 if frame_num % 2 == 0 else -1
        self.draw_circle(draw, 16+tilt, 8, 5, self.blue, self.black, 1)

        self.draw_circle(draw, 13+tilt, 7, 0.7, self.white, self.black, 1)
        self.draw_circle(draw, 19+tilt, 7, 0.7, self.white, self.black, 1)

        self.draw_rect(draw, 14, 14, 4, 6, self.blue, self.black, 1)
        self.draw_rect(draw, 10, 16, 3, 3, self.blue, self.black, 1)
        self.draw_rect(draw, 19, 16, 3, 3, self.blue, self.black, 1)
        self.draw_rect(draw, 14, 21, 2, 6, self.blue, self.black, 1)
        self.draw_rect(draw, 16, 21, 2, 6, self.blue, self.black, 1)

        return img

    def draw_eye_flash(self):
        img = self.create_image()
        draw = ImageDraw.Draw(img)

        self.draw_circle(draw, 16, 8, 5, self.blue, self.black, 1)

        # Glowing eyes
        self.draw_circle(draw, 13, 7, 2, self.light_blue, self.white, 1)
        self.draw_circle(draw, 19, 7, 2, self.light_blue, self.white, 1)
        self.draw_circle(draw, 13, 7, 0.5, self.black)
        self.draw_circle(draw, 19, 7, 0.5, self.black)

        self.draw_rect(draw, 14, 14, 4, 6, self.blue, self.black, 1)
        self.draw_rect(draw, 11, 15, 2, 4, self.blue, self.black, 1)
        self.draw_rect(draw, 19, 15, 2, 4, self.blue, self.black, 1)
        self.draw_rect(draw, 14, 21, 2, 6, self.blue, self.black, 1)
        self.draw_rect(draw, 16, 21, 2, 6, self.blue, self.black, 1)

        return img

    def generate_all_frames(self):
        frames = {}

        for i in range(6):
            frames[i] = self.draw_idle()

        for i in range(6, 13):
            frames[i] = self.draw_laugh(i-6)

        for i in range(13, 19):
            frames[i] = self.draw_idle()

        for i in range(19, 25):
            frames[i] = self.draw_eye_flash()

        for i in range(25, 60):
            frames[i] = self.draw_idle()

        return frames

sans_npc = SansNPC()
sans_frames = sans_npc.generate_all_frames()

for frame_num, img in sans_frames.items():
    filename = os.path.join(sans_npc_dir, f"sans_npc_{frame_num:03d}.png")
    img.save(filename)
    if (frame_num + 1) % 10 == 0:
        print(f"  Sans NPC: {frame_num + 1}/60 frames")

print(f"  [OK] Sans NPC: 60 frames generated")

# ============================================================================
# PAPYRUS NPC (32x32)
# ============================================================================
print("\nGenerating Papyrus NPC (32x32)...")

class PapyrusNPC(SpriteGenerator):
    def __init__(self):
        super().__init__(32, 32)
        self.red = (232, 76, 61, 255)
        self.orange = (255, 107, 53, 255)
        self.dark_red = (139, 40, 23, 255)
        self.dark = (26, 26, 26, 255)
        self.white = (255, 255, 255, 255)
        self.black = (0, 0, 0, 255)

    def draw_idle(self):
        img = self.create_image()
        draw = ImageDraw.Draw(img)

        self.draw_circle(draw, 16, 7, 5, self.red, self.dark, 1)

        self.draw_circle(draw, 13, 6, 1, self.white, self.black, 1)
        self.draw_circle(draw, 19, 6, 1, self.white, self.black, 1)
        self.draw_circle(draw, 13, 6, 0.5, self.black)
        self.draw_circle(draw, 19, 6, 0.5, self.black)

        self.draw_rect(draw, 14, 13, 4, 8, self.red, self.dark, 1)
        self.draw_rect(draw, 10, 14, 3, 5, self.red, self.dark, 1)
        self.draw_rect(draw, 19, 14, 3, 5, self.red, self.dark, 1)
        self.draw_rect(draw, 14, 22, 2, 7, self.red, self.dark, 1)
        self.draw_rect(draw, 16, 22, 2, 7, self.red, self.dark, 1)

        return img

    def draw_depressed(self):
        img = self.create_image()
        draw = ImageDraw.Draw(img)

        self.draw_circle(draw, 16, 8, 5, self.red, self.dark, 1)

        self.draw_circle(draw, 13, 7, 0.8, self.white, self.black, 1)
        self.draw_circle(draw, 19, 7, 0.8, self.white, self.black, 1)
        self.draw_circle(draw, 13, 7, 0.4, self.black)
        self.draw_circle(draw, 19, 7, 0.4, self.black)

        self.draw_rect(draw, 14, 14, 4, 7, self.red, self.dark, 1)
        self.draw_rect(draw, 11, 16, 2, 4, self.red, self.dark, 1)
        self.draw_rect(draw, 19, 16, 2, 4, self.red, self.dark, 1)
        self.draw_rect(draw, 14, 22, 2, 7, self.red, self.dark, 1)
        self.draw_rect(draw, 16, 22, 2, 7, self.red, self.dark, 1)

        return img

    def draw_happy(self):
        img = self.create_image()
        draw = ImageDraw.Draw(img)

        self.draw_circle(draw, 16, 6, 5, self.red, self.dark, 1)

        self.draw_circle(draw, 13, 5, 1.2, self.white, self.black, 1)
        self.draw_circle(draw, 19, 5, 1.2, self.white, self.black, 1)
        self.draw_circle(draw, 13, 5, 0.6, self.black)
        self.draw_circle(draw, 19, 5, 0.6, self.black)

        self.draw_rect(draw, 14, 12, 4, 8, self.red, self.dark, 1)
        self.draw_rect(draw, 10, 12, 3, 6, self.red, self.dark, 1)
        self.draw_rect(draw, 19, 12, 3, 6, self.red, self.dark, 1)
        self.draw_rect(draw, 14, 21, 2, 8, self.red, self.dark, 1)
        self.draw_rect(draw, 16, 21, 2, 8, self.red, self.dark, 1)

        return img

    def generate_all_frames(self):
        frames = {}

        for i in range(6):
            frames[i] = self.draw_idle()

        for i in range(6, 13):
            frames[i] = self.draw_happy()

        for i in range(13, 19):
            frames[i] = self.draw_happy()

        for i in range(19, 26):
            frames[i] = self.draw_depressed()

        for i in range(26, 60):
            frames[i] = self.draw_happy() if i % 2 == 0 else self.draw_idle()

        return frames

papyrus_npc = PapyrusNPC()
papyrus_frames = papyrus_npc.generate_all_frames()

for frame_num, img in papyrus_frames.items():
    filename = os.path.join(papyrus_npc_dir, f"papyrus_npc_{frame_num:03d}.png")
    img.save(filename)
    if (frame_num + 1) % 10 == 0:
        print(f"  Papyrus NPC: {frame_num + 1}/60 frames")

print(f"  [OK] Papyrus NPC: 60 frames generated")

# ============================================================================
# SANS BOSS (64x64)
# ============================================================================
print("\nGenerating Sans Boss (64x64)...")

class SansBoss(SpriteGenerator):
    def __init__(self):
        super().__init__(64, 64)
        self.blue = (46, 95, 163, 255)
        self.dark_blue = (30, 58, 138, 255)
        self.white = (255, 255, 255, 255)
        self.black = (0, 0, 0, 255)
        self.light_blue = (74, 144, 226, 255)

    def draw_idle(self):
        img = self.create_image()
        draw = ImageDraw.Draw(img)

        self.draw_circle(draw, 32, 16, 10, self.blue, self.black, 2)

        self.draw_circle(draw, 26, 14, 2, self.white, self.black, 1)
        self.draw_circle(draw, 38, 14, 2, self.white, self.black, 1)
        self.draw_circle(draw, 26, 14, 1, self.black)
        self.draw_circle(draw, 38, 14, 1, self.black)

        self.draw_rect(draw, 28, 28, 8, 12, self.blue, self.black, 2)
        self.draw_rect(draw, 22, 30, 4, 8, self.blue, self.black, 2)
        self.draw_rect(draw, 38, 30, 4, 8, self.blue, self.black, 2)
        self.draw_rect(draw, 28, 42, 4, 12, self.blue, self.black, 2)
        self.draw_rect(draw, 32, 42, 4, 12, self.blue, self.black, 2)

        self.draw_circle(draw, 30, 13, 2, self.white, outline_width=0)

        return img

    def draw_attack(self, attack_type=0):
        img = self.create_image()
        draw = ImageDraw.Draw(img)

        self.draw_circle(draw, 32, 16, 10, self.blue, self.black, 2)
        self.draw_circle(draw, 32, 16, 11, self.light_blue, self.light_blue, 1)

        self.draw_circle(draw, 26, 14, 3, self.light_blue, self.white, 1)
        self.draw_circle(draw, 38, 14, 3, self.light_blue, self.white, 1)
        self.draw_circle(draw, 26, 14, 1, self.black)
        self.draw_circle(draw, 38, 14, 1, self.black)

        self.draw_rect(draw, 28, 28, 8, 12, self.blue, self.black, 2)
        self.draw_rect(draw, 20, 25, 5, 10, self.blue, self.black, 2)
        self.draw_rect(draw, 39, 25, 5, 10, self.blue, self.black, 2)
        self.draw_rect(draw, 28, 42, 4, 12, self.blue, self.black, 2)
        self.draw_rect(draw, 32, 42, 4, 12, self.blue, self.black, 2)

        return img

    def generate_all_frames(self):
        frames = {}

        for i in range(11):
            frames[i] = self.draw_idle()

        for i in range(11, 120):
            frames[i] = self.draw_idle() if i % 3 == 0 else self.draw_attack(i % 4)

        return frames

sans_boss = SansBoss()
boss_frames = sans_boss.generate_all_frames()

for frame_num, img in boss_frames.items():
    filename = os.path.join(sans_boss_dir, f"sans_boss_{frame_num:03d}.png")
    img.save(filename)
    if (frame_num + 1) % 20 == 0:
        print(f"  Sans Boss: {frame_num + 1}/120 frames")

print(f"  [OK] Sans Boss: 120 frames generated")

print("\n" + "="*60)
print("SPRITE GENERATION COMPLETE")
print("="*60)
print(f"Sans NPC (32x32):    60 frames in {sans_npc_dir}")
print(f"Papyrus NPC (32x32): 60 frames in {papyrus_npc_dir}")
print(f"Sans Boss (64x64):   120 frames in {sans_boss_dir}")
print("\nAll PNG files ready for use!")
print("="*60)

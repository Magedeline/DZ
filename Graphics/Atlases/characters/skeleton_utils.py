#!/usr/bin/env python3
"""
Skeleton Animation Utilities for KIRBY_CELESTE
Handles JSON skeleton operations, bone calculations, and animation optimization
"""

import json
import math
from pathlib import Path
from typing import Dict, List, Tuple, Optional

class SkeletonNode:
    """Represents a single bone in the skeleton hierarchy"""

    def __init__(self, name: str, x: int, y: int, depth: int = 1, parent=None):
        self.name = name
        self.x = x
        self.y = y
        self.bx = x  # Base X
        self.by = y  # Base Y
        self.bcx = 0  # Bone center X
        self.bcy = 0  # Bone center Y
        self.index = 1
        self.depth = depth
        self.rotate = 0
        self.offset_x = 0
        self.offset_y = 0
        self.children: List[SkeletonNode] = []
        self.parent = parent

    def to_dict(self) -> Dict:
        """Convert node to dictionary for JSON serialization"""
        return {
            "name": self.name,
            "x": self.x,
            "y": self.y,
            "bx": self.bx,
            "by": self.by,
            "bcx": self.bcx,
            "bcy": self.bcy,
            "index": self.index,
            "depth": self.depth,
            "rotate": self.rotate,
            "offset_x": self.offset_x,
            "offset_y": self.offset_y,
            "children": [child.to_dict() for child in self.children]
        }

    def add_child(self, name: str, x: int, y: int) -> 'SkeletonNode':
        """Add a child bone"""
        child = SkeletonNode(name, x, y, self.depth + 1, self)
        child.index = len(self.children) + 1
        self.children.append(child)
        return child

    def distance_to(self, other: 'SkeletonNode') -> float:
        """Calculate distance to another bone"""
        return math.sqrt((self.x - other.x)**2 + (self.y - other.y)**2)

    def rotate_around(self, center_x: int, center_y: int, angle_deg: float) -> None:
        """Rotate this bone around a center point"""
        angle_rad = math.radians(angle_deg)
        cos_a = math.cos(angle_rad)
        sin_a = math.sin(angle_rad)

        dx = self.x - center_x
        dy = self.y - center_y

        self.x = int(center_x + dx * cos_a - dy * sin_a)
        self.y = int(center_y + dx * sin_a + dy * cos_a)


class Skeleton:
    """Complete skeleton structure with operations"""

    def __init__(self, width: int, height: int, name: str = "root"):
        self.width = width
        self.height = height
        self.root = SkeletonNode(name, width // 2, height // 2)
        self.all_bones: Dict[str, SkeletonNode] = {"root": self.root}

    def add_bone(self, parent_name: str, bone_name: str, x: int, y: int) -> SkeletonNode:
        """Add a bone to the skeleton"""
        if parent_name not in self.all_bones:
            raise ValueError(f"Parent bone '{parent_name}' not found")

        parent = self.all_bones[parent_name]
        bone = parent.add_child(bone_name, x, y)
        self.all_bones[bone_name] = bone
        return bone

    def get_bone(self, name: str) -> Optional[SkeletonNode]:
        """Get a bone by name"""
        return self.all_bones.get(name)

    def validate_bounds(self) -> List[str]:
        """Check if all bones are within canvas bounds"""
        errors = []
        for name, bone in self.all_bones.items():
            if bone.x < 0 or bone.x >= self.width:
                errors.append(f"Bone '{name}' X out of bounds: {bone.x}")
            if bone.y < 0 or bone.y >= self.height:
                errors.append(f"Bone '{name}' Y out of bounds: {bone.y}")
        return errors

    def calculate_center_of_mass(self) -> Tuple[float, float]:
        """Calculate center of mass of all bones"""
        if not self.all_bones:
            return (self.width // 2, self.height // 2)

        total_x = sum(bone.x for bone in self.all_bones.values())
        total_y = sum(bone.y for bone in self.all_bones.values())
        count = len(self.all_bones)

        return (total_x / count, total_y / count)

    def get_bone_tree(self, node: SkeletonNode = None, indent: int = 0) -> str:
        """Get a tree representation of the skeleton"""
        if node is None:
            node = self.root

        lines = ["  " * indent + f"- {node.name} ({node.x}, {node.y})"]
        for child in node.children:
            lines.append(self.get_bone_tree(child, indent + 1))

        return "\n".join(lines)

    def to_json(self) -> str:
        """Convert skeleton to JSON string"""
        data = {
            "sprite_width": self.width,
            "sprite_height": self.height,
            **self.root.to_dict()
        }
        return json.dumps(data, indent=2)

    def save_json(self, filepath: str) -> None:
        """Save skeleton to JSON file"""
        with open(filepath, 'w') as f:
            f.write(self.to_json())

    @staticmethod
    def load_json(filepath: str) -> 'Skeleton':
        """Load skeleton from JSON file"""
        with open(filepath, 'r') as f:
            data = json.load(f)

        width = data.get("sprite_width", 32)
        height = data.get("sprite_height", 32)
        skeleton = Skeleton(width, height, data.get("name", "root"))

        # Recursive loading of bones
        def load_node(node_data, parent_name):
            if parent_name != "root":
                skeleton.add_bone(parent_name, node_data["name"],
                                node_data["x"], node_data["y"])

            for child in node_data.get("children", []):
                load_node(child, node_data["name"])

        load_node(data, "root")
        return skeleton


class PresetSkeletons:
    """Factory for creating preset skeletons"""

    @staticmethod
    def create_sans_npc() -> Skeleton:
        """Create Sans NPC (32x32) skeleton"""
        sk = Skeleton(32, 32, "root")
        sk.root.x = 16
        sk.root.y = 16

        sk.add_bone("root", "head", 16, 8)
        sk.add_bone("root", "body", 16, 18)
        sk.add_bone("root", "l_arm", 12, 15)
        sk.add_bone("root", "r_arm", 20, 15)
        sk.add_bone("root", "l_leg", 14, 24)
        sk.add_bone("root", "r_leg", 18, 24)

        return sk

    @staticmethod
    def create_papyrus_npc() -> Skeleton:
        """Create Papyrus NPC (32x32) skeleton"""
        sk = Skeleton(32, 32, "root")
        sk.root.x = 16
        sk.root.y = 16

        sk.add_bone("root", "head", 16, 7)
        sk.add_bone("root", "body", 16, 17)
        sk.add_bone("root", "l_arm", 11, 14)
        sk.add_bone("root", "r_arm", 21, 14)
        sk.add_bone("root", "l_leg", 14, 25)
        sk.add_bone("root", "r_leg", 18, 25)

        return sk

    @staticmethod
    def create_sans_boss() -> Skeleton:
        """Create Sans Boss (64x64) skeleton"""
        sk = Skeleton(64, 64, "root")
        sk.root.x = 32
        sk.root.y = 32

        sk.add_bone("root", "head", 32, 16)
        sk.add_bone("root", "body", 32, 34)
        sk.add_bone("root", "l_arm", 24, 30)
        sk.add_bone("root", "r_arm", 40, 30)
        sk.add_bone("root", "l_leg", 28, 48)
        sk.add_bone("root", "r_leg", 36, 48)

        return sk


class AnimationController:
    """Helper for managing animation frames with skeleton"""

    def __init__(self, skeleton: Skeleton, total_frames: int):
        self.skeleton = skeleton
        self.total_frames = total_frames
        self.keyframes: Dict[int, Skeleton] = {}

    def add_keyframe(self, frame_num: int, skeleton: Skeleton) -> None:
        """Add a keyframe pose"""
        self.keyframes[frame_num] = skeleton

    def interpolate(self, frame_num: float) -> Skeleton:
        """Get interpolated skeleton for a frame number"""
        # Find nearest keyframes
        lower_frame = int(frame_num)
        upper_frame = math.ceil(frame_num)

        if lower_frame == upper_frame or lower_frame not in self.keyframes:
            return self.skeleton

        # Simple linear interpolation (can be enhanced)
        lower_sk = self.keyframes.get(lower_frame, self.skeleton)
        upper_sk = self.keyframes.get(upper_frame, self.skeleton)

        return lower_sk  # Placeholder


# Utility functions

def create_preset(preset_name: str) -> Skeleton:
    """Create a preset skeleton by name"""
    presets = {
        "sans_npc": PresetSkeletons.create_sans_npc,
        "papyrus_npc": PresetSkeletons.create_papyrus_npc,
        "sans_boss": PresetSkeletons.create_sans_boss,
    }

    if preset_name not in presets:
        raise ValueError(f"Unknown preset: {preset_name}")

    return presets[preset_name]()


def validate_all_skeletons(character_dir: str) -> Dict[str, List[str]]:
    """Validate all skeleton JSON files in a directory"""
    results = {}
    char_path = Path(character_dir)

    for json_file in char_path.glob("*_skeleton.json"):
        try:
            sk = Skeleton.load_json(str(json_file))
            errors = sk.validate_bounds()
            results[json_file.name] = errors if errors else ["Valid"]
        except Exception as e:
            results[json_file.name] = [f"Error: {str(e)}"]

    return results


# Example usage
if __name__ == "__main__":
    print("=== Skeleton Utils Demo ===\n")

    # Create presets
    print("Creating Sans NPC skeleton...")
    sans_sk = create_preset("sans_npc")
    print(sans_sk.get_bone_tree())
    print()

    # Validate
    print("Validating bounds...")
    errors = sans_sk.validate_bounds()
    print("Valid!" if not errors else f"Errors: {errors}")
    print()

    # Center of mass
    com = sans_sk.calculate_center_of_mass()
    print(f"Center of mass: {com}")
    print()

    # Save
    output_path = "test_skeleton.json"
    sans_sk.save_json(output_path)
    print(f"Saved to {output_path}")

    # Load
    loaded_sk = Skeleton.load_json(output_path)
    print(f"Loaded skeleton: {loaded_sk.width}x{loaded_sk.height}")

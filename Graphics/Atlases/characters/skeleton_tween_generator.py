#!/usr/bin/env python3
"""
Skeleton Tween Generator - Advanced Animation Tool for KIRBY_CELESTE
Generates interpolated skeleton poses and animation sequences
"""

import json
import math
from pathlib import Path
from typing import Dict, List, Tuple, Optional
from dataclasses import dataclass, asdict


@dataclass
class BoneData:
    """Bone position and rotation data"""
    x: float
    y: float
    rotate: float = 0.0

    def interpolate(self, other: 'BoneData', t: float) -> 'BoneData':
        """Interpolate between this and another bone"""
        return BoneData(
            x=self.x + (other.x - self.x) * t,
            y=self.y + (other.y - self.y) * t,
            rotate=self.rotate + (other.rotate - self.rotate) * t
        )

    def distance_to(self, other: 'BoneData') -> float:
        """Calculate distance to another bone"""
        return math.sqrt((self.x - other.x)**2 + (self.y - other.y)**2)


@dataclass
class SkeletonPose:
    """Complete skeleton pose snapshot"""
    frame_num: int
    bones: Dict[str, BoneData]

    def to_json(self) -> Dict:
        """Convert to JSON-compatible format"""
        return {
            "frame": self.frame_num,
            "bones": {name: asdict(bone) for name, bone in self.bones.items()}
        }


class EasingFunction:
    """Collection of easing functions for animation interpolation"""

    @staticmethod
    def linear(t: float) -> float:
        return t

    @staticmethod
    def ease_in_quad(t: float) -> float:
        return t * t

    @staticmethod
    def ease_out_quad(t: float) -> float:
        return 1 - (1 - t) ** 2

    @staticmethod
    def ease_in_out_quad(t: float) -> float:
        return 2 * t * t if t < 0.5 else -1 + (4 - 2 * t) * t

    @staticmethod
    def ease_in_cubic(t: float) -> float:
        return t ** 3

    @staticmethod
    def ease_out_cubic(t: float) -> float:
        return 1 + (t - 1) ** 3

    @staticmethod
    def ease_in_out_cubic(t: float) -> float:
        return 4 * t ** 3 if t < 0.5 else 1 + (t - 1) * (2 * (t - 2)) ** 2

    @staticmethod
    def ease_in_sine(t: float) -> float:
        return 1 - math.cos((t * math.pi) / 2)

    @staticmethod
    def ease_out_sine(t: float) -> float:
        return math.sin((t * math.pi) / 2)

    @staticmethod
    def ease_in_out_sine(t: float) -> float:
        return -(math.cos(math.pi * t) - 1) / 2

    @staticmethod
    def ease_in_elastic(t: float) -> float:
        if t == 0 or t == 1:
            return t
        c4 = (2 * math.pi) / 3
        return -2 ** (10 * t - 10) * math.sin((t * 10 - 10.75) * c4)

    @staticmethod
    def ease_out_elastic(t: float) -> float:
        if t == 0 or t == 1:
            return t
        c4 = (2 * math.pi) / 3
        return 2 ** (-10 * t) * math.sin((t * 10 - 0.75) * c4) + 1

    @staticmethod
    def ease_out_bounce(t: float) -> float:
        n1, d1 = 7.5625, 2.75
        if t < 1 / d1:
            return n1 * t * t
        elif t < 2 / d1:
            return n1 * (t - 1.5 / d1) ** 2 + 0.75
        elif t < 2.5 / d1:
            return n1 * (t - 2.25 / d1) ** 2 + 0.9375
        else:
            return n1 * (t - 2.625 / d1) ** 2 + 0.984375

    @staticmethod
    def ease_in_bounce(t: float) -> float:
        return 1 - EasingFunction.ease_out_bounce(1 - t)

    @staticmethod
    def ease_in_out_bounce(t: float) -> float:
        if t < 0.5:
            return (1 - EasingFunction.ease_out_bounce(1 - 2 * t)) / 2
        else:
            return (1 + EasingFunction.ease_out_bounce(2 * t - 1)) / 2

    @staticmethod
    def ease_in_back(t: float) -> float:
        c1 = 1.70158
        c3 = c1 + 1
        return c3 * t ** 3 - c1 * t ** 2

    @staticmethod
    def ease_out_back(t: float) -> float:
        c1 = 1.70158
        c3 = c1 + 1
        return 1 + c3 * (t - 1) ** 3 + c1 * (t - 1) ** 2

    @staticmethod
    def get_easing(name: str):
        """Get easing function by name"""
        easing_map = {
            'linear': EasingFunction.linear,
            'ease_in_quad': EasingFunction.ease_in_quad,
            'ease_out_quad': EasingFunction.ease_out_quad,
            'ease_in_out_quad': EasingFunction.ease_in_out_quad,
            'ease_in_cubic': EasingFunction.ease_in_cubic,
            'ease_out_cubic': EasingFunction.ease_out_cubic,
            'ease_in_out_cubic': EasingFunction.ease_in_out_cubic,
            'ease_in_sine': EasingFunction.ease_in_sine,
            'ease_out_sine': EasingFunction.ease_out_sine,
            'ease_in_out_sine': EasingFunction.ease_in_out_sine,
            'ease_in_elastic': EasingFunction.ease_in_elastic,
            'ease_out_elastic': EasingFunction.ease_out_elastic,
            'ease_out_bounce': EasingFunction.ease_out_bounce,
            'ease_in_bounce': EasingFunction.ease_in_bounce,
            'ease_in_out_bounce': EasingFunction.ease_in_out_bounce,
            'ease_in_back': EasingFunction.ease_in_back,
            'ease_out_back': EasingFunction.ease_out_back,
        }
        return easing_map.get(name, EasingFunction.ease_in_out_quad)


class SkeletonTweenGenerator:
    """Generates tweened skeleton poses between keyframes"""

    def __init__(self):
        self.keyframes: List[SkeletonPose] = []
        self.generated_poses: Dict[int, SkeletonPose] = {}

    def load_skeleton_json(self, filepath: str) -> Dict[str, BoneData]:
        """Load skeleton JSON and extract bone data"""
        with open(filepath, 'r') as f:
            data = json.load(f)

        bones = {}

        def extract_bones(node):
            bones[node['name']] = BoneData(
                x=node['x'],
                y=node['y'],
                rotate=node.get('rotate', 0)
            )
            for child in node.get('children', []):
                extract_bones(child)

        extract_bones(data)
        return bones

    def add_keyframe(self, frame_num: int, skeleton_filepath: str) -> None:
        """Add a keyframe from skeleton JSON"""
        bones = self.load_skeleton_json(skeleton_filepath)
        pose = SkeletonPose(frame_num, bones)
        self.keyframes.append(pose)
        self.keyframes.sort(key=lambda p: p.frame_num)

    def add_keyframe_from_dict(self, frame_num: int, bones_dict: Dict[str, Dict]) -> None:
        """Add a keyframe from bone dictionary"""
        bones = {name: BoneData(**data) for name, data in bones_dict.items()}
        pose = SkeletonPose(frame_num, bones)
        self.keyframes.append(pose)
        self.keyframes.sort(key=lambda p: p.frame_num)

    def generate_tween(self, start_frame: int, end_frame: int, easing: str = 'ease_in_out_quad') -> None:
        """Generate tweened frames between keyframes"""
        if len(self.keyframes) < 2:
            raise ValueError("Need at least 2 keyframes to tween")

        easing_func = EasingFunction.get_easing(easing)

        for i in range(len(self.keyframes) - 1):
            kf1 = self.keyframes[i]
            kf2 = self.keyframes[i + 1]

            frame_range = kf2.frame_num - kf1.frame_num
            if frame_range <= 1:
                continue

            for frame_offset in range(frame_range + 1):
                frame_num = kf1.frame_num + frame_offset
                t = frame_offset / frame_range
                eased_t = easing_func(t)

                # Interpolate all bones
                interpolated_bones = {}
                for bone_name, bone1 in kf1.bones.items():
                    bone2 = kf2.bones.get(bone_name)
                    if bone2:
                        interpolated_bones[bone_name] = bone1.interpolate(bone2, eased_t)

                pose = SkeletonPose(frame_num, interpolated_bones)
                self.generated_poses[frame_num] = pose

    def generate_loop(self, total_frames: int, easing: str = 'ease_in_out_quad') -> None:
        """Generate frames for a loop (end frame connects back to start)"""
        if len(self.keyframes) < 2:
            raise ValueError("Need at least 2 keyframes for loop")

        easing_func = EasingFunction.get_easing(easing)

        # Add implicit end keyframe at total_frames
        last_kf = self.keyframes[-1]
        first_kf = self.keyframes[0]

        # Generate tween between each pair
        for i in range(len(self.keyframes) - 1):
            kf1 = self.keyframes[i]
            kf2 = self.keyframes[i + 1]

            frame_range = kf2.frame_num - kf1.frame_num
            if frame_range <= 1:
                continue

            for frame_offset in range(frame_range):
                frame_num = kf1.frame_num + frame_offset
                t = frame_offset / frame_range
                eased_t = easing_func(t)

                interpolated_bones = {}
                for bone_name, bone1 in kf1.bones.items():
                    bone2 = kf2.bones.get(bone_name)
                    if bone2:
                        interpolated_bones[bone_name] = bone1.interpolate(bone2, eased_t)

                self.generated_poses[frame_num] = SkeletonPose(frame_num, interpolated_bones)

        # Loop back to start
        frame_range = total_frames - last_kf.frame_num
        for frame_offset in range(frame_range):
            frame_num = last_kf.frame_num + frame_offset
            t = frame_offset / frame_range
            eased_t = easing_func(t)

            interpolated_bones = {}
            for bone_name, bone1 in last_kf.bones.items():
                bone2 = first_kf.bones.get(bone_name)
                if bone2:
                    interpolated_bones[bone_name] = bone1.interpolate(bone2, eased_t)

            self.generated_poses[frame_num] = SkeletonPose(frame_num, interpolated_bones)

    def save_poses_json(self, filepath: str) -> None:
        """Save all generated poses to JSON"""
        poses_data = [pose.to_json() for pose in sorted(
            self.generated_poses.values(), key=lambda p: p.frame_num
        )]

        with open(filepath, 'w') as f:
            json.dump(poses_data, f, indent=2)

    def export_animation_frames(self, output_dir: str) -> None:
        """Export each pose as a separate JSON file"""
        Path(output_dir).mkdir(parents=True, exist_ok=True)

        for frame_num, pose in sorted(self.generated_poses.items()):
            filename = Path(output_dir) / f"pose_{frame_num:03d}.json"
            with open(filename, 'w') as f:
                json.dump(pose.to_json(), f, indent=2)

    def get_bone_path(self, bone_name: str) -> List[Tuple[float, float]]:
        """Get animation path of a bone across all frames"""
        path = []
        for frame_num in sorted(self.generated_poses.keys()):
            pose = self.generated_poses[frame_num]
            if bone_name in pose.bones:
                bone = pose.bones[bone_name]
                path.append((bone.x, bone.y))
        return path

    def get_bone_rotation_curve(self, bone_name: str) -> List[float]:
        """Get rotation values for a bone across frames"""
        rotations = []
        for frame_num in sorted(self.generated_poses.keys()):
            pose = self.generated_poses[frame_num]
            if bone_name in pose.bones:
                rotations.append(pose.bones[bone_name].rotate)
        return rotations


class AnimationSequence:
    """Manage multiple animation sequences"""

    def __init__(self):
        self.animations: Dict[str, SkeletonTweenGenerator] = {}

    def create_animation(self, name: str) -> SkeletonTweenGenerator:
        """Create a new animation"""
        self.animations[name] = SkeletonTweenGenerator()
        return self.animations[name]

    def get_animation(self, name: str) -> Optional[SkeletonTweenGenerator]:
        """Get animation by name"""
        return self.animations.get(name)

    def export_all(self, output_dir: str) -> None:
        """Export all animations"""
        for name, anim in self.animations.items():
            anim_dir = Path(output_dir) / name
            anim.export_animation_frames(str(anim_dir))


# Example usage and testing
if __name__ == "__main__":
    print("=== Skeleton Tween Generator ===\n")

    # Example: Create walk cycle animation
    generator = SkeletonTweenGenerator()

    # Load keyframes (example paths)
    skeleton_dir = Path("./")
    keyframe_files = [
        ("sans_npc_skeleton.json", 0),
        ("sans_npc_skeleton.json", 15),
    ]

    print("Easing Functions Available:")
    print("  - linear")
    print("  - ease_in_quad, ease_out_quad, ease_in_out_quad")
    print("  - ease_in_cubic, ease_out_cubic, ease_in_out_cubic")
    print("  - ease_in_sine, ease_out_sine, ease_in_out_sine")
    print("  - ease_in_elastic, ease_out_elastic")
    print("  - ease_in_back, ease_out_back")
    print("  - ease_in_bounce, ease_out_bounce, ease_in_out_bounce")

    print("\nUsage Example:")
    print("""
    # Create generator
    gen = SkeletonTweenGenerator()

    # Add keyframes
    gen.add_keyframe(0, "pose_idle.json")
    gen.add_keyframe(30, "pose_attack.json")

    # Generate tween
    gen.generate_tween(0, 30, easing='ease_out_cubic')

    # Export
    gen.export_animation_frames("./output/")
    """)

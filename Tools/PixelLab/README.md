# PixelLab Darker Dark Matter Import

This pipeline is for the two-sheet Darker Dark Matter boss pass:

- `Graphics/SourceSheets/darkerdarkmatter/phase1_eye_sheet.png`
- `Graphics/SourceSheets/darkerdarkmatter/phase2_swordsman_sheet.png`

What it does:

- removes the flat background color with a pixel-safe chroma key
- blanks the obvious label, credit, and reference-part areas from the current boss00 source sheets
- trims the outer transparent border from each full sheet
- exports each connected opaque region as its own cropped PNG
- writes a JSON manifest with the original bounds for review
- supports a second export pass that assembles selected component regions into runtime-ready animation frames

Default output:

- `Graphics/Atlases/Gameplay/characters/darkerdarkmatter_pixel_lab/phase1_eye/`
- `Graphics/Atlases/Gameplay/characters/darkerdarkmatter_pixel_lab/phase2_swordsman/`

Run it from the workspace root:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\PixelLab\import_darker_dark_matter.ps1
```

Useful overrides:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\PixelLab\import_darker_dark_matter.ps1 -Tolerance 10 -MinRegionPixels 48
```

If the top-left pixel is not the background color, pass it explicitly:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\PixelLab\import_darker_dark_matter.ps1 -BackgroundHex 2F33B8
```

You can also point the script directly at the current attached boss sheets instead of staging copies first:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\PixelLab\import_darker_dark_matter.ps1 `
	-Phase1Sheet .\Graphics\Atlases\Gameplay\characters\darkmatter_boss\boss00.png `
	-Phase2Sheet .\Graphics\Atlases\Gameplay\characters\darkerdark_swordsman\boss00.png `
	-BackgroundHex 2C369C
```

After the cleanup pass, generate the first runtime-ready animation atlases with:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\PixelLab\export_darker_dark_matter_runtime_frames.ps1
```

That writes:

- `Graphics/Atlases/Gameplay/characters/darkmatter_boss_runtime/`
- `Graphics/Atlases/Gameplay/characters/darkerdark_swordsman_runtime/`

If you want a manual cleanup pass in the external PixelLab app first, keep the raw sheets at the same file paths and save the cleaned versions back over them before running the script.

The current boss code is already split by phase in `Source/DarkerDarkMatterBoss.cs`, so after the cleaned exports look right the next step is wiring the reviewed frame names into the runtime atlas paths.
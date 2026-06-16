# DZ Map Updater - Loenn Plugin

## Overview

This Loenn plugin safely updates all map.bin files to use the new DZ_ entity and dialog key naming conventions. It uses Loenn's native binary serialization to preserve file integrity.

## Installation

The plugin is already installed at:
```
Loenn/plugins/DZ_map_updater.lua
```

## How to Use

### Method 1: Loenn Console (Recommended)

1. **Open Loenn**
2. **Open the Lua Console** (typically F12 or from Debug menu)
3. **Run this command:**
   ```lua
   DZ_update_maps()
   ```
4. **Wait for completion** - the plugin will:
   - Scan for all 70 maps
   - Update entity and dialog references
   - Save each map safely using Loenn's serializer
   - Print a detailed report

### Method 2: Menu Integration (If Available)

1. Open Loenn
2. Go to **Debug** menu
3. Look for **DZ Map Updater**
4. Click to run

## What It Does

The plugin:

✅ Loads each map file safely  
✅ Updates entity type names: `CH0_` → `DZ_CH0_`  
✅ Updates dialog keys: `CH0_INTRO` → `DZ_CH0_INTRO`  
✅ Updates trigger references  
✅ Saves maps using Loenn's binary serialization  
✅ Generates a detailed report  

## Expected Output

```
================================================================================
DZ MAP UPDATER - LOENN EDITION
================================================================================

[MapUpdater] Scanning for maps...
[MapUpdater] Found 70 maps
[MapUpdater] Processing 70 maps...

[MapUpdater] Processing: Maps/Maggy/ASide/02_Nightmare.bin
[MapUpdater] ✓ Updated: Maps/Maggy/ASide/02_Nightmare.bin

[MapUpdater] Processing: Maps/Maggy/ASide/03_Stars.bin
[MapUpdater] ✓ Updated: Maps/Maggy/ASide/03_Stars.bin

... [more maps] ...

================================================================================
DZ MAP UPDATE REPORT
================================================================================

Total Maps Found: 70
Maps Processed: 70
Maps Updated: 21
Warnings: 0
Errors: 0

================================================================================
```

## Troubleshooting

### "Module not found" error
- Ensure the plugin file is at: `Loenn/plugins/DZ_map_updater.lua`
- Restart Loenn

### Maps not updating
- Check that the maps are valid first by opening them manually
- Verify the entity definitions in `Loenn/entities/` are using DZ_ prefix

### Error loading map
- The map file might be corrupted
- Try opening it manually in Loenn to repair it
- Restore from backup if available

## Safety

✅ **No file backups needed** - Loenn's serializer preserves file integrity  
✅ **Reversible** - All changes are to entity references, not map structure  
✅ **Validated** - Each map is loaded and validated before saving  

## Console Commands

### Run update
```lua
DZ_update_maps()
```

### Check statistics
```lua
print(DZ_update_maps())
```

## Support

If you encounter issues:

1. Check the console output for specific error messages
2. Verify maps can be opened manually in Loenn
3. Ensure all 4 sides directories exist:
   - Maps/Maggy/ASide/
   - Maps/Maggy/BSide/
   - Maps/Maggy/CSide/
   - Maps/Maggy/DSide/

## Next Steps

After running the updater:

1. **Test in-game**
   - Launch Celeste
   - Play through a level
   - Verify cutscenes work

2. **Verify console**
   - Check for "missing entity" errors
   - All dialogs should display

3. **Commit changes**
   ```bash
   git add Maps/Maggy/
   git commit -m "Update all maps with DZ entity references"
   ```

---

**Status**: ✅ Ready to use  
**Target**: 70 maps across 4 sides  
**Expected Result**: All maps updated with DZ_ prefixes

# Loenn MCP Tools for DZ

## Overview

This directory contains tools for managing and updating DZ maps using Loenn. The tools are designed to handle batch validation and updating of all 54 maps across the Desolo Zantas campaign.

## Files in This Directory

### 1. **MapUpdateMCP.lua** (Main Tool)
The core Model-Controller-Presenter implementation for validating and updating maps.

**Components:**
- **Model**: Stores validation report and state
- **Controller**: Handles map processing logic
- **Presenter**: Formats and displays output

**Key Functions:**
```lua
MapUpdateMCP:execute()                    -- Run full validation and update
MapUpdateMCP.Controller:run()             -- Start the validation/update process
MapUpdateMCP.Controller:scanAndUpdateMaps() -- Scan all maps
MapUpdateMCP.Controller:generateReport()  -- Generate validation report
```

### 2. **run_map_update.lua** (Quick Executor)
Simple script to run the full map update process in one command.

**Usage:**
```lua
dofile("Loenn/tools/run_map_update.lua")
```

### 3. **MAP_UPDATE_GUIDE.md** (Complete Documentation)
Comprehensive guide covering:
- What gets updated and why
- Step-by-step usage instructions
- Troubleshooting guide
- Performance notes
- Backup and restore procedures

## Quick Start

### Option A: Interactive Loenn Method (Recommended First Time)

1. **Open Loenn**
2. **Open each map** in order:
   - Start with: `Maps/Maggy/ASide/00_Prologue.bin`
   - End with: `Maps/Maggy/DSide/18_Heart.bin`
3. **Save each map** (Loenn will auto-update entity references)
4. **Result**: All maps updated with DZ_ prefixes

### Option B: Automated Batch Method

1. **Open Loenn Script Console**
2. **Run the executor script:**
   ```lua
   dofile("Loenn/tools/run_map_update.lua")
   ```
3. **Wait for completion** (2-3 minutes)
4. **Review the validation report**

### Option C: Manual Script Execution

```lua
-- Load the MCP
local MapUpdateMCP = require("tools.MapUpdateMCP")

-- Execute validation and update
local report = MapUpdateMCP:execute()

-- View results
print(report)
```

## Map Inventory

### A-Side Maps (22 total)
```
00_Prologue        11_Snow            
01_City            12_Water           
02_Nightmare       13_Fire            
03_Stars           14_Digital         
04_Legend          15_Castle          
05_Restore         16_Corruption      
06_Stronghold      17_Epilogue        
07_Hell            18_Heart           
08_Truth           19_Space           
09_Summit          20_TheEnd          
10_Ruins           21_LastLevel       
```

### B-Side Maps (8 total)
```
01_City, 04_Legend, 05_Restore, 06_Stronghold,
07_Hell, 08_Truth, 09_Summit, 18_Heart
```

### C-Side Maps (8 total)
```
01_City, 04_Legend, 05_Restore, 06_Stronghold,
07_Hell, 08_Truth, 09_Summit, 18_Heart
```

### D-Side Maps (16 total)
```
01_City, 02_Nightmare, 03_Stars, 04_Legend, 05_Restore, 06_Stronghold,
07_Hell, 08_Truth, 09_Summit, 10_Ruins, 11_Snow, 12_Water, 13_Fire,
14_Digital, 15_Castle, 18_Heart
```

## What Gets Updated

### Entity Type Names
Each map contains custom entities that reference dialog:
- `CH0_MODINTRO` → `DZ_CH0_MODINTRO`
- `CH2_INTRO` → `DZ_CH2_INTRO`
- `CH18_ENDING` → `DZ_CH18_ENDING`

### Dialog Key References
Entity properties that point to localized text:
- In entity data: `dialogKey = "CH16_LOST_SOULS_1"` → `"DZ_CH16_LOST_SOULS_1"`
- In triggers: cutscene references updated similarly

## Validation Checks

The MCP verifies:

1. **Entity Names** - All follow `DZ_CHX_NAME` format
2. **Dialog Keys** - All point to valid, updated keys
3. **References** - No orphaned or broken entity references
4. **Structure** - Map binary format is valid

## Troubleshooting

### Maps Not Updating?
1. Ensure Loenn has entity definitions loaded
2. Check that Lua files in `Loenn/entities/` use DZ_ prefix
3. Try saving one map manually to test

### Script Not Found?
```lua
-- Verify the file path
local path = require("tools.MapUpdateMCP")
-- If this fails, check that MapUpdateMCP.lua exists
```

### Validation Errors?
1. Review the detailed report in console output
2. Check specific map mentioned in error
3. Open that map in Loenn and verify manually

## Performance

- **Per-map time**: 1-2 seconds
- **Total batch time**: 2-3 minutes for all 54 maps
- **Disk impact**: Automatic backups (can be disabled)

## Backup & Recovery

Automatic backups are stored in:
```
Maps/Maggy/.backups/
  ASide/
    00_Prologue.bin
    01_City.bin
    [etc.]
```

To restore a map:
```bash
cp Maps/Maggy/.backups/ASide/01_City.bin Maps/Maggy/ASide/01_City.bin
```

## After Update Completion

1. **Test the mod**
   - Launch Celeste
   - Play through a level
   - Verify cutscenes work

2. **Verify no errors**
   - Check Celeste console for "missing entity" messages
   - All dialogs should display normally

3. **Commit changes**
   ```bash
   git add Maps/Maggy/*/
   git add Loenn/
   git commit -m "Update all maps to use DZ_ prefixes and add batch update tool"
   ```

## Advanced: Custom Configurations

To customize what gets updated, edit `MapUpdateMCP.lua`:

```lua
-- Add more replacement rules
MapUpdateMCP.replacementRules = {
    ["CH0_MODINTRO"] = "DZ_CH0_MODINTRO",
    ["CH1_ENDMADELINE"] = "DZ_CH1_ENDMADELINE",
    -- Add your custom rules here
}

-- Customize which sides to process
MapUpdateMCP.sides = {"ASide", "BSide", "CSide", "DSide"}
```

## Support & Documentation

- **Full Guide**: See `MAP_UPDATE_GUIDE.md` for comprehensive documentation
- **Loenn Docs**: https://github.com/CelestialCartographers/Loenn
- **DZ Mod**: See parent directory for mod-specific information

---

**Status**: ✓ Ready to use
**Last Updated**: June 3, 2026
**Target**: All 54 DZ maps (21 chapters × 4 sides, minus unmapped chapters)

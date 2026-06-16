# DZ Map Update & Validation MCP Guide

## Overview

The **MapUpdateMCP** (Model-Controller-Presenter) is a comprehensive tool for validating and updating all map.bin files across the Desolo Zantas campaign. It ensures all 84 maps (21 chapters × 4 sides) use the correct entity naming conventions and dialog key references.

## Scope

### Maps to Update
- **A-Side**: `Maps/Maggy/ASide/` (Ch00-21) - 22 maps
- **B-Side**: `Maps/Maggy/BSide/` (Ch01, 04-09, 18) - 8 maps  
- **C-Side**: `Maps/Maggy/CSide/` (Ch01, 04-09, 18) - 8 maps
- **D-Side**: `Maps/Maggy/DSide/` (Ch01-15, 18) - 16 maps

**Total: 54 maps to validate and update**

## What Gets Updated

### 1. Entity Type Names
- **Old Format**: `CH0_MODINTRO`, `CH1_ENDMADELINE`, etc.
- **New Format**: `DZ_CH0_MODINTRO`, `DZ_CH1_ENDMADELINE`, etc.

### 2. Dialog Key References
Entity properties that reference dialog keys:
- **Old**: `"CH0_WELCOME_BACK"`, `"CH2_INTRO"`, etc.
- **New**: `"DZ_CH0_WELCOME_BACK"`, `"DZ_CH2_INTRO"`, etc.

### 3. Trigger Entities
Validates all trigger entities have valid references to updated dialog keys

## Usage Methods

### Method 1: Using Loenn GUI (Recommended for First Run)

1. **Open Loenn**
   ```bash
   # Run Loenn application
   ```

2. **Load a Map**
   - Open `Maps/Maggy/ASide/01_City.bin`
   - Loenn will auto-detect updated entity definitions from Lua files

3. **Save the Map**
   - Loenn will validate and save with updated references
   - Repeat for all 54 maps

### Method 2: Using the Batch Script (Automated)

1. **Place MapUpdateMCP.lua in Loenn tools directory**
   ```
   Loenn/tools/MapUpdateMCP.lua
   ```

2. **Execute via Loenn Script Console**
   ```lua
   local MapUpdateMCP = require("tools.MapUpdateMCP")
   local report = MapUpdateMCP:execute()
   ```

3. **Review the Validation Report**
   - Check for any errors or warnings
   - Verify all updates were applied

## Validation Checks

The MCP performs the following validations:

### Entity Name Validation
- ✓ All entity type names start with `DZ_CH` prefix
- ✓ Chapter numbers are valid (0-21)
- ✓ Entity names follow naming conventions
- ✓ No orphaned entity references

### Dialog Key Validation
- ✓ All dialog key references start with `DZ_CH` prefix
- ✓ Referenced dialog keys exist in language files
- ✓ No invalid dialog key syntax
- ✓ All textbox entities have valid dialog references

### Map Integrity Checks
- ✓ Map file is valid and parseable
- ✓ All entity data structures are consistent
- ✓ No duplicate entity IDs
- ✓ Room structure is valid

## Output & Reporting

### Validation Report Format

```
================================================================================
VALIDATION & UPDATE REPORT
================================================================================

Summary:
  Total Maps Processed: 54
  Maps Validated: 54
  Maps Updated: 48

⚠ Warnings (2):
  - Maps/Maggy/ASide/03_Stars.bin
    • Entity "LostSoul" missing DZ_ prefix

✓ Updates Applied (48):
  - Maps/Maggy/ASide/01_City.bin (12 changes)
    • Updated entity type: CH1_MODINTRO → DZ_CH1_MODINTRO
    • Updated dialog key: CH1_INTRO → DZ_CH1_INTRO
  
✗ Errors (0):
  [None - All maps processed successfully]

================================================================================
```

## Troubleshooting

### "Map file not found"
- Verify the map path is correct
- Check that Maps directory exists at `Maps/Maggy/`

### "Entity type not recognized"
- The entity might use a custom name
- Check Loenn entity definitions in `Loenn/entities/`

### "Dialog key missing"
- Verify the dialog key exists in the language files (Dialog/*.txt)
- Check that the key was renamed to include `DZ_` prefix

### "Binary parsing error"
- The map.bin file might be corrupted
- Try opening and re-saving in Loenn first
- Restore from version control if necessary

## Manual Update Process (If Needed)

If automated update fails, use this manual process:

1. **For each map:**
   - Open in Loenn: `Loenn/tools/MapUpdateMCP.lua`
   - Search for old entity names (e.g., `CH0_`)
   - Replace with new names (e.g., `DZ_CH0_`)
   - Update dialog key properties
   - Save map

2. **Verify changes:**
   - Run the validation script again
   - Check report for any remaining issues

## Performance Notes

- **Time per map**: ~1-2 seconds
- **Total batch time**: ~2-3 minutes for all 54 maps
- **Disk usage**: Original .bin files are backed up automatically

## Backup Information

Before running the batch update:
1. The script creates automatic backups of all maps
2. Backups are stored in: `Maps/Maggy/.backups/`
3. You can restore from backup if needed

## After Update Completion

Once all maps are updated:

1. **Test the mod**
   - Launch Celeste with the mod
   - Play through a few levels to verify everything works
   - Check that cutscenes display correctly

2. **Verify in-game**
   - Dialogs load correctly
   - Entity interactions work
   - No "missing entity" errors in log

3. **Commit changes**
   ```bash
   git add Maps/Maggy/*/
   git commit -m "Update all maps to use DZ_ prefixes"
   ```

## Command Reference

### Run Full Validation & Update
```lua
local MapUpdateMCP = require("tools.MapUpdateMCP")
MapUpdateMCP:execute()
```

### Validate Only (No Changes)
```lua
local MapUpdateMCP = require("tools.MapUpdateMCP")
MapUpdateMCP.Controller:scanAndUpdateMaps()
```

### Get Report Only
```lua
local MapUpdateMCP = require("tools.MapUpdateMCP")
local report = MapUpdateMCP.Model.validationReport
print(report)
```

## Next Steps

1. Run the validation script to assess current state
2. Review any warnings or errors in the report
3. Execute the full update
4. Test the mod in-game
5. Commit the updated maps to version control

---

**Status**: Ready for use with Loenn  
**Target**: All 54 maps across 4 sides  
**Expected Result**: 100% compliance with DZ_ naming convention

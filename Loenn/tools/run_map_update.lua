-- ============================================================================
-- QUICK EXECUTION SCRIPT FOR MAGGYHELPER MAP UPDATE
-- ============================================================================
-- Usage: Run this script in Loenn's Lua console or script executor
-- Purpose: Execute full map validation and update in one command
-- ============================================================================

print("\n" .. string.rep("=", 80))
print("MAGGYHELPER MAP UPDATE EXECUTION SCRIPT")
print(string.rep("=", 80) .. "\n")

-- Load the MCP
print("[*] Loading MapUpdateMCP...")
local MapUpdateMCP = require("tools.MapUpdateMCP")

if not MapUpdateMCP then
    print("[ERROR] Failed to load MapUpdateMCP!")
    print("Make sure MapUpdateMCP.lua is in Loenn/tools/")
    return
end

print("[✓] MapUpdateMCP loaded successfully\n")

-- Display configuration
print("Configuration:")
print(string.format("  Maps Root: %s", MapUpdateMCP.mapsRootPath))
print(string.format("  Sides: %s", table.concat(MapUpdateMCP.sides, ", ")))
print(string.format("  A-Side Chapters: %d", #MapUpdateMCP.chaptersASide))
print(string.format("  B/C/D-Side Chapters: %d", #MapUpdateMCP.chaptersBSideDSide))
print(string.format("  Expected Total Maps: 54\n")

-- Run the update
print("Starting validation and update process...")
print("(This may take 2-3 minutes)\n")

local startTime = os.time()
local report = MapUpdateMCP:execute()
local endTime = os.time()

-- Display final summary
print("\nFinal Summary:")
print(string.format("  ✓ Completed in %.1f seconds", endTime - startTime))
print(string.format("  ✓ Total maps processed: %d", report.totalMaps))
print(string.format("  ✓ Maps validated: %d", report.mapsValidated))
print(string.format("  ✓ Maps updated: %d", report.mapsUpdated))
print(string.format("  ⚠ Warnings: %d", #report.warnings))
print(string.format("  ✗ Errors: %d", #report.errors))

-- Return the report for inspection
print("\n[*] Update complete! Review the report above for details.")
print("    Use 'return report' to view the full report object in console.\n")

return report

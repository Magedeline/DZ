---
description: Generate and import AI-generated map rooms using the gamelab PCG AI
auto_execution_mode: 3
---

## PCG AI Map Generation Workflow

1. Open the target map in Lönn

2. Run the "PCG AI: Build Generation Request" script in Lönn
   - Set preset, room count, theme, difficulty
   - Optionally enable includeExistingContext and add a customPrompt
   - This writes `Loenn/pcg/output/generation_request.json`

3. In Windsurf/Cascade chat, ask the AI to generate the map:
   - "Use the gamelab-mcp server to generate a map from Loenn/pcg/output/generation_request.json using the entity catalog at Loenn/pcg/entity_catalog.json. Save to Loenn/pcg/output/generated_map.json."
   - The AI will call the gamelab-mcp MCP tools to generate the map

4. Run the "PCG AI: Import Generated Map" script in Lönn
   - Set mode to "merge" or "replace"
   - Optionally set targetRoom, offset_x, offset_y
   - Use dryRun=true first to preview

5. Review imported rooms in Lönn, adjust as needed, save (Ctrl+S)

6. Test in-game before committing

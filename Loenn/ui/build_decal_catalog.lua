-- Utility: produce a categorized decal list from Decals/decals.yaml for external tools or custom UI

local decalsLib = require("libraries.decals")

local M = {}

function M.build()
    local byCategory = {}
    for _, e in ipairs(decalsLib.getRegisteredDecals()) do
        local cat = e.category or "Ingeste"
        byCategory[cat] = byCategory[cat] or {}
        table.insert(byCategory[cat], { id = e.id, texture = e.texture })
    end
    return byCategory
end

return M

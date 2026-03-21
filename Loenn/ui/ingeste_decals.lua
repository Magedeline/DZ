-- Provides a list of decals for the Loenn editor to show in its Decal tool.
-- Other Loenn-side code can require("ui.desolo_decals").get() to retrieve definitions.

local decalsLib = require("libraries.decals")

local M = {}

function M.get()
    return decalsLib.getRegisteredDecals()
end

return M

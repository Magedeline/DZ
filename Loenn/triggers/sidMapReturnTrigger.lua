-- SidMapReturnTrigger: place at the exit of each small-map / EX / boss room.
-- Sends the player back to whichever lobby they came from.
local sidMapReturnTrigger = {}

sidMapReturnTrigger.name = "DZ/SidMapReturnTrigger"
sidMapReturnTrigger.placements = {
    { name = "main", data = { width = 8, height = 40 } }
}

sidMapReturnTrigger.fieldOrder = { "x", "y", "width", "height" }
sidMapReturnTrigger.color = { 1.0, 0.6, 0.2 }  -- orange

return sidMapReturnTrigger

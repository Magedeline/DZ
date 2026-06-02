local utils = require("utils")

local starJumpCutsceneControlV2 = {}

starJumpCutsceneControlV2.name = "MaggyHelper/StarJumpControlCutscenesV2"
starJumpCutsceneControlV2.depth = -100
starJumpCutsceneControlV2.texture = "@Internal@/northern_lights"

starJumpCutsceneControlV2.fieldInformation = {
    triggerHeight = {
        fieldType = "number",
        minimumValue = 8
    },
    triggerWidth = {
        fieldType = "number",
        minimumValue = 8
    },
    triggerOffsetX = {
        fieldType = "number"
    },
    triggerOffsetY = {
        fieldType = "number"
    }
}

starJumpCutsceneControlV2.fieldOrder = {
    "x", "y",
    "musicEvent", "cutsceneFlag",
    "useCustomTriggerBox",
    "triggerWidth", "triggerHeight",
    "triggerOffsetX", "triggerOffsetY"
}

starJumpCutsceneControlV2.placements = {
    name = "StarJumpControlCutscenesV2",
    data = {
        musicEvent = "event:/pusheen/music/lvl8/starjump",
        cutsceneFlag = "plateaumod_2",
        triggerHeight = 32,
        triggerWidth = 64,
        triggerOffsetX = 0,
        triggerOffsetY = 0,
        useCustomTriggerBox = false
    }
}

-- Custom drawing to show the trigger area
function starJumpCutsceneControlV2.draw(room, entity, viewport)
    local x = entity.x or 0
    local y = entity.y or 0
    local triggerWidth = entity.triggerWidth or 64
    local triggerHeight = entity.triggerHeight or 32
    local offsetX = entity.triggerOffsetX or 0
    local offsetY = entity.triggerOffsetY or 0
    local useCustom = entity.useCustomTriggerBox or false
    
    -- Calculate trigger box position
    local boxX, boxY, boxW, boxH
    if useCustom then
        boxX = x + offsetX - triggerWidth / 2
        boxY = y + offsetY
        boxW = triggerWidth
        boxH = triggerHeight
    else
        boxX = x - 32
        boxY = y
        boxW = 64
        boxH = 32
    end
    
    -- Draw main indicator (entity position)
    love.graphics.setColor(0.64, 1.0, 1.0, 0.8)  -- Cyan color
    love.graphics.rectangle("fill", x - 8, y - 8, 16, 16)
    
    -- Draw trigger box fill
    love.graphics.setColor(0.64, 1.0, 1.0, 0.3)
    love.graphics.rectangle("fill", boxX, boxY, boxW, boxH)
    
    -- Draw trigger box outline
    love.graphics.setColor(0.64, 1.0, 1.0, 0.8)
    love.graphics.rectangle("line", boxX, boxY, boxW, boxH)
    
    -- Draw entry zone indicator (bottom of trigger box)
    -- This shows where the Kglobal::global::Celeste.Playershould enter from (below)
    love.graphics.setColor(0.0, 1.0, 0.5, 0.5)  -- Green for entry zone
    love.graphics.rectangle("fill", boxX, boxY + boxH - 4, boxW, 4)
    
    -- Draw ascending arrow to indicate vertical hit direction (Kglobal::global::Celeste.Playerenters from below)
    love.graphics.setColor(1.0, 1.0, 0.0, 0.9)  -- Yellow for arrow
    local arrowX = boxX + boxW / 2
    local arrowY = boxY + boxH + 20  -- Below the trigger box
    
    -- Arrow pointing up into the trigger
    love.graphics.line(arrowX, arrowY + 15, arrowX, arrowY - 5)  -- Arrow shaft
    love.graphics.line(arrowX - 6, arrowY + 2, arrowX, arrowY - 5)  -- Arrow head left
    love.graphics.line(arrowX + 6, arrowY + 2, arrowX, arrowY - 5)  -- Arrow head right
    
    -- Draw "ENTER FROM BELOW" text indicator
    love.graphics.setColor(1.0, 1.0, 0.0, 0.7)
    love.graphics.print("↑", arrowX - 4, arrowY + 18)
    
    love.graphics.setColor(1, 1, 1, 1)  -- Reset color
end

function starJumpCutsceneControlV2.selection(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    local triggerWidth = entity.triggerWidth or 64
    local triggerHeight = entity.triggerHeight or 32
    local offsetX = entity.triggerOffsetX or 0
    local offsetY = entity.triggerOffsetY or 0
    local useCustom = entity.useCustomTriggerBox or false
    
    -- Include the arrow area below the trigger in the selection
    if useCustom then
        local boxX = x + offsetX - triggerWidth / 2
        local boxY = y + offsetY
        return utils.rectangle(boxX - 8, y - 16, triggerWidth + 16, offsetY + triggerHeight + 56)
    else
        return utils.rectangle(x - 40, y - 16, 80, 80)
    end
end

return starJumpCutsceneControlV2

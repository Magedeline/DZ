-- Kirby Audio Entity for DesoloZantas
-- Demonstrates mod-specific character audio integration

local audioLib = require("libraries.audio_library")

local kirbyAudio = {}

kirbyAudio.name = "DesoloZantas/KirbyAudioEntity"
kirbyAudio.depth = 0
kirbyAudio.placements = {
    {
        name = "Kirby Voice Box",
        data = {
            x = 0,
            y = 0,
            audioType = "dialogue",
            triggerOnContact = true,
            autoPlay = false,
            volume = 1.0,
            cooldown = 1000
        }
    }
}

-- Kirby-specific audio types
local kirbyAudioTypes = {
    "dialogue",
    "ability_copy",
    "float",
    "inhale",
    "spit",
    "land",
    "damage"
}

kirbyAudio.fieldInformation = {
    audioType = {
        options = kirbyAudioTypes,
        editable = false
    },
    volume = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0
    },
    cooldown = {
        fieldType = "integer",
        minimumValue = 0
    }
}

kirbyAudio.fieldOrder = {
    "x", "y", "audioType", "triggerOnContact", "autoPlay", "volume", "cooldown"
}

function kirbyAudio.sprite(room, entity)
    -- Use a simple Kirby-colored circle as sprite
    return "ahorn/DesoloZantas_kirby_audio_16"
end

function kirbyAudio.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function kirbyAudio.tooltip(room, entity)
    local tooltip = "Kirby Audio Entity\n"
    tooltip = tooltip .. "Type: " .. (entity.audioType or "dialogue") .. "\n"
    
    -- Get the actual audio event from our library
    local audioEvent = audioLib.getAudioEvent("sfx", "dialogue", "kirby")
    if audioEvent then
        tooltip = tooltip .. "Event: " .. audioEvent.path .. "\n"
        tooltip = tooltip .. "GUID: " .. audioEvent.guid .. "\n"
    end
    
    if entity.autoPlay then
        tooltip = tooltip .. "Auto-plays on level start"
    elseif entity.triggerOnContact then
        tooltip = tooltip .. "Plays on player contact"
    else
        tooltip = tooltip .. "Manual trigger only"
    end
    
    return tooltip
end

function kirbyAudio.draw(room, entity)
    local x, y = entity.x, entity.y
    
    -- Draw Kirby-inspired pink circle
    love.graphics.setColor(1.0, 0.7, 0.8, 0.8)
    love.graphics.circle("fill", x, y, 8)
    
    -- Draw border
    love.graphics.setColor(0.8, 0.4, 0.6, 1.0)
    love.graphics.circle("line", x, y, 8)
    
    -- Draw simple face
    love.graphics.setColor(0.2, 0.2, 0.2, 1.0)
    -- Eyes
    love.graphics.circle("fill", x - 3, y - 2, 1)
    love.graphics.circle("fill", x + 3, y - 2, 1)
    
    -- Mouth based on audio type
    if entity.audioType == "dialogue" then
        -- Open mouth for dialogue
        love.graphics.arc("fill", x, y + 1, 2, 0, math.pi)
    elseif entity.audioType == "inhale" then
        -- Larger circular mouth for inhale
        love.graphics.circle("line", x, y + 2, 2)
    else
        -- Small smile for other types
        love.graphics.arc("line", x, y + 1, 2, 0, math.pi)
    end
    
    -- Draw sound indicator if auto-play or trigger on contact
    if entity.autoPlay or entity.triggerOnContact then
        love.graphics.setColor(1.0, 1.0, 1.0, 0.7)
        -- Sound waves
        love.graphics.arc("line", x + 10, y, 3, -math.pi/4, math.pi/4)
        love.graphics.arc("line", x + 10, y, 5, -math.pi/6, math.pi/6)
        love.graphics.arc("line", x + 10, y, 7, -math.pi/8, math.pi/8)
    end
    
    love.graphics.setColor(1.0, 1.0, 1.0, 1.0)
end

return kirbyAudio
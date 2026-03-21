local utils = require("utils")

local cassette = {}

cassette.name = "MaggyHelper/Cassette"
cassette.depth = -1000000

cassette.placements = {
    {
        name = "Cassette",
        data = {
            onCollect = "",
            customAudio = "",
            particleColor = "9CFCFF",
            glowStrength = 1.0,
            bloomStrength = 0.8,
            wiggleIntensity = 0.35,
            floatSpeed = 2.0,
            floatRange = 2.0,
            collectDelay = 0.3,
            persistent = true
        }
    }
}

cassette.fieldInformation = {
    onCollect = {
        fieldType = "string",
        options = {
            "",
            "unlock_door",
            "trigger_cutscene", 
            "activate_switch",
            "spawn_entity",
            "teleport_player",
            "change_music",
            "show_dialogue",
            "set_flag",
            "complete_level",
            "custom_script"
        },
        editable = true
    },
    customAudio = {
        fieldType = "string"
    },
    particleColor = {
        fieldType = "string"
    },
    glowStrength = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0
    },
    bloomStrength = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0
    },
    wiggleIntensity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 1.0
    },
    floatSpeed = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 10.0
    },
    floatRange = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    collectDelay = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0
    },
    persistent = {
        fieldType = "boolean"
    }
}

function cassette.sprite(room, entity)
    local color = {1.0, 1.0, 1.0, 1.0}
    
    -- Parse hex color if provided
    if entity.particleColor and entity.particleColor ~= "" then
        local hex = entity.particleColor:gsub("#", "")
        if #hex == 6 then
            local r = tonumber(hex:sub(1, 2), 16) / 255
            local g = tonumber(hex:sub(3, 4), 16) / 255
            local b = tonumber(hex:sub(5, 6), 16) / 255
            color = {r, g, b, 1.0}
        end
    else
        -- Default cassette glow color (9CFCFF)
        color = {156/255, 252/255, 255/255, 1.0}
    end
    
    return {
        {
            texture = "collectables/cassette/idle00",
            x = entity.x,
            y = entity.y,
            color = color
        }
    }
end

function cassette.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

-- Helper function to get action description for the editor
function cassette.getActionDescription(action)
    local descriptions = {
        unlock_door = "Unlocks doors with matching ID",
        trigger_cutscene = "Starts a cutscene sequence",
        activate_switch = "Activates switches or mechanisms",
        spawn_entity = "Spawns a new entity at collection point",
        teleport_player = "Teleports player to specified location",
        change_music = "Changes background music",
        show_dialogue = "Displays dialogue text",
        set_flag = "Sets a session flag",
        complete_level = "Completes the current level",
        custom_script = "Runs custom Lua script"
    }
    return descriptions[action] or "Custom collection behavior"
end

-- Node support for complex behaviors
cassette.nodeLineRenderType = "line"
cassette.nodeLimits = {0, 3}

function cassette.nodeSprite(room, entity, node, nodeIndex)
    local x = node.x or 0
    local y = node.y or 0
    
    if nodeIndex == 1 then
        -- First node: target location for teleport or spawn
        return {
            {
                texture = "util/pixel",
                x = x - 4,
                y = y - 4,
                width = 8,
                height = 8,
                color = {1.0, 0.8, 0.2, 0.7}
            }
        }
    elseif nodeIndex == 2 then
        -- Second node: secondary target or activation point
        return {
            {
                texture = "util/pixel",
                x = x - 4,
                y = y - 4,
                width = 8,
                height = 8,
                color = {0.2, 0.8, 1.0, 0.7}
            }
        }
    elseif nodeIndex == 3 then
        -- Third node: tertiary target or effect area
        return {
            {
                texture = "util/pixel",
                x = x - 4,
                y = y - 4,
                width = 8,
                height = 8,
                color = {0.8, 0.2, 1.0, 0.7}
            }
        }
    end
    
    return {}
end

return cassette
local cartridge = {}

cartridge.name = "DZ/Cartridge"
cartridge.depth = -100
cartridge.justification = {0.5, 0.5}
cartridge.placements = {
    name = "cartridge",
    data = {
        spritePath = "collectables/cartridge/",
        menuSprite = "collectables/cartridge",
        unlockText = "",
        remixExtraToUnlock = "",
        onCollect = "",
        customAudio = "",
        particleColor = "FFD700",
        glowStrength = 1.5,
        bloomStrength = 1.0,
        wiggleIntensity = 0.5,
        floatSpeed = 1.5,
        floatRange = 3.0,
        collectDelay = 0.5,
        persistent = true,
        isChapter19Finale = false
    }
}

-- Define the sprite texture
cartridge.texture = "collectables/cartridge/idle00"

-- Node support for teleportation points
cartridge.nodeLineRenderType = "line"
cartridge.nodeLimits = {0, -1}  -- Unlimited nodes

-- Field information for the entity editor
cartridge.fieldInformation = {
    spritePath = {
        fieldType = "string",
        description = "Path to the cartridge sprite folder"
    },
    menuSprite = {
        fieldType = "string", 
        description = "Sprite shown in unlock menu"
    },
    unlockText = {
        fieldType = "string",
        description = "Custom unlock text (comma-separated for multiple lines). Leave empty to auto-generate."
    },
    remixExtraToUnlock = {
        fieldType = "string",
        description = "ID of the remix extra to unlock when collected"
    },
    onCollect = {
        fieldType = "string",
        options = {
            "",
            "unlock_remix_mode",
            "activate_finale", 
            "unlock_secret_area",
            "trigger_ending",
            "complete_chapter19",
            "unlock_developer_commentary",
            "activate_rainbow_mode",
            "set_flag",
            "complete_level",
            "custom_script"
        },
        description = "Action to execute when collected"
    },
    customAudio = {
        fieldType = "string",
        description = "Custom audio event for collection (leave empty for default)"
    },
    particleColor = {
        fieldType = "string",
        description = "Hex color for particles (default: FFD700 - gold)"
    },
    glowStrength = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 5.0,
        description = "Intensity of the glow effect"
    },
    bloomStrength = {
        fieldType = "number", 
        minimumValue = 0.0,
        maximumValue = 3.0,
        description = "Intensity of the bloom effect"
    },
    wiggleIntensity = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0,
        description = "Intensity of the wiggle animation"
    },
    floatSpeed = {
        fieldType = "number",
        minimumValue = 0.1,
        maximumValue = 5.0,
        description = "Speed of the floating animation"
    },
    floatRange = {
        fieldType = "number",
        minimumValue = 1.0,
        maximumValue = 10.0,
        description = "Range of the floating movement in pixels"
    },
    collectDelay = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 2.0,
        description = "Delay before collection sequence starts"
    },
    persistent = {
        fieldType = "boolean",
        description = "Whether the cartridge state persists between sessions"
    },
    isChapter19Finale = {
        fieldType = "boolean",
        description = "Enable special Chapter 19 finale effects (rainbow coloring, etc.)"
    }
}

-- Field order for cleaner UI
cartridge.fieldOrder = {
    "x", "y",
    "remixExtraToUnlock",
    "unlockText", 
    "onCollect",
    "isChapter19Finale",
    "spritePath",
    "menuSprite",
    "customAudio",
    "particleColor",
    "glowStrength",
    "bloomStrength", 
    "wiggleIntensity",
    "floatSpeed",
    "floatRange",
    "collectDelay",
    "persistent"
}

-- Custom selection function for better editor experience
function cartridge.selection(room, entity)
    return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

-- Render function for the level editor
function cartridge.sprite(room, entity)
    local sprites = {}
    
    -- Main cartridge sprite
    local mainSprite = drawableSprite.fromTexture(cartridge.texture, entity)
    
    -- Add special effects for Chapter 19 finale
    if entity.isChapter19Finale then
        -- Add a subtle rainbow tint to indicate special properties
        mainSprite:setColor({1.0, 0.9, 0.3, 1.0})  -- Golden tint
    end
    
    table.insert(sprites, mainSprite)
    
    -- Add glow effect indicator if glow strength is high
    if entity.glowStrength and entity.glowStrength > 1.2 then
        local glowSprite = drawableSprite.fromTexture(cartridge.texture, entity)
        glowSprite:setColor({1.0, 1.0, 0.5, 0.3})  -- Subtle yellow glow
        glowSprite:setScale(1.2, 1.2)
        table.insert(sprites, 1, glowSprite)  -- Insert behind main sprite
    end
    
    return sprites
end

-- Node rendering for teleportation points
function cartridge.nodeSprite(room, entity, node, nodeIndex)
    if nodeIndex == 1 then
        -- First node is the respawn point (blue)
        local sprite = drawableSprite.fromTexture("objects/checkpoint/bg", {x = node.x, y = node.y})
        sprite:setColor({0.3, 0.6, 1.0, 0.8})
        return sprite
    else
        -- Additional nodes are teleportation points (green)
        local sprite = drawableSprite.fromTexture("objects/checkpoint/bg", {x = node.x, y = node.y})
        sprite:setColor({0.3, 1.0, 0.6, 0.8})
        return sprite
    end
end

-- Helper function to get particle color as RGB
function cartridge.getParticleColorRGB(entity)
    local colorHex = entity.particleColor or "FFD700"
    if string.sub(colorHex, 1, 1) == "#" then
        colorHex = string.sub(colorHex, 2)
    end
    
    local r = tonumber(string.sub(colorHex, 1, 2), 16) / 255
    local g = tonumber(string.sub(colorHex, 3, 4), 16) / 255  
    local b = tonumber(string.sub(colorHex, 5, 6), 16) / 255
    
    return {r, g, b, 1.0}
end

-- Validation function to check entity configuration
function cartridge.validate(room, entity)
    local issues = {}
    
    -- Check if sprite path exists
    if entity.spritePath and entity.spritePath ~= "" then
        -- Could add sprite path validation here
    end
    
    -- Validate hex color format
    if entity.particleColor and entity.particleColor ~= "" then
        local colorHex = entity.particleColor
        if string.sub(colorHex, 1, 1) == "#" then
            colorHex = string.sub(colorHex, 2)
        end
        
        if string.len(colorHex) ~= 6 or not string.match(colorHex, "^%x+$") then
            table.insert(issues, "Particle color must be a valid 6-digit hex color (e.g., FFD700)")
        end
    end
    
    -- Validate numeric ranges
    if entity.glowStrength and (entity.glowStrength < 0 or entity.glowStrength > 5) then
        table.insert(issues, "Glow strength must be between 0 and 5")
    end
    
    if entity.floatSpeed and (entity.floatSpeed < 0.1 or entity.floatSpeed > 5) then
        table.insert(issues, "Float speed must be between 0.1 and 5")
    end
    
    return issues
end

return cartridge
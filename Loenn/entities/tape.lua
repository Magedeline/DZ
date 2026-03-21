local tape = {}

tape.name = "MaggyHelper/Tape"
tape.depth = -1000000
tape.nodeLineRenderType = "line"
tape.nodeLimits = {2, 2}

tape.placements = {
    name = "tape",
    data = {
        -- Visuals
        spritePath       = "collectables/cassette/",
        menuSprite       = "collectables/maggy/tape",
        particleColor    = "FF9CCF",
        glowStrength     = 1.0,
        bloomStrength    = 0.8,
        wiggleIntensity  = 0.35,
        floatSpeed       = 2.0,
        floatRange       = 2.0,
        -- Audio
        collectSfx        = "event:/desolozantas/game/general/cassette_unlocked",
        previewEvent      = "event:/desolozantas/game/general/cassette_preview",
        previewParamName  = "remix",
        previewParamValue = -1.0,
        -- Unlock
        cSideToUnlock = "map/campaingname/mapname/map.bin",
        unlockText    = ""
    }
}

tape.fieldInformation = {
    particleColor = {
        fieldType = "color"
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
    previewParamValue = {
        fieldType = "number",
        minimumValue = -1.0
    },
    unlockText = {
        fieldType = "string",
        tooltipText = "Comma-separated dialog keys shown in order during the unlock cutscene. Leave empty to auto-detect from cSideToUnlock."
    }
}

-- Folder path (ending with /) → append "idle00"; direct frame key → use as-is.
function tape.texture(room, entity)
    local path = entity.spritePath or "collectables/cassette/"
    if path:sub(-1) == "/" then
        return path .. "idle00"
    end
    return path
end

-- Node 1 (gold)  = respawn point / cassette-fly destination
-- Node 2 (teal)  = cassette-fly origin
function tape.nodeSprite(room, entity, node, nodeIndex)
    local colors = {
        {1.0, 0.8, 0.2, 0.8},
        {0.2, 0.9, 0.8, 0.8}
    }
    local color = colors[nodeIndex] or {1.0, 1.0, 1.0, 0.6}
    return {
        {
            texture = "util/pixel",
            x = -4, y = -4,
            width = 8, height = 8,
            color = color
        }
    }
end

return tape
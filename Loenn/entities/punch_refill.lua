local punchRefill = {}

punchRefill.name = "DZ/PunchRefill"
punchRefill.depth = -100

punchRefill.fieldInformation = {
    punchCount = {
        fieldType = "integer",
        minimumValue = 1,
        maximumValue = 10
    },
    respawnTime = {
        fieldType = "number",
        minimumValue = 0.0,
        maximumValue = 10.0
    },
    spriteVariant = {
        options = {
            ["auto"] = "auto",
            ["single"] = "single",
            ["multi"] = "multi"
        },
        editable = false
    }
}

punchRefill.fieldOrder = {
    "x", "y",
    "punchCount",
    "oneUse",
    "respawnTime",
    "breakEvenWhenFull",
    "spriteVariant"
}

punchRefill.placements = {
    {
        name = "single_punch",
        alternativeName = "punch_refill_single",
        data = {
            punchCount = 3,
            oneUse = false,
            respawnTime = 2.5,
            breakEvenWhenFull = false,
            spriteVariant = "single"
        }
    },
    {
        name = "multi_punch",
        alternativeName = "punch_refill_multi",
        data = {
            punchCount = 5,
            oneUse = false,
            respawnTime = 2.5,
            breakEvenWhenFull = false,
            spriteVariant = "multi"
        }
    },
    {
        name = "auto_punch",
        alternativeName = "punch_refill",
        data = {
            punchCount = 3,
            oneUse = false,
            respawnTime = 2.5,
            breakEvenWhenFull = false,
            spriteVariant = "auto"
        }
    }
}

function punchRefill.texture(room, entity)
    local spriteVariant = entity.spriteVariant or "auto"
    local punchCount = entity.punchCount or 3

    -- Determine variant based on punch count if auto
    if spriteVariant == "auto" then
        spriteVariant = punchCount > 2 and "multi" or "single"
    end

    -- Return texture based on variant
    if spriteVariant == "multi" then
        return "objects/punchMulti/idle_fist00"
    else
        return "objects/punchSingle/idle_fist00"
    end
end

function punchRefill.color(room, entity)
    return {1.0, 0.84, 0.0, 1.0}  -- Gold for punch attacks
end

return punchRefill

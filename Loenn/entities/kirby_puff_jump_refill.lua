local puffRefill = {}

puffRefill.name = "DZ/KirbyPuffJumpRefill"
puffRefill.depth = -100

puffRefill.fieldInformation = {
    puffCount = {
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

puffRefill.fieldOrder = {
    "x", "y",
    "puffCount",
    "oneUse",
    "respawnTime",
    "breakEvenWhenFull",
    "spriteVariant"
}

puffRefill.placements = {
    {
        name = "single_puff",
        alternativeName = "kirby_puff_refill_single",
        data = {
            puffCount = 3,
            oneUse = false,
            respawnTime = 2.5,
            breakEvenWhenFull = false,
            spriteVariant = "single"
        }
    },
    {
        name = "multi_puff",
        alternativeName = "kirby_puff_refill_multi",
        data = {
            puffCount = 5,
            oneUse = false,
            respawnTime = 2.5,
            breakEvenWhenFull = false,
            spriteVariant = "multi"
        }
    },
    {
        name = "auto_puff",
        alternativeName = "kirby_puff_refill",
        data = {
            puffCount = 3,
            oneUse = false,
            respawnTime = 2.5,
            breakEvenWhenFull = false,
            spriteVariant = "auto"
        }
    }
}

function puffRefill.texture(room, entity)
    local spriteVariant = entity.spriteVariant or "auto"
    local puffCount = entity.puffCount or 3

    -- Determine variant based on puff count if auto
    if spriteVariant == "auto" then
        spriteVariant = puffCount > 2 and "multi" or "single"
    end

    -- Return texture based on variant
    if spriteVariant == "multi" then
        return "objects/puffrefillmulti/idle00"
    else
        return "objects/puffrefill/idle00"
    end
end

function puffRefill.color(room, entity)
    return {1.0, 0.4, 0.7, 1.0}  -- Hot pink for Kirby puff
end

return puffRefill

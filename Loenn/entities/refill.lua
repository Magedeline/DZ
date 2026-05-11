local refill = {}

refill.name = "MaggyHelper/AdvancedRefill"
refill.depth = -100
refill.placements = {
    {
        name = "one_dash",
        alternativeName = "one_dash_crystal",
        data = {
            oneUse = false,
            dashCount = 1
        }
    },
    {
        name = "two_dashes",
        alternativeName = "two_dashes_crystal",
        data = {
            oneUse = false,
            dashCount = 2
        }
    },
    {
        name = "three_dashes",
        alternativeName = "three_dashes_crystal",
        data = {
            oneUse = false,
            dashCount = 3
        }
    },
    {
        name = "four_dashes",
        alternativeName = "four_dashes_crystal",
        data = {
            oneUse = false,
            dashCount = 4
        }
    },
    {
        name = "five_dashes",
        alternativeName = "five_dashes_crystal",
        data = {
            oneUse = false,
            dashCount = 5
        }
    },
    {
        name = "ten_dashes",
        alternativeName = "ten_dashes_crystal",
        data = {
            oneUse = false,
            dashCount = 10
        }
    }
}

function refill.texture(room, entity)
    local dashCount = entity.dashCount or 1
    if dashCount == 2 then
        return "objects/Maggy/DesoloZantas/refillTwo/idle00"
    elseif dashCount == 3 then
        return "objects/Maggy/DesoloZantas/solarrefill/idle00"
    elseif dashCount == 4 then
        return "objects/Maggy/DesoloZantas/lunarrefill/idle00"
    elseif dashCount == 5 then
        return "objects/Maggy/DesoloZantas/blackholerefill/idle00"
    elseif dashCount == 10 then
        return "objects/Maggy/DesoloZantas/savestarrefill/idle00"
    else
        return "objects/Maggy/DesoloZantas/refill/idle00"
    end
end

function refill.playDashSfx(dashCount)
    local sfx
    if dashCount == 3 then
        sfx = ""
    elseif dashCount == 4 then
        sfx = ""
    elseif dashCount == 5 then
        sfx = "event:/desolo_zantas/final_content/game/19_the_end/gigadiamond_touch"
    elseif dashCount == 10 then
        sfx = "event:/new_content/game/10_farewell/pinkrefill_touch"
    else
        sfx = "event:/game/general/refill_touch"
    end
    return sfx
end

return refill

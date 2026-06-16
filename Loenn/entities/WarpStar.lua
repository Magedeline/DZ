local utils = require("utils")

local warpStar = {}

warpStar.name = "DZ/WarpStar"
warpStar.depth = -1000000
warpStar.texture = "objects/warpstars/idle00"

warpStar.placements = {
    {
        name = "WarpStar",
        data = {
            oneUse = false,
            shielded = false,
            isKirbyWarpStar = false
        }
    },
    {
        name = "one_use",
        data = {
            oneUse = true,
            shielded = false,
            isKirbyWarpStar = false
        }
    },
    {
        name = "kirby_warp_star",
        data = {
            oneUse = false,
            shielded = false,
            isKirbyWarpStar = true
        }
    },
    {
        name = "kirby_one_use",
        data = {
            oneUse = true,
            shielded = false,
            isKirbyWarpStar = true
        }
    }
}

warpStar.fieldInformation = {
    oneUse = {
        fieldType = "boolean",
        description = "If true, the warp star will disappear after use instead of returning to its original position"
    },
    shielded = {
        fieldType = "boolean", 
        description = "If true, the warp star cannot be destroyed by enemies or environmental hazards"
    },
    isKirbyWarpStar = {
        fieldType = "boolean",
        description = "If true, uses Kirby-themed visuals and sounds, optimized for KirbyKglobal::Player"
    }
}

warpStar.fieldOrder = {
    "x", "y", "oneUse", "shielded", "isKirbyWarpStar"
}

function warpStar.texture(room, entity)
    if entity.isKirbyWarpStar then
        return "objects/warpstars/kirby/idle00"
    else
        return "objects/warpstars/idle00"
    end
end

return warpStar

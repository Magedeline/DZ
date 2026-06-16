local Entity = {}

Entity.name = "DZ/ClutterSwitch"
Entity.depth = 0

local variantTextures = {
    ["ClutterSwitch red"] = "objects/resortclutter/icon_red",
    ["ClutterSwitch green"] = "objects/resortclutter/icon_green",
    ["ClutterSwitch blue"] = "objects/resortclutter/icon_blue",
    ["ClutterSwitch yellow"] = "objects/resortclutter/icon_yellow"
}

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        type = entity.type or "ClutterSwitch red",
        musicEvent = entity.musicEvent or "guid://{d49a04ce-06fb-43bb-8880-1b95a4f6544f}",
        absorbCutsceneSound = entity.absorbCutsceneSound or "guid://{ab48ef65-2a19-4e26-bd96-c91188020dd6}",
        progressMusic = entity.progressMusic or true,
        lightingAlphaAdd = entity.lightingAlphaAdd or 0.05,
        disableLightning = entity.disableLightning or false
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local variant = entity.type or "ClutterSwitch red"
    local texture = variantTextures[variant] or "objects/resortclutter/icon_red"
    return {
        texture = texture,
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 0.5
    }
end

return Entity

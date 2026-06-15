local Entity = {}

Entity.name = "MaggyHelper/KirbyNPC"
Entity.depth = 100

function Entity.place(room, entity)
    return {
        x = entity.x or 0,
        y = entity.y or 0,
        character = entity.character or 0,
        behavior = entity.behavior or 0,
        dialogId = entity.dialogId or "",
        canGiveItem = entity.canGiveItem or false,
        giveItemId = entity.giveItemId or "",
        followDistance = entity.followDistance or 48,
        moveSpeed = entity.moveSpeed or 40
    }
end

function Entity.draw(room, entity)
    local x, y = entity.x or 0, entity.y or 0
    local character = entity.character or 0
    local textures = {
        [0] = "characters/kirby_npc/bandana_dee/idle00",
        [1] = "characters/kirby_npc/king_dedede/idle00",
        [2] = "characters/kirby_npc/meta_knight/idle00",
        [3] = "characters/kirby_npc/magolor/idle00",
        [9] = "characters/kirby_npc/gooey/idle00"
    }
    local texture = textures[character] or "characters/kirby/idle00"
    return {
        texture = texture,
        x = x,
        y = y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return Entity

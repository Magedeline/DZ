local npc19_gravestone = {
    name = "DesoloZantas/NPC19_Gravestone",
    depth = 1000,
    texture = "characters/gravestones/maddydead00",
    nodeLimits = {1, 1},
    nodeLineRenderType = "line",
    fieldInformation = {},
    placements = {
        {
            name = "NPC19 Gravestone (Chara Boost)",
            data = {}
        }
    }
}

function npc19_gravestone.nodeTexture(room, entity, node, index)
    return "objects/charaboost/idle00"
end

function npc19_gravestone.nodeRender(room, entity, node, index)
    local x, y = node.x, node.y
    local sprite = drawableSprite.fromTexture("objects/charaboost/idle00", {x = x, y = y})
    sprite:setColor({0.8, 0.8, 1.0, 0.6})
    return sprite
end

return npc19_gravestone

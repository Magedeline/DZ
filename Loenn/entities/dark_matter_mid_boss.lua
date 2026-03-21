local darkMatterMidBoss = {}

darkMatterMidBoss.name = "MaggyHelper/DarkMatterMidBoss"
darkMatterMidBoss.depth = -10000
darkMatterMidBoss.justification = {0.5, 1.0}
darkMatterMidBoss.texture = "characters/darkmatter_boss/idle00"
darkMatterMidBoss.nodeLimits = {0, -1}
darkMatterMidBoss.nodeLineRenderType = "line"

darkMatterMidBoss.placements = {
    {
        name = "dark_matter_mid_boss",
        data = {
            health = 80
        }
    },
    {
        name = "dark_matter_mid_boss_hard",
        data = {
            health = 120
        }
    }
}

darkMatterMidBoss.fieldInformation = {
    health = {
        fieldType = "integer",
        minimumValue = 20,
        maximumValue = 200
    }
}

function darkMatterMidBoss.nodeSprite(room, entity, node, nodeIndex, viewport)
    local x, y = node.x, node.y
    local sprites = {}
    
    -- Draw teleport node marker
    table.insert(sprites, {
        texture = "ahorn/entity_node",
        x = x,
        y = y,
        color = {0.5, 0.0, 0.8, 0.8}
    })
    
    return sprites
end

return darkMatterMidBoss

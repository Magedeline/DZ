local bosses_group = {}

bosses_group.name = "DZ/BossesGroup"
bosses_group.depth = 0
bosses_group.placements = {
    {
        name = "main",
        data = {
            x = 0,
            y = 0,
            groupName = "BossGroup",
            bossNames = "kingtitan"
        }
    }
}

bosses_group.fieldInformation = {
    bossNames = {
        fieldType = "string"
    }
}

function bosses_group.sprite(room, entity)
    local sprite = require("structs.drawable_sprite").fromTexture("objects/heartGem/idle00", entity)
    return {sprite}
end

return bosses_group

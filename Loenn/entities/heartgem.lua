local heartGem = {}

heartGem.name = "MaggyHelper/HeartGem"
heartGem.depth = -2000000
heartGem.justification = {0.5, 1.0}
heartGem.texture = "collectables/maggy/heartgem/0/00"

heartGem.fieldInformation = {
    fake = {
        fieldType = "boolean"
    },
    removeCameraTriggers = {
        fieldType = "boolean"
    },
    endLevelOnCollect = {
        fieldType = "boolean"
    }
}

heartGem.placements = {
    {
        name = "heartgem",
        data = {
            fake = false,
            removeCameraTriggers = false,
            endLevelOnCollect = false
        }
    },
    {
        name = "fake",
        data = {
            fake = true,
            removeCameraTriggers = false,
            endLevelOnCollect = false
        }
    },
    {
        name = "with_camera_removal",
        data = {
            fake = false,
            removeCameraTriggers = true,
            endLevelOnCollect = false
        }
    },
    {
        name = "end_level",
        data = {
            fake = false,
            removeCameraTriggers = false,
            endLevelOnCollect = true
        }
    }
}

function heartGem.sprite(room, entity)
    local isFake = entity.fake or false
    local texture = isFake and "collectables/maggy/heartgem/4/00" or "collectables/maggy/heartgem/0/00"
    
    return {
        texture = texture,
     x = entity.x,
        y = entity.y,
    justificationX = 0.5,
        justificationY = 1.0,
  scaleX = 1.0,
      scaleY = 1.0
    }
end

function heartGem.selection(room, entity)
  return utils.rectangle(entity.x - 8, entity.y - 16, 16, 16)
end

return heartGem

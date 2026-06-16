local heartGem = {}

heartGem.name = "DZ/HeartGem"
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
    },
    sevenbirdFlyby = {
        fieldType = "boolean"
    },
    unlockDside = {
        fieldType = "boolean"
    },
    customSfx = {
        fieldType = "string"
    }
}

heartGem.placements = {
    {
        name = "heartgem",
        data = {
            fake = false,
            removeCameraTriggers = false,
            endLevelOnCollect = false,
            sevenbirdFlyby = false,
            unlockDside = false,
            customSfx = ""
        }
    },
    {
        name = "fake",
        data = {
            fake = true,
            removeCameraTriggers = false,
            endLevelOnCollect = false,
            sevenbirdFlyby = false,
            unlockDside = false,
            customSfx = ""
        }
    },
    {
        name = "with_camera_removal",
        data = {
            fake = false,
            removeCameraTriggers = true,
            endLevelOnCollect = false,
            sevenbirdFlyby = false,
            unlockDside = false,
            customSfx = ""
        }
    },
    {
        name = "end_level",
        data = {
            fake = false,
            removeCameraTriggers = false,
            endLevelOnCollect = true,
            sevenbirdFlyby = false,
            unlockDside = false,
            customSfx = ""
        }
    },
    {
        name = "crystal_sevenbird",
        data = {
            fake = false,
            removeCameraTriggers = false,
            endLevelOnCollect = false,
            sevenbirdFlyby = true,
            unlockDside = false,
            customSfx = ""
        }
    },
    {
        name = "crystal_pink_custom",
        data = {
            fake = false,
            removeCameraTriggers = false,
            endLevelOnCollect = false,
            sevenbirdFlyby = true,
            unlockDside = false,
            customSfx = "event:/game/pusheen/general/pink_crystalheart_get"
        }
    },
    {
        name = "crystal_rainbow_custom",
        data = {
            fake = false,
            removeCameraTriggers = false,
            endLevelOnCollect = false,
            sevenbirdFlyby = true,
            unlockDside = false,
            customSfx = "event:/game/pusheen/general/rainbow_crystalheart_get"
        }
    },
    {
        name = "crystal_unlock_dside",
        data = {
            fake = false,
            removeCameraTriggers = false,
            endLevelOnCollect = false,
            sevenbirdFlyby = true,
            unlockDside = true,
            customSfx = "event:/game/pusheen/general/pink_crystalheart_get"
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

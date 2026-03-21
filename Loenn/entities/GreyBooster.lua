local utils = require("utils")

return {
    name = "DesoloZatnas/GreyBooster",
    depth = -8500,
    texture = "objects/greybooster/gasterbooster00",
    fieldInformation = {
        red = {
            fieldType = "boolean",
            editorType = "boolean"
        }
    },
    placements = {
        {
            name = "grey_booster_green",
            data = {
                red = false
            }
        },
        {
            name = "grey_booster_red",
            data = {
                red = true
            }
        }
    },
    selection = function(room, entity)
        return utils.rectangle(entity.x - 8, entity.y - 8, 16, 16)
    end
}

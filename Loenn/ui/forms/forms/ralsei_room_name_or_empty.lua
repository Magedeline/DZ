local stringField = require("ui.forms.fields.string")
local loadedState = require("loaded_state")

-- Shamelessly "borrowed" from VivHelper, except this one accepts empty values.

local room_names = {}
room_names.fieldType = "MaggyHelper.ralsei_room_name_or_empty"

function room_names.getElement(name, value, options)
    -- Add extra options and pass it onto string field
    local roomNames = {}

    if loadedState.map then
        for _, room in ipairs(loadedState.map.rooms) do
            table.insert(roomNames, room.name:match("^lvl_(.*)") or room.name)
        end
    end

    options.options = roomNames

    options.validator = function(v)
        local currentName = v

        if not currentName or currentName == "" then
            return true
        end
        
        for _, room in ipairs(roomNames) do 
            if currentName == room then 
                return true 
            end
        end
        
        return false
    end
    
    return stringField.getElement(name, value, options)
end

return room_names
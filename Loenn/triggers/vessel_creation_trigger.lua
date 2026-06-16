local vesselCreationTrigger = {}

vesselCreationTrigger.name = "DZ/VesselCreationTrigger"
vesselCreationTrigger.placements = {
    {
        name = "VesselCreationTrigger",
        data = {
            width = 16,
            height = 16,
            triggerOnce = true,
            autoStart = false
        }
    },
    {
        name = "auto_start",
        data = {
            width = 32,
            height = 32,
            triggerOnce = true,
            autoStart = true
        }
    }
}

vesselCreationTrigger.fieldInformation = {
    triggerOnce = {
        fieldType = "boolean"
    },
    autoStart = {
        fieldType = "boolean"
    }
}

function vesselCreationTrigger.sprite(room, entity)
    local width = entity.width or 16
    local height = entity.height or 16
    
    return {
        texture = "ahorn/vessel_creation_trigger",
        x = 0,
        y = 0,
        width = width,
        height = height
    }
end

return vesselCreationTrigger
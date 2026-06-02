local tesseractPortalTorch = {}

tesseractPortalTorch.name = "MaggyHelper/TesseractPortalTorch"
tesseractPortalTorch.depth = 0
tesseractPortalTorch.texture = "objects/IngesteHelper/tesseract_portal_torch"
tesseractPortalTorch.justification = {0.5, 1.0}

tesseractPortalTorch.fieldInformation = {
    isLit = {
        fieldType = "boolean"
    },
    portalActive = {
        fieldType = "boolean"
    },
    portalDestination = {
        fieldType = "string"
    },
    lightRadius = {
        fieldType = "number",
        minimumValue = 16.0,
        maximumValue = 200.0
    },
    portalColor = {
        fieldType = "color"
    },
    requiresActivation = {
        fieldType = "boolean"
    },
    activationFlag = {
        fieldType = "string"
    },
    particleEffect = {
        fieldType = "boolean"
    }
}

tesseractPortalTorch.placements = {
    {
        name = "normal",
        data = {
            isLit = true,
            portalActive = false,
            portalDestination = "",
            lightRadius = 80.0,
            portalColor = "8844ff",
            requiresActivation = false,
            activationFlag = "",
            particleEffect = true
        }
    },
    {
        name = "active_portal",
        data = {
            isLit = true,
            portalActive = true,
            portalDestination = "spawn",
            lightRadius = 120.0,
            portalColor = "ff4488",
            requiresActivation = false,
            activationFlag = "",
            particleEffect = true
        }
    },
    {
        name = "activation_required",
        data = {
            isLit = false,
            portalActive = false,
            portalDestination = "",
            lightRadius = 40.0,
            portalColor = "4488ff",
            requiresActivation = true,
            activationFlag = "torch_activated",
            particleEffect = false
        }
    }
}

function tesseractPortalTorch.sprite(room, entity)
    local isLit = entity.isLit or true
    local portalActive = entity.portalActive or false
    
    local texture = "objects/IngesteHelper/tesseract_portal_torch"
    if not isLit then
        texture = texture .. "_unlit"
    elseif portalActive then
        texture = texture .. "_portal"
    end
    
    return {
        texture = texture,
        x = entity.x,
        y = entity.y,
        justificationX = 0.5,
        justificationY = 1.0
    }
end

return tesseractPortalTorch
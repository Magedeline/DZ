local bridge = {}

bridge.name = "MaggyHelper/Bridge"
bridge.depth = 0
bridge.fillColor = {0.4, 0.4, 0.4, 0.8}
bridge.borderColor = {0.0, 0.0, 0.0, 1.0}

bridge.placements = {
    {
        name = "BridgeUWU",
        data = {
            width = 160,
            height = 8,
        }
    }
}

bridge.fieldInformation = {
    width = {
        fieldType = "integer",
        minimumValue = 8,
        maximumValue = 2048
    },
    getLevelFlag = {
        fieldType = "string"
    }
}

bridge.sprite = "objects/bridge/bridge"
bridge.nodeLimits = {2, 2}
bridge.nodeLineRenderType = "line"

function bridge.nodeSprite(room, entity, node, nodeIndex)
    if nodeIndex == 1 then
        return {texture = "util/gap_start"}
    elseif nodeIndex == 2 then
        return {texture = "util/gap_end"}
    end
end

function bridge.nodeTooltip(entity, node, nodeIndex)
    if nodeIndex == 1 then
        return "Gap Start Position"
    elseif nodeIndex == 2 then
        return "Gap End Position"
    end
end

function bridge.rectangle(room, entity)
    return utils.rectangle(entity.x, entity.y, entity.width or 160, entity.height or 8)
end

return bridge
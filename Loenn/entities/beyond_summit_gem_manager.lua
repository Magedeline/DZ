local beyondSummitGemManager = {}
beyondSummitGemManager.name = "DZ/BeyondSummitGemManager"
beyondSummitGemManager.depth = -10010
beyondSummitGemManager.nodeLimits = {0, 7}
beyondSummitGemManager.nodeLineRenderType = "line"

beyondSummitGemManager.placements = {
    name = "BeyondSummitGemManager",
    data = {
        flag = "beyondsummit_gate_open",
    }
}

function beyondSummitGemManager.texture(room, entity)
    return "collectables/summitgems/yourbestfriend00"
end

function beyondSummitGemManager.nodeTexture(room, entity, node, nodeIndex)
    local gem = (nodeIndex - 1) % 7
    return "collectables/summitgems/" .. gem .. "/gem00"
end

function beyondSummitGemManager.selection(room, entity)
    return Ahorn and Ahorn.Rectangle or require("structs.rectangle")(entity.x - 16, entity.y - 16, 32, 32)
end

return beyondSummitGemManager

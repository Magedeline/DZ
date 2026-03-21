local cutsceneNode = {}

cutsceneNode.name = "MaggyHelper/CutsceneNode"
cutsceneNode.depth = 0
cutsceneNode.texture = "@Internal@/cutscene_node"
cutsceneNode.placements = {
    name = "cutscene_node",
    data = {
        nodeName = "player_skip"
    }
}

return cutsceneNode
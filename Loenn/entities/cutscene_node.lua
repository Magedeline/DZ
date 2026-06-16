local cutsceneNode = {}

cutsceneNode.name = "DZ/CutsceneNode"
cutsceneNode.depth = 0
cutsceneNode.texture = "@Internal@/cutscene_node"
cutsceneNode.placements = {
    name = "cutscene_node",
    data = {
        nodeName = "Kglobal::Player_skip"
    }
}

return cutsceneNode
local FlingBirdIntroCutscene = {}

FlingBirdIntroCutscene.name = "MaggyHelper/FlingBirdIntroCutscene"
FlingBirdIntroCutscene.depth = -1
FlingBirdIntroCutscene.texture = "characters/bird/hover00"
FlingBirdIntroCutscene.justification = {0.5, 1.0}
FlingBirdIntroCutscene.nodeLimits = {1, -1}
FlingBirdIntroCutscene.nodeLineRenderType = "line"

FlingBirdIntroCutscene.placements = {
    {
        name = "FlingBirdIntroCutscene_Crash",
        data = {
            crashes = true
        },
        nodes = {
            {x = 80, y = 0},
            {x = 160, y = -40},
            {x = 240, y = 0}
        }
    },
    {
        name = "FlingBirdIntroCutscene_Miss",
        data = {
            crashes = false
        },
        nodes = {
            {x = 80, y = 0},
            {x = 160, y = -40},
            {x = 240, y = 0}
        }
    }
}

FlingBirdIntroCutscene.fieldInformation = {
    crashes = {
        fieldType = "boolean"
    }
}

function FlingBirdIntroCutscene.nodeSprite(room, entity, node, nodeIndex)
    return {
        texture = "characters/bird/hover00",
        x = node.x,
        y = node.y,
        justificationX = 0.5,
        justificationY = 1.0,
        scaleX = 0.6,
        scaleY = 0.6
    }
end

function FlingBirdIntroCutscene.nodeRectangle(room, entity, node, nodeIndex)
    return utils.rectangle(node.x - 8, node.y - 16, 16, 16)
end

return FlingBirdIntroCutscene

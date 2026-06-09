local kirbyPlayer = {}

kirbyPlayer.name = "MaggyHelper/KirbyPlayer"
kirbyPlayer.depth = -1000000
kirbyPlayer.texture = "characters/kirby/idle00"
kirbyPlayer.justification = {0.5, 1.0}

-- Simple spawn marker entity
kirbyPlayer.nodeLineRenderType = "line"
kirbyPlayer.nodeLimits = {0, 0}

kirbyPlayer.placements = {
    {
        name = "kirby_player",
        data = {
        }
    }
}

kirbyPlayer.fieldInformation = {
    facing = {
        fieldType = "integer",
        options = {-1, 1},
        editable = false
    }
}

return kirbyPlayer

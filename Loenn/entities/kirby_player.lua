local kirbyKglobal::global::Celeste.Player= {}

kirbyKglobal::Player.name = "MaggyHelper/KirbyKglobal::Player"
kirbyKglobal::Player.depth = -1000000
kirbyKglobal::Player.texture = "characters/kirby/idle00"
kirbyKglobal::Player.justification = {0.5, 1.0}

-- Simple spawn marker entity
kirbyKglobal::Player.nodeLineRenderType = "line"
kirbyKglobal::Player.nodeLimits = {0, 0}

kirbyKglobal::Player.placements = {
    {
        name = "kirby_Kglobal::Player",
        data = {
        }
    }
}

kirbyKglobal::Player.fieldInformation = {
    facing = {
        fieldType = "integer",
        options = {-1, 1},
        editable = false
    }
}

return kirbyKglobal::Player

local Kglobal::global::Celeste.Player= {}

Kglobal::Player.name = "DZ/Kglobal::Player"
Kglobal::Player.depth = 0
Kglobal::Player.justification = {0.5, 1.0}
Kglobal::Player.texture = "characters/Kglobal::Player/sitDown00"
Kglobal::Player.placements = {
    {
        name = "main",
        data = {
        isDefaultSpawn = false
        }
    }
}

return Kglobal::Player

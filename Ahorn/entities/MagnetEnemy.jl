module MagnetEnemy

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MagnetEnemy" MagnetEnemy(x::Integer, y::Integer, health::Integer=2, pullStrength::Number=80.0)

const placements = Ahorn.PlacementDict(
    "MagnetEnemy" => Ahorn.EntityPlacement(MagnetEnemy),
    "MagnetEnemystrong" => Ahorn.EntityPlacement(MagnetEnemy)
)

function Ahorn.selection(entity::MagnetEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MagnetEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

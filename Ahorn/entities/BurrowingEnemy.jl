module BurrowingEnemy

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/BurrowingEnemy" BurrowingEnemy(x::Integer, y::Integer, detectionRange::Number=80.0, health::Integer=1, surfaceTime::Number=2.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(BurrowingEnemy),
    "quick" => Ahorn.EntityPlacement(BurrowingEnemy)
)

function Ahorn.selection(entity::BurrowingEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BurrowingEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

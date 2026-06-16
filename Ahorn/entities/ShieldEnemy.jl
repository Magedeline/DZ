module ShieldEnemy

using ..Ahorn, Maple

@mapdef Entity "DZ/ShieldEnemy" ShieldEnemy(x::Integer, y::Integer, health::Integer=2, speed::Number=30.0)

const placements = Ahorn.PlacementDict(
    "ShieldEnemy" => Ahorn.EntityPlacement(ShieldEnemy)
)

function Ahorn.selection(entity::ShieldEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ShieldEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

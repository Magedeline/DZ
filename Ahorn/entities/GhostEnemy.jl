module GhostEnemy

using ..Ahorn, Maple

@mapdef Entity "DZ/GhostEnemy" GhostEnemy(x::Integer, y::Integer, health::Integer=3)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(GhostEnemy),
    "fast" => Ahorn.EntityPlacement(GhostEnemy)
)

function Ahorn.selection(entity::GhostEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GhostEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

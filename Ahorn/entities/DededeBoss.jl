module DededeBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/DededeBoss" DededeBoss(x::Integer, y::Integer, attackCooldown::Number=1.5, bossMusic::String="event:/music/lvl9/main", health::Integer=25)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(DededeBoss)
)

function Ahorn.selection(entity::DededeBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DededeBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

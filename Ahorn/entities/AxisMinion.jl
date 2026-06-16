module AxisMinion

using ..Ahorn, Maple

@mapdef Entity "DZ/AxisMinion" AxisMinion(x::Integer, y::Integer, detectionRange::Integer=120, health::Integer=2, moveSpeed::Integer=60, patrolDistance::Integer=80)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(AxisMinion)
)

function Ahorn.selection(entity::AxisMinion)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AxisMinion, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

module FlameCerberus

using ..Ahorn, Maple

@mapdef Entity "DZ/FlameCerberus" FlameCerberus(x::Integer, y::Integer, detectionRange::Integer=200, headHealth::Integer=3, moveSpeed::Integer=60)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(FlameCerberus)
)

function Ahorn.selection(entity::FlameCerberus)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlameCerberus, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

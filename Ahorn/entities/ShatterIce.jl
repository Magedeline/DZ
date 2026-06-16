module ShatterIce

using ..Ahorn, Maple

@mapdef Entity "DZ/ShatterIce" ShatterIce(x::Integer, y::Integer, height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "ShatterIce" => Ahorn.EntityPlacement(ShatterIce),
    "ShatterIcethick" => Ahorn.EntityPlacement(ShatterIce)
)

function Ahorn.selection(entity::ShatterIce)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ShatterIce, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

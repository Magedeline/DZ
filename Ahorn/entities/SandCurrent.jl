module SandCurrent

using ..Ahorn, Maple

@mapdef Entity "DZ/SandCurrent" SandCurrent(x::Integer, y::Integer, directionX::Number=1.0, directionY::Number=0.0, height::Integer=64, width::Integer=64)

const placements = Ahorn.PlacementDict(
    "right" => Ahorn.EntityPlacement(SandCurrent),
    "left" => Ahorn.EntityPlacement(SandCurrent),
    "up" => Ahorn.EntityPlacement(SandCurrent)
)

function Ahorn.selection(entity::SandCurrent)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SandCurrent, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

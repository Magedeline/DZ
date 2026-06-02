module PaintCanvas

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/PaintCanvas" PaintCanvas(x::Integer, y::Integer, defaultColor::String="ffffff", height::Integer=64, width::Integer=64)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(PaintCanvas)
)

function Ahorn.selection(entity::PaintCanvas)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PaintCanvas, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

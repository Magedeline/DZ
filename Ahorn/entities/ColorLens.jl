module ColorLens

using ..Ahorn, Maple

@mapdef Entity "DZ/ColorLens" ColorLens(x::Integer, y::Integer, color::String="ff0000", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "red" => Ahorn.EntityPlacement(ColorLens),
    "blue" => Ahorn.EntityPlacement(ColorLens),
    "green" => Ahorn.EntityPlacement(ColorLens)
)

function Ahorn.selection(entity::ColorLens)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ColorLens, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

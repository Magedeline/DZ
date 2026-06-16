module ColorBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/ColorBlock" ColorBlock(x::Integer, y::Integer, alpha::Number=1.0, color::String="6969ee", depth::Integer=5000, height::Integer=8, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(ColorBlock)
)

function Ahorn.selection(entity::ColorBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ColorBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

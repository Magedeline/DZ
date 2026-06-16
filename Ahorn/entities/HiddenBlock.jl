module HiddenBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/HiddenBlock" HiddenBlock(x::Integer, y::Integer, height::Integer=16, permanent::Bool=true, tiletype::String="3", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(HiddenBlock)
)

function Ahorn.selection(entity::HiddenBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HiddenBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

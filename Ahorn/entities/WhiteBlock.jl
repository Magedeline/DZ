module WhiteBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/WhiteBlock" WhiteBlock(x::Integer, y::Integer, height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(WhiteBlock)
)

function Ahorn.selection(entity::WhiteBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WhiteBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

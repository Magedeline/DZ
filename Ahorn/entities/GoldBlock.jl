module GoldBlock

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/GoldBlock" GoldBlock(x::Integer, y::Integer, height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(GoldBlock)
)

function Ahorn.selection(entity::GoldBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GoldBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

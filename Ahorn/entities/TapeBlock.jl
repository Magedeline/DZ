module TapeBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/TapeBlock" TapeBlock(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "TapeBlock" => Ahorn.EntityPlacement(TapeBlock)
)

function Ahorn.selection(entity::TapeBlock)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TapeBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

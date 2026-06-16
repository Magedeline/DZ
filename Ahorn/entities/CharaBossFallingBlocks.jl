module CharaBossFallingBlocks

using ..Ahorn, Maple

@mapdef Entity "DZ/CharaBossFallingBlocks" CharaBossFallingBlocks(x::Integer, y::Integer, height::Integer=8, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "chara_falling_block" => Ahorn.EntityPlacement(CharaBossFallingBlocks)
)

function Ahorn.selection(entity::CharaBossFallingBlocks)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CharaBossFallingBlocks, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

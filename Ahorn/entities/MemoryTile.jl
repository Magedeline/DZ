module MemoryTile

using ..Ahorn, Maple

@mapdef Entity "DZ/MemoryTile" MemoryTile(x::Integer, y::Integer, puzzleId::String="memory_1", tileId::Integer=0, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(MemoryTile)
)

function Ahorn.selection(entity::MemoryTile)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MemoryTile, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

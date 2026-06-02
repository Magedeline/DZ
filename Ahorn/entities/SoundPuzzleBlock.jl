module SoundPuzzleBlock

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/SoundPuzzleBlock" SoundPuzzleBlock(x::Integer, y::Integer, noteIndex::Integer=0, puzzleId::String="puzzle_1", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "SoundPuzzleBlock" => Ahorn.EntityPlacement(SoundPuzzleBlock)
)

function Ahorn.selection(entity::SoundPuzzleBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SoundPuzzleBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

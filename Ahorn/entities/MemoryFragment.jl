module MemoryFragment

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MemoryFragment" MemoryFragment(x::Integer, y::Integer, dialogueKey::String="MEMORY_FRAGMENT", fragmentId::String="", fragmentNumber::Integer=1, showDialogue::Bool=true)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(MemoryFragment)
)

function Ahorn.selection(entity::MemoryFragment)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MemoryFragment, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

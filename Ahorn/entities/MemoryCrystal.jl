module MemoryCrystal

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MemoryCrystal" MemoryCrystal(x::Integer, y::Integer, flashbackDuration::Number=3.0, memoryId::String="", oneTime::Bool=true)

const placements = Ahorn.PlacementDict(
    "Memory Crystal" => Ahorn.EntityPlacement(MemoryCrystal)
)

function Ahorn.selection(entity::MemoryCrystal)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MemoryCrystal, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

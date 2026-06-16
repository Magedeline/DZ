module HorseHitch

using ..Ahorn, Maple

@mapdef Entity "DZ/HorseHitch" HorseHitch(x::Integer, y::Integer, destinationId::String="", hitchId::String="", isUnlocked::Bool=false)

const placements = Ahorn.PlacementDict(
    "locked" => Ahorn.EntityPlacement(HorseHitch),
    "unlocked" => Ahorn.EntityPlacement(HorseHitch)
)

function Ahorn.selection(entity::HorseHitch)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HorseHitch, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

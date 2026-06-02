module CorruptionCrystal

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/CorruptionCrystal" CorruptionCrystal(x::Integer, y::Integer, corruptionRadius::Integer=100, health::Integer=3, spreadSpeed::Integer=20)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(CorruptionCrystal)
)

function Ahorn.selection(entity::CorruptionCrystal)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CorruptionCrystal, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

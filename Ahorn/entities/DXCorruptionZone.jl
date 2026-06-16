module DXCorruptionZone

using ..Ahorn, Maple

@mapdef Entity "DZ/DXCorruptionZone" DXCorruptionZone(x::Integer, y::Integer, damageInterval::Number=1.0, height::Integer=64, intensity::Number=1.0, width::Integer=64)

const placements = Ahorn.PlacementDict(
    "DXCorruptionZone" => Ahorn.EntityPlacement(DXCorruptionZone),
    "DXCorruptionZone_Intense" => Ahorn.EntityPlacement(DXCorruptionZone)
)

function Ahorn.selection(entity::DXCorruptionZone)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DXCorruptionZone, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.0, 0.5, 0.2), (0.8, 0.0, 0.8, 0.6))
end

end

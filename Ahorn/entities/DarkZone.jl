module DarkZone

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/DarkZone" DarkZone(x::Integer, y::Integer, PlayerLightRadius::Number=40.0, height::Integer=64, width::Integer=64)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(DarkZone),
    "dim" => Ahorn.EntityPlacement(DarkZone)
)

function Ahorn.selection(entity::DarkZone)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DarkZone, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

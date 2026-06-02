module SpringCloud

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/SpringCloud" SpringCloud(x::Integer, y::Integer, respawnTime::Number=3.0, width::Integer=24)

const placements = Ahorn.PlacementDict(
    "SpringCloud" => Ahorn.EntityPlacement(SpringCloud),
    "one_use" => Ahorn.EntityPlacement(SpringCloud),
    "super_bounce" => Ahorn.EntityPlacement(SpringCloud)
)

function Ahorn.selection(entity::SpringCloud)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SpringCloud, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.9, 0.9, 1.0, 0.3), (1.0, 1.0, 1.0, 0.5))
end

end

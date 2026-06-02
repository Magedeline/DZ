module GravityZoneTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/GravityZoneTrigger" GravityZoneTrigger(x::Integer, y::Integer, gravityDirection::String="Up", height::Integer=32, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "GravityZoneTrigger" => Ahorn.EntityPlacement(GravityZoneTrigger),
    "low_gravity" => Ahorn.EntityPlacement(GravityZoneTrigger),
    "zero_gravity" => Ahorn.EntityPlacement(GravityZoneTrigger)
)

function Ahorn.selection(entity::GravityZoneTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GravityZoneTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

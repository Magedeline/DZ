module TeleportTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/TeleportTrigger" TeleportTrigger(x::Integer, y::Integer, height::Integer=16, targetRoom::String="", targetX::Integer=0, targetY::Integer=0, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "TeleportTrigger" => Ahorn.EntityPlacement(TeleportTrigger)
)

function Ahorn.selection(entity::TeleportTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TeleportTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

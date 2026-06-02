module BridgeFreezeTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/BridgeFreezeTrigger" BridgeFreezeTrigger(x::Integer, y::Integer, height::Integer=80, width::Integer=120)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(BridgeFreezeTrigger),
    "slow_motion" => Ahorn.EntityPlacement(BridgeFreezeTrigger)
)

function Ahorn.selection(entity::BridgeFreezeTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BridgeFreezeTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

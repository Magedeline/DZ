module PlayerTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/PlayerTrigger" PlayerTrigger(x::Integer, y::Integer, height::Integer=16, inventoryDashes::Integer=1, inventoryDreamDash::Bool=false, inventoryNoRefills::Bool=false, kirbyPower::String="None", maxDashes::Integer=3, onEnterAction::String="None", onEnterFlag::String="", onExitAction::String="None", onExitFlag::String="", requiredFlag::String="", setFlagState::Bool=true, triggerOnEnter::Bool=true, triggerOnExit::Bool=true, triggerOnce::Bool=false, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(PlayerTrigger),
    "enter_only" => Ahorn.EntityPlacement(PlayerTrigger),
    "exit_only" => Ahorn.EntityPlacement(PlayerTrigger)
)

function Ahorn.selection(entity::PlayerTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PlayerTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

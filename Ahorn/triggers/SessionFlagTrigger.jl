module SessionFlagTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/SessionFlagTrigger" SessionFlagTrigger(x::Integer, y::Integer, flagAction::String="SetValue", flagState::Bool=true, height::Integer=8, requiredFlag::String="", requiredFlagState::Bool=true, sampleProperty::Integer=0, sessionFlag::String="", triggerMode::String="OnEnter", triggerOnce::Bool=true, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "MaggyHelper/SessionFlagTrigger" => Ahorn.EntityPlacement(SessionFlagTrigger)
)

function Ahorn.selection(entity::SessionFlagTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SessionFlagTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

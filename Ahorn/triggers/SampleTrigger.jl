module SampleTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/SampleTrigger" SampleTrigger(x::Integer, y::Integer, flagAction::String="SetValue", flagState::Bool=true, height::Integer=8, requiredFlag::String="", requiredFlagState::Bool=true, sampleProperty::Integer=0, sessionFlag::String="", triggerMode::String="OnEnter", triggerOnce::Bool=true, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "MaggyHelper/SampleTrigger" => Ahorn.EntityPlacement(SampleTrigger)
)

function Ahorn.selection(entity::SampleTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SampleTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

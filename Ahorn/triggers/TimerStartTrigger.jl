module TimerStartTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/TimerStartTrigger" TimerStartTrigger(x::Integer, y::Integer, duration::Number=60.0, height::Integer=16, timerId::String="timer_1", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "TimerStartTrigger" => Ahorn.EntityPlacement(TimerStartTrigger)
)

function Ahorn.selection(entity::TimerStartTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TimerStartTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

module TimerStopTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/TimerStopTrigger" TimerStopTrigger(x::Integer, y::Integer, height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "TimerStopTrigger" => Ahorn.EntityPlacement(TimerStopTrigger)
)

function Ahorn.selection(entity::TimerStopTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TimerStopTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

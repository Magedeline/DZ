module CountdownEscapeTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/CountdownEscapeTrigger" CountdownEscapeTrigger(x::Integer, y::Integer, countdown::Number=60.0, height::Integer=16, targetRoom::String="", warningTime::Number=10.0, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CountdownEscapeTrigger" => Ahorn.EntityPlacement(CountdownEscapeTrigger)
)

function Ahorn.selection(entity::CountdownEscapeTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CountdownEscapeTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

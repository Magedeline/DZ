module ScoreTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/ScoreTrigger" ScoreTrigger(x::Integer, y::Integer, height::Integer=16, points::Integer=100, showPopup::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "ScoreTrigger" => Ahorn.EntityPlacement(ScoreTrigger)
)

function Ahorn.selection(entity::ScoreTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ScoreTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

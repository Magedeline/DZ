module NarratorTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/NarratorTrigger" NarratorTrigger(x::Integer, y::Integer, dialogId::String="", duration::Number=3.0, height::Integer=16, position::String="Top", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "NarratorTrigger" => Ahorn.EntityPlacement(NarratorTrigger)
)

function Ahorn.selection(entity::NarratorTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NarratorTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

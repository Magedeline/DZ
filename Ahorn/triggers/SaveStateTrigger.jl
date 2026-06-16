module SaveStateTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/SaveStateTrigger" SaveStateTrigger(x::Integer, y::Integer, action::String="Save", height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "save" => Ahorn.EntityPlacement(SaveStateTrigger),
    "load" => Ahorn.EntityPlacement(SaveStateTrigger)
)

function Ahorn.selection(entity::SaveStateTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SaveStateTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

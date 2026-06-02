module MirrorModeTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/MirrorModeTrigger" MirrorModeTrigger(x::Integer, y::Integer, height::Integer=16, mirrorX::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "horizontal" => Ahorn.EntityPlacement(MirrorModeTrigger),
    "vertical" => Ahorn.EntityPlacement(MirrorModeTrigger),
    "both" => Ahorn.EntityPlacement(MirrorModeTrigger)
)

function Ahorn.selection(entity::MirrorModeTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MirrorModeTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

module CharaZoomAppearTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/CharaZoomAppearTrigger" CharaZoomAppearTrigger(x::Integer, y::Integer, affectBadeline::Bool=true, affectChara::Bool=true, height::Integer=16, hideOnLeave::Bool=true, onlyOnce::Bool=true, resetZoomOnLeave::Bool=true, showOnEnter::Bool=true, targetZoom::Number=2.0, width::Integer=16, zoomSpeed::Number=2.0)

const placements = Ahorn.PlacementDict(
    "chara_zoom_appear" => Ahorn.EntityPlacement(CharaZoomAppearTrigger)
)

function Ahorn.selection(entity::CharaZoomAppearTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CharaZoomAppearTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

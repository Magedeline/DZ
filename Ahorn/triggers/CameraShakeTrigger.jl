module CameraShakeTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/CameraShakeTrigger" CameraShakeTrigger(x::Integer, y::Integer, duration::Number=0.5, height::Integer=16, intensity::Number=0.3, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "light" => Ahorn.EntityPlacement(CameraShakeTrigger),
    "heavy" => Ahorn.EntityPlacement(CameraShakeTrigger)
)

function Ahorn.selection(entity::CameraShakeTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CameraShakeTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

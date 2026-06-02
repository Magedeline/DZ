module TimeEchoPlatform

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/TimeEchoPlatform" TimeEchoPlatform(x::Integer, y::Integer, offset::Number=0.0, phaseTime::Number=2.0, syncOnDash::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "Time Echo Platform" => Ahorn.EntityPlacement(TimeEchoPlatform),
    "Time Echo Platform (Fast)" => Ahorn.EntityPlacement(TimeEchoPlatform),
    "Time Echo Platform (Slow)" => Ahorn.EntityPlacement(TimeEchoPlatform)
)

function Ahorn.selection(entity::TimeEchoPlatform)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TimeEchoPlatform, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

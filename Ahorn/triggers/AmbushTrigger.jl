module AmbushTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/AmbushTrigger" AmbushTrigger(x::Integer, y::Integer, enemyCount::Integer=4, height::Integer=32, lockCamera::Bool=true, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "AmbushTrigger" => Ahorn.EntityPlacement(AmbushTrigger)
)

function Ahorn.selection(entity::AmbushTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AmbushTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

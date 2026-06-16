module MagmaPool

using ..Ahorn, Maple

@mapdef Entity "DZ/MagmaPool" MagmaPool(x::Integer, y::Integer, bubbleInterval::Number=0.5, eruptInterval::Number=3.0, height::Integer=16, isInstantDeath::Bool=true, width::Integer=48)

const placements = Ahorn.PlacementDict(
    "deadly" => Ahorn.EntityPlacement(MagmaPool),
    "damage" => Ahorn.EntityPlacement(MagmaPool)
)

function Ahorn.selection(entity::MagmaPool)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MagmaPool, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

module RidgeGate

using ..Ahorn, Maple

@mapdef Entity "DZ/RidgeGate" RidgeGate(x::Integer, y::Integer, flag::String="", height::Integer=48, inverted::Bool=false, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "RidgeGate" => Ahorn.EntityPlacement(RidgeGate)
)

function Ahorn.selection(entity::RidgeGate)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RidgeGate, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

module FarewellGate

using ..Ahorn, Maple

@mapdef Entity "DZ/FarewellGate" FarewellGate(x::Integer, y::Integer, flag::String="", heartGems::Integer=0, height::Integer=48, inverted::Bool=false, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(FarewellGate),
    "heart_locked" => Ahorn.EntityPlacement(FarewellGate)
)

function Ahorn.selection(entity::FarewellGate)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FarewellGate, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

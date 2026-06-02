module DXGravityShifter

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/DXGravityShifter" DXGravityShifter(x::Integer, y::Integer, gravityMultiplier::Number=-1.0, height::Integer=80, startActive::Bool=true, width::Integer=80)

const placements = Ahorn.PlacementDict(
    "DXGravityShifter_Reverse" => Ahorn.EntityPlacement(DXGravityShifter),
    "DXGravityShifter_Low" => Ahorn.EntityPlacement(DXGravityShifter),
    "DXGravityShifter_Heavy" => Ahorn.EntityPlacement(DXGravityShifter)
)

function Ahorn.selection(entity::DXGravityShifter)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DXGravityShifter, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.0, 0.8, 0.8, 0.2), (0.0, 0.8, 0.8, 0.6))
end

end

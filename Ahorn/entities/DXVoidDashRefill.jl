module DXVoidDashRefill

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/DXVoidDashRefill" DXVoidDashRefill(x::Integer, y::Integer, oneUse::Bool=false, respawnTime::Number=2.5)

const placements = Ahorn.PlacementDict(
    "DXVoidDashRefill" => Ahorn.EntityPlacement(DXVoidDashRefill),
    "DXVoidDashRefill_OneUse" => Ahorn.EntityPlacement(DXVoidDashRefill)
)

function Ahorn.selection(entity::DXVoidDashRefill)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DXVoidDashRefill, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

module FirewallBarrier

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/FirewallBarrier" FirewallBarrier(x::Integer, y::Integer, energyDrain::Integer=5, height::Integer=32, maxEnergy::Integer=100, startActive::Bool=true, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "active" => Ahorn.EntityPlacement(FirewallBarrier),
    "inactive" => Ahorn.EntityPlacement(FirewallBarrier)
)

function Ahorn.selection(entity::FirewallBarrier)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FirewallBarrier, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

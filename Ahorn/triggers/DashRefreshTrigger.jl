module DashRefreshTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/DashRefreshTrigger" DashRefreshTrigger(x::Integer, y::Integer, dashCount::Integer=2, height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "DashRefreshTrigger" => Ahorn.EntityPlacement(DashRefreshTrigger),
    "triple" => Ahorn.EntityPlacement(DashRefreshTrigger)
)

function Ahorn.selection(entity::DashRefreshTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DashRefreshTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

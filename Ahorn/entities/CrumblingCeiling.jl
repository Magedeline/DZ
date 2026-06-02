module CrumblingCeiling

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/CrumblingCeiling" CrumblingCeiling(x::Integer, y::Integer, crumbleDelay::Number=0.5, height::Integer=8, width::Integer=24)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(CrumblingCeiling)
)

function Ahorn.selection(entity::CrumblingCeiling)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CrumblingCeiling, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

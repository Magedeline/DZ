module SoulBarrier

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/SoulBarrier" SoulBarrier(x::Integer, y::Integer, barrierId::String="", dissolveTime::Number=1.0, fragmentsRequired::Integer=3, height::Integer=32, width::Integer=8)

const placements = Ahorn.PlacementDict(
    "Soul Barrier" => Ahorn.EntityPlacement(SoulBarrier)
)

function Ahorn.selection(entity::SoulBarrier)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SoulBarrier, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

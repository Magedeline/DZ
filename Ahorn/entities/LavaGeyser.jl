module LavaGeyser

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/LavaGeyser" LavaGeyser(x::Integer, y::Integer, eruptDuration::Number=1.5, eruptInterval::Number=3.0, height::Integer=64)

const placements = Ahorn.PlacementDict(
    "hotgeysernormal" => Ahorn.EntityPlacement(LavaGeyser),
    "hotgeyserrapid" => Ahorn.EntityPlacement(LavaGeyser)
)

function Ahorn.selection(entity::LavaGeyser)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::LavaGeyser, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

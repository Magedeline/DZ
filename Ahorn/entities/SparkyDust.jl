module SparkyDust

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/SparkyDust" SparkyDust(x::Integer, y::Integer, Player::Bool=false, isActive::Bool=true, particleColor::String="ffff00", particleCount::Integer=10, radius::Number=32.0, soundEffect::String="event:/game/general/thing_booped", sparkFrequency::Number=2.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(SparkyDust),
    "magical" => Ahorn.EntityPlacement(SparkyDust)
)

function Ahorn.selection(entity::SparkyDust)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SparkyDust, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/sparky_dust", entity.x, entity.y)
end

end

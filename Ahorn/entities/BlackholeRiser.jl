module BlackholeRiser

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/BlackholeRiser" BlackholeRiser(x::Integer, y::Integer, glitchy::Bool=true, looping::Bool=true, maxHeight::Number=200.0, riseDelay::Number=1.0, speed::Number=120.0, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(BlackholeRiser),
    "fast" => Ahorn.EntityPlacement(BlackholeRiser),
    "slow_tall" => Ahorn.EntityPlacement(BlackholeRiser)
)

function Ahorn.selection(entity::BlackholeRiser)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BlackholeRiser, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/blackhole_riser_base", entity.x, entity.y)
end

end

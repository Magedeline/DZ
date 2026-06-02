module MaggyJumpThru

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MaggyJumpThru" MaggyJumpThru(x::Integer, y::Integer, Player::Bool=false, animated::Bool=false, animationSpeed::Number=1.0, attachToSolids::Bool=false, fallDelay::Number=0.3, fallSpeed::Number=160.0, falls::Bool=false, letSeekersThrough::Bool=false, moveSpeed::Number=0.0, moves::Bool=false, oneWay::Bool=true, respawnTime::Number=2.0, respawns::Bool=true, shakeTime::Number=0.5, sinkAmount::Number=3.0, sinkSpeed::Number=100.0, sinks::Bool=false, surfaceIndex::Integer=4, texture::String="wood", tint::String="ffffff", width::Integer=24)

const placements = Ahorn.PlacementDict(
    "wood" => Ahorn.EntityPlacement(MaggyJumpThru),
    "cliffside" => Ahorn.EntityPlacement(MaggyJumpThru),
    "core" => Ahorn.EntityPlacement(MaggyJumpThru),
    "dream" => Ahorn.EntityPlacement(MaggyJumpThru),
    "error" => Ahorn.EntityPlacement(MaggyJumpThru),
    "fatal" => Ahorn.EntityPlacement(MaggyJumpThru),
    "heart" => Ahorn.EntityPlacement(MaggyJumpThru),
    "moon" => Ahorn.EntityPlacement(MaggyJumpThru),
    "reflection" => Ahorn.EntityPlacement(MaggyJumpThru),
    "temple" => Ahorn.EntityPlacement(MaggyJumpThru),
    "templeB" => Ahorn.EntityPlacement(MaggyJumpThru),
    "falling" => Ahorn.EntityPlacement(MaggyJumpThru),
    "sinking" => Ahorn.EntityPlacement(MaggyJumpThru),
    "moving" => Ahorn.EntityPlacement(MaggyJumpThru)
)

function Ahorn.selection(entity::MaggyJumpThru)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MaggyJumpThru, room::Maple.Room)
    Ahorn.drawSprite(ctx, "wood", entity.x, entity.y)
end

end

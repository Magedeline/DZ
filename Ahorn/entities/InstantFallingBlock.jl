module InstantFallingBlock

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/InstantFallingBlock" InstantFallingBlock(x::Integer, y::Integer, behind::Bool=false, climbFall::Bool=true, fallDelay::Number=0.0, fallOnShake::Bool=false, fallOnTouch::Bool=true, finalBoss::Bool=false, height::Integer=16, respawnTime::Number=5.0, shakeIntensity::Number=0.0, tiletype::String="3", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(InstantFallingBlock),
    "shake_activated" => Ahorn.EntityPlacement(InstantFallingBlock),
    "boss_block" => Ahorn.EntityPlacement(InstantFallingBlock)
)

function Ahorn.selection(entity::InstantFallingBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::InstantFallingBlock, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/IngesteHelper/instant_falling_block", entity.x, entity.y)
end

end

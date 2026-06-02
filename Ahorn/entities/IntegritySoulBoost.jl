module IntegritySoulBoost

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/IntegritySoulBoost" IntegritySoulBoost(x::Integer, y::Integer, abilityDuration::Number=2.0, allowWallBounce::Bool=true, boostSpeed::Number=480.0, canSkip::Bool=false, lockCamera::Bool=true, momentumDuration::Number=3.0, oneUse::Bool=false, speedMultiplier::Number=1.8)

const placements = Ahorn.PlacementDict(
    "integrity_soul_boost" => Ahorn.EntityPlacement(IntegritySoulBoost)
)

function Ahorn.selection(entity::IntegritySoulBoost)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::IntegritySoulBoost, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

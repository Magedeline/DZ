module DeterminationSoulBoost

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/DeterminationSoulBoost" DeterminationSoulBoost(x::Integer, y::Integer, abilityDuration::Number=3.0, boostSpeed::Number=320.0, canSkip::Bool=false, dashPowerMultiplier::Number=1.5, extraDashes::Integer=2, lockCamera::Bool=true, oneUse::Bool=false)

const placements = Ahorn.PlacementDict(
    "determination_soul_boost" => Ahorn.EntityPlacement(DeterminationSoulBoost)
)

function Ahorn.selection(entity::DeterminationSoulBoost)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DeterminationSoulBoost, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

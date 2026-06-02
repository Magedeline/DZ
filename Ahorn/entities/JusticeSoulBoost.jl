module JusticeSoulBoost

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/JusticeSoulBoost" JusticeSoulBoost(x::Integer, y::Integer, abilityDuration::Number=2.0, boostSpeed::Number=320.0, canSkip::Bool=false, lockCamera::Bool=true, oneUse::Bool=false, projectileCount::Integer=5, projectileSpeed::Number=400.0, spreadShot::Bool=true)

const placements = Ahorn.PlacementDict(
    "justice_soul_boost" => Ahorn.EntityPlacement(JusticeSoulBoost)
)

function Ahorn.selection(entity::JusticeSoulBoost)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::JusticeSoulBoost, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

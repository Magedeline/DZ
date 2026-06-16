module BraverySoulBoost

using ..Ahorn, Maple

@mapdef Entity "DZ/BraverySoulBoost" BraverySoulBoost(x::Integer, y::Integer, abilityDuration::Number=2.5, boostSpeed::Number=360.0, breakSpinners::Bool=true, canSkip::Bool=false, invincibilityDuration::Number=1.5, lockCamera::Bool=true, oneUse::Bool=false)

const placements = Ahorn.PlacementDict(
    "bravery_soul_boost" => Ahorn.EntityPlacement(BraverySoulBoost)
)

function Ahorn.selection(entity::BraverySoulBoost)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BraverySoulBoost, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

module PerseveranceSoulBoost

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/PerseveranceSoulBoost" PerseveranceSoulBoost(x::Integer, y::Integer, abilityDuration::Number=4.0, autoClimb::Bool=true, boostSpeed::Number=300.0, canSkip::Bool=false, enduranceDuration::Number=5.0, lockCamera::Bool=true, oneUse::Bool=false, staminaRegen::Number=20.0)

const placements = Ahorn.PlacementDict(
    "perseverance_soul_boost" => Ahorn.EntityPlacement(PerseveranceSoulBoost)
)

function Ahorn.selection(entity::PerseveranceSoulBoost)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PerseveranceSoulBoost, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

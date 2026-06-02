module KindnessSoulBoost

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/KindnessSoulBoost" KindnessSoulBoost(x::Integer, y::Integer, abilityDuration::Number=4.0, boostSpeed::Number=280.0, canSkip::Bool=false, fullRestore::Bool=true, lockCamera::Bool=true, oneUse::Bool=false, shieldDuration::Number=5.0)

const placements = Ahorn.PlacementDict(
    "kindness_soul_boost" => Ahorn.EntityPlacement(KindnessSoulBoost)
)

function Ahorn.selection(entity::KindnessSoulBoost)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KindnessSoulBoost, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

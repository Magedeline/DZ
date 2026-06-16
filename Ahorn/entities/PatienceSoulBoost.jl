module PatienceSoulBoost

using ..Ahorn, Maple

@mapdef Entity "DZ/PatienceSoulBoost" PatienceSoulBoost(x::Integer, y::Integer, abilityDuration::Number=3.0, boostSpeed::Number=240.0, canSkip::Bool=false, extendedAirTime::Number=1.5, lockCamera::Bool=true, oneUse::Bool=false, slowdownAmount::Number=0.5, slowdownDuration::Number=2.0)

const placements = Ahorn.PlacementDict(
    "patience_soul_boost" => Ahorn.EntityPlacement(PatienceSoulBoost)
)

function Ahorn.selection(entity::PatienceSoulBoost)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PatienceSoulBoost, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

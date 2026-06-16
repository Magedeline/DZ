module SevenSoulBoost

using ..Ahorn, Maple

@mapdef Entity "DZ/SevenSoulBoost" SevenSoulBoost(x::Integer, y::Integer, boostSpeed::Number=320.0, canSkip::Bool=false, dashCount::Integer=10, finalCh210Dialog::String="", finalCh21Boost::Bool=false, finalCh21Dialog::String="", finalCh21GoldenBoost::Bool=false, lockCamera::Bool=true, oneUse::Bool=false, refillDashes::Bool=true, refillStamina::Bool=true)

const placements = Ahorn.PlacementDict(
    "seven_soul_boost" => Ahorn.EntityPlacement(SevenSoulBoost),
    "seven_soul_boost_one_use" => Ahorn.EntityPlacement(SevenSoulBoost),
    "seven_soul_boost_ch21_final" => Ahorn.EntityPlacement(SevenSoulBoost),
    "seven_soul_boost_ch21_golden" => Ahorn.EntityPlacement(SevenSoulBoost)
)

function Ahorn.selection(entity::SevenSoulBoost)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SevenSoulBoost, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

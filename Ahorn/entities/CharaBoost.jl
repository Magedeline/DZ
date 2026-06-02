module CharaBoost

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/CharaBoost" CharaBoost(x::Integer, y::Integer, canSkip::Bool=false, finalCh19Boost::Bool=false, finalCh19Dialog::String="", finalCh19GoldenBoost::Bool=false, finalCh19PPBoost::Bool=false, lockCamera::Bool=true)

const placements = Ahorn.PlacementDict(
    "chara_boost" => Ahorn.EntityPlacement(CharaBoost)
)

function Ahorn.selection(entity::CharaBoost)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CharaBoost, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/charaboost/idle00", entity.x, entity.y)
end

end

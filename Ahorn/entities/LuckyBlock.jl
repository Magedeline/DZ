module LuckyBlock

using ..Ahorn, Maple

@mapdef Entity "DZ/LuckyBlock" LuckyBlock(x::Integer, y::Integer, height::Integer=16, maxHits::Integer=3, rewardType::String="coin", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "luckyblock" => Ahorn.EntityPlacement(LuckyBlock),
    "luckyblock_single_use" => Ahorn.EntityPlacement(LuckyBlock)
)

function Ahorn.selection(entity::LuckyBlock)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::LuckyBlock, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

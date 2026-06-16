module WarpStar

using ..Ahorn, Maple

@mapdef Entity "DZ/WarpStar" WarpStar(x::Integer, y::Integer, isKirbyWarpStar::Bool=false, oneUse::Bool=false, shielded::Bool=false)

const placements = Ahorn.PlacementDict(
    "WarpStar" => Ahorn.EntityPlacement(WarpStar),
    "one_use" => Ahorn.EntityPlacement(WarpStar),
    "kirby_warp_star" => Ahorn.EntityPlacement(WarpStar),
    "kirby_one_use" => Ahorn.EntityPlacement(WarpStar)
)

function Ahorn.selection(entity::WarpStar)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::WarpStar, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/Maggy/DesoloZantas/warpstars/idle00", entity.x, entity.y)
end

end

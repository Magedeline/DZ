module ShopKeeper

using ..Ahorn, Maple

@mapdef Entity "DZ/ShopKeeper" ShopKeeper(x::Integer, y::Integer, shopId::String="shop_1")

const placements = Ahorn.PlacementDict(
    "ShopKeeper" => Ahorn.EntityPlacement(ShopKeeper)
)

function Ahorn.selection(entity::ShopKeeper)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ShopKeeper, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/magolor/idle00", entity.x, entity.y)
end

end

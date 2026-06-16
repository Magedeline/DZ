module RalseiDummy

using ..Ahorn, Maple

@mapdef Entity "DZ/RalseiDummy" RalseiDummy(x::Integer, y::Integer, alpha::Number=1.0, animation::String="idle", facing::Integer=1, isVisible::Bool=true, playAnimationOnSpawn::Bool=true, scale::Number=1.0)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(RalseiDummy),
    "campfire" => Ahorn.EntityPlacement(RalseiDummy),
    "starjump" => Ahorn.EntityPlacement(RalseiDummy)
)

function Ahorn.selection(entity::RalseiDummy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RalseiDummy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/ralsei/ralsei00", entity.x, entity.y)
end

end

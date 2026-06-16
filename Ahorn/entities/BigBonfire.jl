module BigBonfire

using ..Ahorn, Maple

@mapdef Entity "DZ/BigBonfire" BigBonfire(x::Integer, y::Integer, bloomRadius::Number=64.0, lightInner::Number=64.0, lightOuter::Number=128.0, mode::String="Unlit", scale::Number=2.0)

const placements = Ahorn.PlacementDict(
    "unlit" => Ahorn.EntityPlacement(BigBonfire),
    "lit" => Ahorn.EntityPlacement(BigBonfire),
    "smoking" => Ahorn.EntityPlacement(BigBonfire)
)

function Ahorn.selection(entity::BigBonfire)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::BigBonfire, room::Maple.Room)
    Ahorn.drawSprite(ctx, "objects/campfire/idle00", entity.x, entity.y)
end

end

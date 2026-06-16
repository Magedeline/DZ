module PhantomKnight

using ..Ahorn, Maple

@mapdef Entity "DZ/PhantomKnight" PhantomKnight(x::Integer, y::Integer, attackTime::Number=0.5, health::Integer=3, hiddenTime::Number=2.0)

const placements = Ahorn.PlacementDict(
    "PhantomKnight" => Ahorn.EntityPlacement(PhantomKnight),
    "PhantomKnightAggressive" => Ahorn.EntityPlacement(PhantomKnight)
)

function Ahorn.selection(entity::PhantomKnight)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PhantomKnight, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

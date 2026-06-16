module DarkMatterMinion

using ..Ahorn, Maple

@mapdef Entity "DZ/DarkMatterMinion" DarkMatterMinion(x::Integer, y::Integer, fireInterval::Number=2.5, health::Integer=2)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(DarkMatterMinion),
    "long_range" => Ahorn.EntityPlacement(DarkMatterMinion)
)

function Ahorn.selection(entity::DarkMatterMinion)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DarkMatterMinion, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

module FountainSpirit

using ..Ahorn, Maple

@mapdef Entity "DZ/FountainSpirit" FountainSpirit(x::Integer, y::Integer, buffDuration::Number=10.0, healAmount::Integer=3, spiritType::String="Healing")

const placements = Ahorn.PlacementDict(
    "healing" => Ahorn.EntityPlacement(FountainSpirit),
    "buff" => Ahorn.EntityPlacement(FountainSpirit)
)

function Ahorn.selection(entity::FountainSpirit)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FountainSpirit, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

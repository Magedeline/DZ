module AlphaApexPredatorBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/AlphaApexPredatorBoss" AlphaApexPredatorBoss(x::Integer, y::Integer, health::Integer=1600, maxHealth::Integer=1600)

const placements = Ahorn.PlacementDict(
    "alpha_apex_predator_boss" => Ahorn.EntityPlacement(AlphaApexPredatorBoss)
)

function Ahorn.selection(entity::AlphaApexPredatorBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::AlphaApexPredatorBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

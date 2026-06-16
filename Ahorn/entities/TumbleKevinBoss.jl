module TumbleKevinBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/TumbleKevinBoss" TumbleKevinBoss(x::Integer, y::Integer, health::Integer=1200, maxHealth::Integer=1200)

const placements = Ahorn.PlacementDict(
    "tumble_kevin_boss" => Ahorn.EntityPlacement(TumbleKevinBoss)
)

function Ahorn.selection(entity::TumbleKevinBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TumbleKevinBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

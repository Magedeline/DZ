module SansBoss

using ..Ahorn, Maple

@mapdef Entity "DZ/SansBoss" SansBoss(x::Integer, y::Integer, health::Integer=1100, maxHealth::Integer=1100)

const placements = Ahorn.PlacementDict(
    "sans_boss" => Ahorn.EntityPlacement(SansBoss)
)

function Ahorn.selection(entity::SansBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SansBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

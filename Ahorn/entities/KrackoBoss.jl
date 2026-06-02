module KrackoBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/KrackoBoss" KrackoBoss(x::Integer, y::Integer, health::Integer=900, maxHealth::Integer=900)

const placements = Ahorn.PlacementDict(
    "kracko_boss" => Ahorn.EntityPlacement(KrackoBoss)
)

function Ahorn.selection(entity::KrackoBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KrackoBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

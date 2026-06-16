module CustomEnemy

using ..Ahorn, Maple

@mapdef Entity "DoonvHelper/CustomEnemy" CustomEnemy(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "CustomEnemy" => Ahorn.EntityPlacement(CustomEnemy)
)

function Ahorn.selection(entity::CustomEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CustomEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

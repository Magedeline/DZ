module HealerEnemy

using ..Ahorn, Maple

@mapdef Entity "DZ/HealerEnemy" HealerEnemy(x::Integer, y::Integer, healRange::Number=80.0, health::Integer=1)

const placements = Ahorn.PlacementDict(
    "HealerEnemy" => Ahorn.EntityPlacement(HealerEnemy)
)

function Ahorn.selection(entity::HealerEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HealerEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

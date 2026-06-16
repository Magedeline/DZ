module MirrorEnemy

using ..Ahorn, Maple

@mapdef Entity "DZ/MirrorEnemy" MirrorEnemy(x::Integer, y::Integer, health::Integer=2)

const placements = Ahorn.PlacementDict(
    "MirrorEnemy" => Ahorn.EntityPlacement(MirrorEnemy)
)

function Ahorn.selection(entity::MirrorEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MirrorEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

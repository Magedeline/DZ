module ScarfyEnemy

using ..Ahorn, Maple

@mapdef Entity "DZ/ScarfyEnemy" ScarfyEnemy(x::Integer, y::Integer, canBeInhaled::Bool=false, chaseSpeed::Number=100.0, health::Integer=2, moveSpeed::Number=20.0)

const placements = Ahorn.PlacementDict(
    "ScarfyEnemy" => Ahorn.EntityPlacement(ScarfyEnemy)
)

function Ahorn.selection(entity::ScarfyEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ScarfyEnemy, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

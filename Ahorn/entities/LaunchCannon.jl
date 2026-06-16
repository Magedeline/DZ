module LaunchCannon

using ..Ahorn, Maple

@mapdef Entity "DZ/LaunchCannon" LaunchCannon(x::Integer, y::Integer, autoFire::Bool=false, launchSpeed::Number=400.0)

const placements = Ahorn.PlacementDict(
    "manual_aim" => Ahorn.EntityPlacement(LaunchCannon),
    "auto_up" => Ahorn.EntityPlacement(LaunchCannon),
    "auto_right" => Ahorn.EntityPlacement(LaunchCannon),
    "auto_left" => Ahorn.EntityPlacement(LaunchCannon),
    "auto_diagonal_up_right" => Ahorn.EntityPlacement(LaunchCannon)
)

function Ahorn.selection(entity::LaunchCannon)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::LaunchCannon, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

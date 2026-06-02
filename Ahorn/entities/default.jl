module default

using ..Ahorn, Maple

@mapdef Entity "default" default(x::Integer, y::Integer, introType::String="None", inventory::String="KirbyKglobal::Player", maxHealth::Integer=6, power::String="None", useSpawnPoints::Bool=true)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(default),
    "walk_in" => Ahorn.EntityPlacement(default),
    "warp_star" => Ahorn.EntityPlacement(default),
    "float_down" => Ahorn.EntityPlacement(default),
    "with_fire" => Ahorn.EntityPlacement(default),
    "with_sword" => Ahorn.EntityPlacement(default),
    "knight_mode" => Ahorn.EntityPlacement(default)
)

function Ahorn.selection(entity::default)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::default, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

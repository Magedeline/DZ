module KirbyKglobalPlayerExtension

using ..Ahorn, Maple

@mapdef Entity "DZ/KirbyKglobal::PlayerExtension" KirbyKglobalPlayerExtension(x::Integer, y::Integer, introType::String="None", inventory::String="KirbyKglobal::Player", maxHealth::Integer=6, power::String="None", useSpawnPoints::Bool=true)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(KirbyKglobalPlayerExtension),
    "walk_in" => Ahorn.EntityPlacement(KirbyKglobalPlayerExtension),
    "warp_star" => Ahorn.EntityPlacement(KirbyKglobalPlayerExtension),
    "float_down" => Ahorn.EntityPlacement(KirbyKglobalPlayerExtension),
    "with_fire" => Ahorn.EntityPlacement(KirbyKglobalPlayerExtension),
    "with_sword" => Ahorn.EntityPlacement(KirbyKglobalPlayerExtension),
    "knight_mode" => Ahorn.EntityPlacement(KirbyKglobalPlayerExtension)
)

function Ahorn.selection(entity::KirbyKglobalPlayerExtension)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyKglobalPlayerExtension, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/kirby/idle00", entity.x, entity.y)
end

end

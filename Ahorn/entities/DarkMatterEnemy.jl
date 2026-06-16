module DarkMatterEnemy

using ..Ahorn, Maple

@mapdef Entity "DZ/DarkMatterEnemy" DarkMatterEnemy(x::Integer, y::Integer, health::Integer=8, maxDamage::Integer=4, minDamage::Integer=2, patrolRadius::Integer=64)

const placements = Ahorn.PlacementDict(
    "dark_matter_weak" => Ahorn.EntityPlacement(DarkMatterEnemy),
    "dark_matter_normal" => Ahorn.EntityPlacement(DarkMatterEnemy),
    "dark_matter_strong" => Ahorn.EntityPlacement(DarkMatterEnemy)
)

function Ahorn.selection(entity::DarkMatterEnemy)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DarkMatterEnemy, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/darkmatter/idle00", entity.x, entity.y)
end

end

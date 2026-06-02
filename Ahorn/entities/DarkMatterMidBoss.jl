module DarkMatterMidBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/DarkMatterMidBoss" DarkMatterMidBoss(x::Integer, y::Integer, health::Integer=80)

const placements = Ahorn.PlacementDict(
    "dark_matter_mid_boss" => Ahorn.EntityPlacement(DarkMatterMidBoss),
    "dark_matter_mid_boss_hard" => Ahorn.EntityPlacement(DarkMatterMidBoss)
)

function Ahorn.selection(entity::DarkMatterMidBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DarkMatterMidBoss, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/darkmatter_boss/idle00", entity.x, entity.y)
end

end

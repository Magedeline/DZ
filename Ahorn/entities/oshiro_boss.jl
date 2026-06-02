module oshiro_boss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/oshiro_boss" oshiro_boss(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "oshiro_boss" => Ahorn.EntityPlacement(oshiro_boss)
)

function Ahorn.selection(entity::oshiro_boss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::oshiro_boss, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/oshiro/boss13", entity.x, entity.y)
end

end

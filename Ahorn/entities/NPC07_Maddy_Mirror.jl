module NPC07_Maddy_Mirror

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/NPC07_Maddy_Mirror" NPC07_Maddy_Mirror(x::Integer, y::Integer, dialogKey::String="ingeste_maddy_07_mirror", flagName::String="maddy_07_mirror", spriteId::String="madeline")

const placements = Ahorn.PlacementDict(
    "NPC07_Maddy_Mirror" => Ahorn.EntityPlacement(NPC07_Maddy_Mirror)
)

function Ahorn.selection(entity::NPC07_Maddy_Mirror)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC07_Maddy_Mirror, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/madeline/idle00", entity.x, entity.y)
end

end

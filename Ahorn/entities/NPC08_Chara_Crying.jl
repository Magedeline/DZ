module NPC08_Chara_Crying

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC08_Chara_Crying" NPC08_Chara_Crying(x::Integer, y::Integer, dialogKey::String="ingeste_chara_08_crying", flagName::String="chara_08_crying", spriteId::String="chara")

const placements = Ahorn.PlacementDict(
    "NPC08_Chara_Crying" => Ahorn.EntityPlacement(NPC08_Chara_Crying)
)

function Ahorn.selection(entity::NPC08_Chara_Crying)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC08_Chara_Crying, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/chara/idle00", entity.x, entity.y)
end

end

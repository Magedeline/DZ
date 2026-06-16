module NPC07_Chara

using ..Ahorn, Maple

@mapdef Entity "DZ/NPC07_Chara" NPC07_Chara(x::Integer, y::Integer, dialogKey::String="ingeste_chara_07_hell", entityId::String="chara_07_hell", flagName::String="chara_07_hell", index::Integer=0, spriteId::String="chara")

const placements = Ahorn.PlacementDict(
    "NPC07_Chara" => Ahorn.EntityPlacement(NPC07_Chara)
)

function Ahorn.selection(entity::NPC07_Chara)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC07_Chara, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/chara/idle00", entity.x, entity.y)
end

end

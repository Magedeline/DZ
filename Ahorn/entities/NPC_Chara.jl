module NPC_Chara

using ..Ahorn, Maple

@mapdef Entity "DesoloZantas/NPC_Chara" NPC_Chara(x::Integer, y::Integer, dialogKey::String="CH2_CHARA_INTRO", enabledByDefault::Bool=true, flagName::String="chara_met")

const placements = Ahorn.PlacementDict(
    "NPC_Chara" => Ahorn.EntityPlacement(NPC_Chara),
    "NPC_Chara_Second_Encounter" => Ahorn.EntityPlacement(NPC_Chara),
    "NPC_Chara_Payphone" => Ahorn.EntityPlacement(NPC_Chara)
)

function Ahorn.selection(entity::NPC_Chara)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPC_Chara, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/chara/idle00", entity.x, entity.y)
end

end

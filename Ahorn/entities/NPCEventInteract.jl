module NPCEventInteract

using ..Ahorn, Maple

@mapdef Entity "DZ/NPCEventInteract" NPCEventInteract(x::Integer, y::Integer, alternateSprite::String="kirby", csFlag::String="npc00_kirby_met", dialogKey::String="CH1_KIRBY_INTRO", enableSpriteSwap::Bool=false, hasLight::Bool=true, npcName::String="Kirby", npcType::String="NPC00_Kirby_Start", requireFlag::String="", spriteId::String="kirby", triggerFlag::String="met_kirby")

const placements = Ahorn.PlacementDict(
    "NPC00 - Kirby Start" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC01 - Magolor Intro" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC02 - Chara First" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC03 - Maggy" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC04 - Ralsei Mirror" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC05 - Oshiro Breakdown" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC06 - Gaster Lab" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC07 - Maddy Mirror" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC08 - Maggy Ending" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC08 - Madeline Plateau" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC08 - Maddy and Theo Ending" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC08 - Chara Crying" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC09 - Sans Phone" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC09 - Fake Save Point" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC09 - Fake Save Point (Stage A)" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC09 - Fake Save Point (Stage B)" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC09 - Fake Save Point (Stage C)" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC09 - Fake Save Point (Stage D)" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC09 - Fake Save Point (Trap)" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC10 - Flowey Ruins" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC11 - Papyrus Voice" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC12 - Undyne Battle" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC13 - Asgore Core" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC14 - Alphys Lab" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC15 - Mettaton Show" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC16 - Toriel" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC16 - Theo" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC16 - Oshiro" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC16 - Kirby" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC17 - Toriel Outside" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC17 - Toriel Inside" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC19 - Maggy Loop" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC19 - Gravestone" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC20 - Madeline" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC20 - Granny" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC20 - Asriel" => Ahorn.EntityPlacement(NPCEventInteract),
    "NPC21 - Final Boss" => Ahorn.EntityPlacement(NPCEventInteract)
)

function Ahorn.selection(entity::NPCEventInteract)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPCEventInteract, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/Kglobal::Player/idle00", entity.x, entity.y)
end

end

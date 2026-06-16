module NPCWithNodes

using ..Ahorn, Maple

@mapdef Entity "DZ/NPCWithNodes" NPCWithNodes(x::Integer, y::Integer, actionTriggers::String="interaction", animationIdle::String="idle", animationTalk::String="talk", animationWalk::String="walk", canInteractWhileMoving::Bool=false, faceMovementDirection::Bool=true, flagToSet::String="met_patrol_theo", interactionDialog::String="THEO_PATROL", interactionRange::Integer=48, loopActions::Bool=true, movementSpeed::Integer=50, movementType::String="patrol", npcName::String="Theo", removeAfterInteraction::Bool=false, requiredFlag::String="", soundFootsteps::String="", soundInteraction::String="", spriteId::String="theo", waitTimeAtNodes::Number=2.0)

const placements = Ahorn.PlacementDict(
    "patrol_npc" => Ahorn.EntityPlacement(NPCWithNodes),
    "quest_giver_npc" => Ahorn.EntityPlacement(NPCWithNodes),
    "guide_npc" => Ahorn.EntityPlacement(NPCWithNodes),
    "boss_npc" => Ahorn.EntityPlacement(NPCWithNodes),
    "cutscene_npc" => Ahorn.EntityPlacement(NPCWithNodes)
)

function Ahorn.selection(entity::NPCWithNodes)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::NPCWithNodes, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/theo/idle00", entity.x, entity.y)
end

end

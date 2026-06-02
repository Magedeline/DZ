module ElsFloweyBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/ElsFloweyBoss" ElsFloweyBoss(x::Integer, y::Integer, allowMercy::Bool=true, autoStartBattle::Bool=false, corruptionLevel::Number=5.0, enableBonePhase::Bool=true, enableFinalForm::Bool=true, enableFleshPhase::Bool=true, enableOrganicPhase::Bool=true, enableSoulCollection::Bool=true, enableSpecialAttacks::Bool=true, enableWeaponPhase::Bool=true, isVulnerable::Bool=false, maxHealth::Number=1000.0, nightmareIntensity::Number=3.0, showHealthBar::Bool=true, startingPhase::String="Intro")

const placements = Ahorn.PlacementDict(
    "elsflowey" => Ahorn.EntityPlacement(ElsFloweyBoss),
    "no_mercy" => Ahorn.EntityPlacement(ElsFloweyBoss)
)

function Ahorn.selection(entity::ElsFloweyBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ElsFloweyBoss, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

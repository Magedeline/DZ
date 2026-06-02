module ElsTrueFinalBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/ElsTrueFinalBoss" ElsTrueFinalBoss(x::Integer, y::Integer, attackSequence::String="", dialog::Bool=true, patternIndex::Integer=4, siamoTier::String="soulBlack", siamoVariant::String="zero", startHit::Bool=false)

const placements = Ahorn.PlacementDict(
    "siamo_zero" => Ahorn.EntityPlacement(ElsTrueFinalBoss),
    "siamo_zero_aeon" => Ahorn.EntityPlacement(ElsTrueFinalBoss),
    "siamo_zero_morpho" => Ahorn.EntityPlacement(ElsTrueFinalBoss),
    "siamo_zero_pink" => Ahorn.EntityPlacement(ElsTrueFinalBoss),
    "siamo_zero_soul_black" => Ahorn.EntityPlacement(ElsTrueFinalBoss),
    "siamo_zero_stellarruss" => Ahorn.EntityPlacement(ElsTrueFinalBoss),
    "siamo_zero_delta_extra" => Ahorn.EntityPlacement(ElsTrueFinalBoss),
    "celestial_zero_remix" => Ahorn.EntityPlacement(ElsTrueFinalBoss),
    "siamo_zero_ultimate_chain" => Ahorn.EntityPlacement(ElsTrueFinalBoss)
)

function Ahorn.selection(entity::ElsTrueFinalBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ElsTrueFinalBoss, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/els_true_final_boss/boss00", entity.x, entity.y)
end

end

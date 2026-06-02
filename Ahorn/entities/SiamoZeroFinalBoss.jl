module SiamoZeroFinalBoss

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/SiamoZeroFinalBoss" SiamoZeroFinalBoss(x::Integer, y::Integer, attackSequence::String="", dialog::Bool=true, patternIndex::Integer=4, siamoTier::String="soulBlack", siamoVariant::String="zero", startHit::Bool=false)

const placements = Ahorn.PlacementDict(
    "siamo_zero" => Ahorn.EntityPlacement(SiamoZeroFinalBoss),
    "siamo_zero_aeon" => Ahorn.EntityPlacement(SiamoZeroFinalBoss),
    "siamo_zero_morpho" => Ahorn.EntityPlacement(SiamoZeroFinalBoss),
    "siamo_zero_pink" => Ahorn.EntityPlacement(SiamoZeroFinalBoss),
    "siamo_zero_soul_black" => Ahorn.EntityPlacement(SiamoZeroFinalBoss),
    "siamo_zero_stellarruss" => Ahorn.EntityPlacement(SiamoZeroFinalBoss),
    "siamo_zero_delta_extra" => Ahorn.EntityPlacement(SiamoZeroFinalBoss),
    "celestial_zero_remix" => Ahorn.EntityPlacement(SiamoZeroFinalBoss),
    "siamo_zero_ultimate_chain" => Ahorn.EntityPlacement(SiamoZeroFinalBoss)
)

function Ahorn.selection(entity::SiamoZeroFinalBoss)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SiamoZeroFinalBoss, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/els_true_final_boss/boss00", entity.x, entity.y)
end

end

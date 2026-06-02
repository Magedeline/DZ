module LostSoul

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/LostSoul" LostSoul(x::Integer, y::Integer, autoCollectable::Bool=false, canBeHealed::Bool=true, corruption::Number=0.0, dialogKey::String="CH16_LOST_SOULS_1", enableParticles::Bool=true, floatingHeight::Number=20.0, floatingSpeed::Number=1.0, glowIntensity::Number=0.8, message::String="Help... us...", requiresInteraction::Bool=true, soulColor::String="white", soulId::String="LOST_SOUL_1", soulIndex::Integer=1, soulType::String="human")

const placements = Ahorn.PlacementDict(
    "soul_1" => Ahorn.EntityPlacement(LostSoul),
    "soul_patience" => Ahorn.EntityPlacement(LostSoul),
    "soul_bravery" => Ahorn.EntityPlacement(LostSoul),
    "soul_integrity" => Ahorn.EntityPlacement(LostSoul),
    "soul_perseverance" => Ahorn.EntityPlacement(LostSoul),
    "soul_kindness" => Ahorn.EntityPlacement(LostSoul),
    "soul_justice" => Ahorn.EntityPlacement(LostSoul),
    "soul_determination" => Ahorn.EntityPlacement(LostSoul)
)

function Ahorn.selection(entity::LostSoul)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::LostSoul, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

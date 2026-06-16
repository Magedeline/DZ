module Tape

using ..Ahorn, Maple

@mapdef Entity "DZ/Tape" Tape(x::Integer, y::Integer, bloomStrength::Number=0.8, cSideToUnlock::String="map/campaingname/mapname/map.bin", collectSfx::String="", floatRange::Number=2.0, floatSpeed::Number=2.0, glowStrength::Number=1.0, menuSprite::String="collectables/maggy/tape", particleColor::String="FF9CCF", previewEvent::String="event:/pusheen/game/general/cassette_preview", previewParamName::String="remix", previewParamValue::Number=-1.0, spritePath::String="collectables/cassette/", unlockText::String="", wiggleIntensity::Number=0.35)

const placements = Ahorn.PlacementDict(
    "tape" => Ahorn.EntityPlacement(Tape)
)

function Ahorn.selection(entity::Tape)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Tape, room::Maple.Room)
    Ahorn.drawSprite(ctx, "util/pixel", entity.x, entity.y)
end

# Nodes: min=2, max=2
# Basic node rendering not implemented in auto-generated plugin

end

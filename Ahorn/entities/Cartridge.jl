module Cartridge

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/Cartridge" Cartridge(x::Integer, y::Integer, bloomStrength::Number=1.0, collectDelay::Number=0.5, customAudio::String="", floatRange::Number=3.0, floatSpeed::Number=1.5, glowStrength::Number=1.5, isChapter19Finale::Bool=false, menuSprite::String="collectables/cartridge", onCollect::String="", particleColor::String="FFD700", persistent::Bool=true, remixExtraToUnlock::String="", spritePath::String="collectables/cartridge/", unlockText::String="", wiggleIntensity::Number=0.5)

const placements = Ahorn.PlacementDict(
    "cartridge" => Ahorn.EntityPlacement(Cartridge)
)

function Ahorn.selection(entity::Cartridge)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Cartridge, room::Maple.Room)
    Ahorn.drawSprite(ctx, "collectables/cartridge/idle00", entity.x, entity.y)
end

end

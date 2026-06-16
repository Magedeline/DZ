module Cassette

using ..Ahorn, Maple

@mapdef Entity "DZ/Cassette" Cassette(x::Integer, y::Integer, bloomStrength::Number=0.8, collectDelay::Number=0.3, customAudio::String="", floatRange::Number=2.0, floatSpeed::Number=2.0, glowStrength::Number=1.0, onCollect::String="", particleColor::String="9CFCFF", persistent::Bool=true, wiggleIntensity::Number=0.35)

const placements = Ahorn.PlacementDict(
    "Cassette" => Ahorn.EntityPlacement(Cassette)
)

function Ahorn.selection(entity::Cassette)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Cassette, room::Maple.Room)
    Ahorn.drawSprite(ctx, "collectables/cassette/idle00", entity.x, entity.y)
end

# Nodes: min=0, max=3
# Basic node rendering not implemented in auto-generated plugin

end

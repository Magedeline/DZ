module ElsGlitchRainbowBlackholeTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/ElsGlitchRainbowBlackholeTrigger" ElsGlitchRainbowBlackholeTrigger(x::Integer, y::Integer, blackholeStrength::String="Medium", changeBlackholeStrength::Bool=true, doGlitch::Bool=true, glitchDuration::String="Short", height::Integer=16, sessionFlag::String="", stayOn::Bool=false, triggerOnce::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "short_glitch_medium_strength" => Ahorn.EntityPlacement(ElsGlitchRainbowBlackholeTrigger),
    "medium_glitch_high_strength" => Ahorn.EntityPlacement(ElsGlitchRainbowBlackholeTrigger),
    "long_glitch_wild_strength" => Ahorn.EntityPlacement(ElsGlitchRainbowBlackholeTrigger),
    "insane_chaos" => Ahorn.EntityPlacement(ElsGlitchRainbowBlackholeTrigger),
    "rainbow_chaos" => Ahorn.EntityPlacement(ElsGlitchRainbowBlackholeTrigger),
    "cosmic" => Ahorn.EntityPlacement(ElsGlitchRainbowBlackholeTrigger),
    "strength_only_no_glitch" => Ahorn.EntityPlacement(ElsGlitchRainbowBlackholeTrigger),
    "glitch_only_no_strength_change" => Ahorn.EntityPlacement(ElsGlitchRainbowBlackholeTrigger)
)

function Ahorn.selection(entity::ElsGlitchRainbowBlackholeTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ElsGlitchRainbowBlackholeTrigger, room::Maple.Room)
    Ahorn.drawSprite(ctx, "ahorn/entityTrigger", entity.x, entity.y)
end

end

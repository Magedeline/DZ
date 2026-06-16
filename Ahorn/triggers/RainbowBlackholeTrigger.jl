module RainbowBlackholeTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/RainbowBlackholeTrigger" RainbowBlackholeTrigger(x::Integer, y::Integer, action::String="Enable", alpha::Number=1.0, direction::Number=1.0, fadeTime::Number=1.0, flag::String="", height::Integer=16, onlyIfFlag::Bool=false, scale::Number=1.0, strength::String="Medium", triggerOnce::Bool=false, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "enable" => Ahorn.EntityPlacement(RainbowBlackholeTrigger),
    "disable" => Ahorn.EntityPlacement(RainbowBlackholeTrigger),
    "change_strength" => Ahorn.EntityPlacement(RainbowBlackholeTrigger),
    "set_alpha" => Ahorn.EntityPlacement(RainbowBlackholeTrigger),
    "set_scale" => Ahorn.EntityPlacement(RainbowBlackholeTrigger),
    "set_direction" => Ahorn.EntityPlacement(RainbowBlackholeTrigger),
    "toggle" => Ahorn.EntityPlacement(RainbowBlackholeTrigger)
)

function Ahorn.selection(entity::RainbowBlackholeTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RainbowBlackholeTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

module Kirby_Mode_Toggle_Trigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/Kirby_Mode_Toggle_Trigger" Kirby_Mode_Toggle_Trigger(x::Integer, y::Integer, activationMode::String="OnEnter", effectDuration::Number=1.0, flagRequired::String="", flagToSet::String="", height::Integer=16, initialPower::String="None", oneUse::Bool=false, particleColor::String="FFC0CB", particleCount::Integer=30, playSound::Bool=true, respectSettings::Bool=true, screenShake::Bool=true, shakeIntensity::Number=0.3, silentMode::Bool=false, transformEffect::String="Sparkle", transformSound::String="event:/pusheen/char/kirby/transform", triggerState::String="Enable", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "enable_kirby_on_enter" => Ahorn.EntityPlacement(Kirby_Mode_Toggle_Trigger),
    "disable_kirby_on_enter" => Ahorn.EntityPlacement(Kirby_Mode_Toggle_Trigger),
    "toggle_kirby_mode" => Ahorn.EntityPlacement(Kirby_Mode_Toggle_Trigger),
    "enable_fire_power" => Ahorn.EntityPlacement(Kirby_Mode_Toggle_Trigger),
    "enable_sword_power" => Ahorn.EntityPlacement(Kirby_Mode_Toggle_Trigger),
    "enable_knight_power" => Ahorn.EntityPlacement(Kirby_Mode_Toggle_Trigger),
    "one_use_enable" => Ahorn.EntityPlacement(Kirby_Mode_Toggle_Trigger),
    "persistent_enable" => Ahorn.EntityPlacement(Kirby_Mode_Toggle_Trigger),
    "silent_toggle" => Ahorn.EntityPlacement(Kirby_Mode_Toggle_Trigger),
    "custom_effect_toggle" => Ahorn.EntityPlacement(Kirby_Mode_Toggle_Trigger),
    "disable_on_exit" => Ahorn.EntityPlacement(Kirby_Mode_Toggle_Trigger)
)

function Ahorn.selection(entity::Kirby_Mode_Toggle_Trigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Kirby_Mode_Toggle_Trigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

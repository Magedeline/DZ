module Kirby_Player_Trigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/Kirby_Player_Trigger" Kirby_Player_Trigger(x::Integer, y::Integer, activationType::String="OnEnter", height::Integer=16, initialPower::String="None", oneUse::Bool=false, playSound::Bool=true, preserveVelocity::Bool=true, requiredFlag::String="", transformAnimation::String="transform_to_kirby", transformDuration::Number=1.0, transformationType::String="Animated", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "on_enter" => Ahorn.EntityPlacement(Kirby_Player_Trigger),
    "on_exit" => Ahorn.EntityPlacement(Kirby_Player_Trigger),
    "toggle" => Ahorn.EntityPlacement(Kirby_Player_Trigger),
    "fire_power_enable" => Ahorn.EntityPlacement(Kirby_Player_Trigger),
    "sword_power_enable" => Ahorn.EntityPlacement(Kirby_Player_Trigger),
    "knight_mode_enable" => Ahorn.EntityPlacement(Kirby_Player_Trigger)
)

function Ahorn.selection(entity::Kirby_Player_Trigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::Kirby_Player_Trigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

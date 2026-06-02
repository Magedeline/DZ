module MainCharaVisionActor

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/MainCharaVisionActor" MainCharaVisionActor(x::Integer, y::Integer, PlayerControlled::Bool=false, acceleration::Number=650.0, clampToRoomBounds::Bool=true, driveCameraWhenControlled::Bool=true, facing::String="down", friction::Number=800.0, genocideMode::Bool=true, maxMoveSpeed::Number=90.0)

const placements = Ahorn.PlacementDict(
    "default" => Ahorn.EntityPlacement(MainCharaVisionActor),
    "neutral" => Ahorn.EntityPlacement(MainCharaVisionActor),
    "Kglobal::Player_controlled" => Ahorn.EntityPlacement(MainCharaVisionActor),
    "intro_right" => Ahorn.EntityPlacement(MainCharaVisionActor),
    "finale_left" => Ahorn.EntityPlacement(MainCharaVisionActor),
    "camera_off" => Ahorn.EntityPlacement(MainCharaVisionActor)
)

function Ahorn.selection(entity::MainCharaVisionActor)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::MainCharaVisionActor, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/MaggyHelper/mainchara_vision/down00", entity.x, entity.y)
end

end

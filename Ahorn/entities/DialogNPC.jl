module DialogNPC

using ..Ahorn, Maple

@mapdef Entity "DZ/DialogNPC" DialogNPC(x::Integer, y::Integer, Player::Bool=false, XSpeed::Number=120.0, YSpeed::Number=160.0, aiEnabled::Bool=true, aiType::String="None", basicDialogID::String="", csEventID::String="", cutsceneActive::Bool=false, cutsceneLocked::Bool=false, cutsceneModeEnabled::Bool=false, cutscenePaused::Bool=false, cutscenePlayed::Bool=false, cutsceneSkippable::Bool=true, cutsceneWaitingForInput::Bool=false, hitboxXOffset::Integer=0, hitboxYOffset::Integer=0, isActive::Bool=true, isAirborne::Bool=false, isFriendly::Bool=true, isGrounded::Bool=true, isHostile::Bool=false, isInCutscene::Bool=false, isInteractable::Bool=true, isInvincible::Bool=false, isMoving::Bool=false, isPatrolling::Bool=false, isStunned::Bool=false, isTalking::Bool=false, isVisible::Bool=true, luaCutscene::String="", sprite::String="characters/Maggy/DesoloZantas/Kglobal::Player/idle00", talkBoundsHeight::Integer=32, talkBoundsWidth::Integer=32, talkIndicatorX::Integer=0, talkIndicatorY::Integer=-24)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(DialogNPC),
    "theo_style" => Ahorn.EntityPlacement(DialogNPC),
    "chara_style" => Ahorn.EntityPlacement(DialogNPC),
    "cutscene_npc" => Ahorn.EntityPlacement(DialogNPC)
)

function Ahorn.selection(entity::DialogNPC)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DialogNPC, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/Kglobal::Player/idle00", entity.x, entity.y)
end

end

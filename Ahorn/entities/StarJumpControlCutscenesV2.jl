module StarJumpControlCutscenesV2

using ..Ahorn, Maple

@mapdef Entity "DZ/StarJumpControlCutscenesV2" StarJumpControlCutscenesV2(x::Integer, y::Integer, cutsceneFlag::String="plateaumod_2", musicEvent::String="event:/pusheen/music/lvl8/starjump", triggerHeight::Integer=32, triggerOffsetX::Integer=0, triggerOffsetY::Integer=0, triggerWidth::Integer=64, useCustomTriggerBox::Bool=false)

const placements = Ahorn.PlacementDict(
    "StarJumpControlCutscenesV2" => Ahorn.EntityPlacement(StarJumpControlCutscenesV2)
)

function Ahorn.selection(entity::StarJumpControlCutscenesV2)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StarJumpControlCutscenesV2, room::Maple.Room)
    Ahorn.drawSprite(ctx, "@Internal@/northern_lights", entity.x, entity.y)
end

end

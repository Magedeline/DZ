module CreditsTriggerPart2

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/CreditsTriggerPart2" CreditsTriggerPart2(x::Integer, y::Integer, event::String="WaitJumpDash", height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "wait_jump_dash" => Ahorn.EntityPlacement(CreditsTriggerPart2),
    "wait_jump_double_dash" => Ahorn.EntityPlacement(CreditsTriggerPart2),
    "wait_jump_quintriple_dash" => Ahorn.EntityPlacement(CreditsTriggerPart2),
    "climb_down" => Ahorn.EntityPlacement(CreditsTriggerPart2),
    "wait" => Ahorn.EntityPlacement(CreditsTriggerPart2),
    "badeline_offset" => Ahorn.EntityPlacement(CreditsTriggerPart2),
    "chara_offset" => Ahorn.EntityPlacement(CreditsTriggerPart2),
    "kirby_offset" => Ahorn.EntityPlacement(CreditsTriggerPart2),
    "ralsei_offset" => Ahorn.EntityPlacement(CreditsTriggerPart2),
    "oshiro_marker" => Ahorn.EntityPlacement(CreditsTriggerPart2)
)

function Ahorn.selection(entity::CreditsTriggerPart2)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::CreditsTriggerPart2, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

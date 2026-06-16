module FlashbackTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/FlashbackTrigger" FlashbackTrigger(x::Integer, y::Integer, dialogId::String="", flashbackDuration::Number=5.0, height::Integer=16, targetRoom::String="", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "FlashbackTrigger" => Ahorn.EntityPlacement(FlashbackTrigger)
)

function Ahorn.selection(entity::FlashbackTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FlashbackTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

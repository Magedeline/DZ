module ScreenFlashTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/ScreenFlashTrigger" ScreenFlashTrigger(x::Integer, y::Integer, color::String="ffffff", duration::Number=0.5, height::Integer=16, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "white_flash" => Ahorn.EntityPlacement(ScreenFlashTrigger),
    "red_flash" => Ahorn.EntityPlacement(ScreenFlashTrigger)
)

function Ahorn.selection(entity::ScreenFlashTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ScreenFlashTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

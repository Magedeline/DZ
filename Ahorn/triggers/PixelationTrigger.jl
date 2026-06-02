module PixelationTrigger

using ..Ahorn, Maple

@mapdef Trigger "MaggyHelper/PixelationTrigger" PixelationTrigger(x::Integer, y::Integer, height::Integer=16, pixelSize::Integer=4, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "retro" => Ahorn.EntityPlacement(PixelationTrigger),
    "heavy" => Ahorn.EntityPlacement(PixelationTrigger)
)

function Ahorn.selection(entity::PixelationTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PixelationTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

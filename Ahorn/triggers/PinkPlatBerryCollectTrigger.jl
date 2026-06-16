module PinkPlatBerryCollectTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/PinkPlatBerryCollectTrigger" PinkPlatBerryCollectTrigger(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "PinkPlatBerryCollectTrigger" => Ahorn.EntityPlacement(PinkPlatBerryCollectTrigger)
)

function Ahorn.selection(entity::PinkPlatBerryCollectTrigger)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::PinkPlatBerryCollectTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

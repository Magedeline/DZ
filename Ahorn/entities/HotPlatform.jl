module HotPlatform

using ..Ahorn, Maple

@mapdef Entity "DZ/HotPlatform" HotPlatform(x::Integer, y::Integer, coolRate::Integer=10, heatRate::Integer=20, height::Integer=8, maxHeat::Integer=100, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(HotPlatform)
)

function Ahorn.selection(entity::HotPlatform)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::HotPlatform, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

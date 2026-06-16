module IcePlatform

using ..Ahorn, Maple

@mapdef Entity "DZ/IcePlatform" IcePlatform(x::Integer, y::Integer, friction::Number=0.98, width::Integer=32)

const placements = Ahorn.PlacementDict(
    "IcePlatform" => Ahorn.EntityPlacement(IcePlatform),
    "permanent" => Ahorn.EntityPlacement(IcePlatform),
    "super_slippery" => Ahorn.EntityPlacement(IcePlatform)
)

function Ahorn.selection(entity::IcePlatform)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::IcePlatform, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.7, 0.9, 1.0, 0.4), (0.8, 1.0, 1.0, 0.7))
end

end

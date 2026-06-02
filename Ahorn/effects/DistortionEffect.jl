module DistortionEffect

using ..Ahorn, Maple

@mapdef Effect "MaggyHelper/DistortionEffect" DistortionEffect(x::Integer, y::Integer, anxiety::Number=0.0, gamerate::Number=1.0, waterAlpha::Number=1.0)

const placements = Ahorn.PlacementDict(
    "distortion" => Ahorn.EntityPlacement(DistortionEffect)
)

function Ahorn.selection(entity::DistortionEffect)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::DistortionEffect, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

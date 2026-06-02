module GravityFlipPlatform

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/GravityFlipPlatform" GravityFlipPlatform(x::Integer, y::Integer, cooldown::Number=2.0, height::Integer=8, togglable::Bool=true, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "normal" => Ahorn.EntityPlacement(GravityFlipPlatform)
)

function Ahorn.selection(entity::GravityFlipPlatform)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::GravityFlipPlatform, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.4, 0.2, 0.8, 0.4), (0.6, 0.3, 1.0, 0.8))
end

end

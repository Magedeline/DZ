module ShadowLantern

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/ShadowLantern" ShadowLantern(x::Integer, y::Integer, lightRadius::Number=64.0, persistent::Bool=false)

const placements = Ahorn.PlacementDict(
    "ShadowLantern" => Ahorn.EntityPlacement(ShadowLantern),
    "ShadowLanternlarge" => Ahorn.EntityPlacement(ShadowLantern)
)

function Ahorn.selection(entity::ShadowLantern)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ShadowLantern, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

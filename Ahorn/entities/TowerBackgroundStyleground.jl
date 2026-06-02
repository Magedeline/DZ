module TowerBackgroundStyleground

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TowerBackgroundStyleground" TowerBackgroundStyleground(x::Integer, y::Integer, alpha::Number=0.8, autoFindTower::Bool=true, backgroundStyle::String="Default", height::Integer=240, parallaxX::Number=1.0, parallaxY::Number=1.0, scrollX::Number=0.0, scrollY::Number=0.0, tileHeight::Integer=64, tileWidth::Integer=64, tintBlue::Integer=128, tintGreen::Integer=0, tintRed::Integer=128, width::Integer=320)

const placements = Ahorn.PlacementDict(
    "default_background" => Ahorn.EntityPlacement(TowerBackgroundStyleground),
    "mystical_background" => Ahorn.EntityPlacement(TowerBackgroundStyleground),
    "dark_background" => Ahorn.EntityPlacement(TowerBackgroundStyleground),
    "golden_background" => Ahorn.EntityPlacement(TowerBackgroundStyleground),
    "ethereal_background" => Ahorn.EntityPlacement(TowerBackgroundStyleground),
    "custom_background" => Ahorn.EntityPlacement(TowerBackgroundStyleground)
)

function Ahorn.selection(entity::TowerBackgroundStyleground)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TowerBackgroundStyleground, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

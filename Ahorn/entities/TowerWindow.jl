module TowerWindow

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TowerWindow" TowerWindow(x::Integer, y::Integer, lightIntensity::Number=0.6, startLit::Bool=true, view::String="Sky")

const placements = Ahorn.PlacementDict(
    "sky" => Ahorn.EntityPlacement(TowerWindow),
    "stars" => Ahorn.EntityPlacement(TowerWindow),
    "dark" => Ahorn.EntityPlacement(TowerWindow)
)

function Ahorn.selection(entity::TowerWindow)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TowerWindow, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

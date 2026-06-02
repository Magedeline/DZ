module TowerLantern

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/TowerLantern" TowerLantern(x::Integer, y::Integer, flickerIntensity::Number=0.1, lanternId::String="", lightColor::String="FFA500", lightRadius::Integer=80, startLit::Bool=false)

const placements = Ahorn.PlacementDict(
    "unlit" => Ahorn.EntityPlacement(TowerLantern),
    "lit" => Ahorn.EntityPlacement(TowerLantern)
)

function Ahorn.selection(entity::TowerLantern)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TowerLantern, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

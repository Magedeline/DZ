module KglobalPlayer

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/Kglobal::Player" KglobalPlayer(x::Integer, y::Integer, isDefaultSpawn::Bool=false)

const placements = Ahorn.PlacementDict(
    "MaggyHelper/Kglobal::Player" => Ahorn.EntityPlacement(KglobalPlayer)
)

function Ahorn.selection(entity::KglobalPlayer)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KglobalPlayer, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

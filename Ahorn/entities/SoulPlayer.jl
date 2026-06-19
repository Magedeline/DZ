module SoulPlayer

using ..Ahorn, Maple

@mapdef Entity "DZ/SoulPlayer" SoulPlayer(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "Soul Player" => Ahorn.EntityPlacement(SoulPlayer)
)

function Ahorn.selection(entity::SoulPlayer)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::SoulPlayer, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.6, 1.0, 1.0, 0.5), (0.0, 0.8, 1.0, 1.0))
end

end

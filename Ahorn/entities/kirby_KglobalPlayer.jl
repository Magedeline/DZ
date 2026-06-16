module kirby_KglobalPlayer

using ..Ahorn, Maple

@mapdef Entity "kirby_Kglobal::Player" kirby_KglobalPlayer(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "kirby_Kglobal::Player" => Ahorn.EntityPlacement(kirby_KglobalPlayer)
)

function Ahorn.selection(entity::kirby_KglobalPlayer)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::kirby_KglobalPlayer, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

# Nodes: min=0, max=inf
# Basic node rendering not implemented in auto-generated plugin

end

module KirbyKglobalPlayer

using ..Ahorn, Maple

@mapdef Entity "DZ/KirbyKglobal::Player" KirbyKglobalPlayer(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "kirby_Kglobal::Player" => Ahorn.EntityPlacement(KirbyKglobalPlayer)
)

function Ahorn.selection(entity::KirbyKglobalPlayer)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::KirbyKglobalPlayer, room::Maple.Room)
    Ahorn.drawSprite(ctx, "characters/Maggy/DesoloZantas/kirby/idle00", entity.x, entity.y)
end

# Nodes: min=0, max=inf
# Basic node rendering not implemented in auto-generated plugin

end

module OshiroLobbyBell

using ..Ahorn, Maple

@mapdef Entity "MaggyHelper/OshiroLobbyBell" OshiroLobbyBell(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
    "oshiroLobbyBell" => Ahorn.EntityPlacement(OshiroLobbyBell)
)

function Ahorn.selection(entity::OshiroLobbyBell)
    return Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::OshiroLobbyBell, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.Rectangle(entity.x - 8, entity.y - 8, 16, 16), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

module RuinsLobbyEnterTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/RuinsLobbyEnterTrigger" RuinsLobbyEnterTrigger(x::Integer, y::Integer, width::Integer=16)

const placements = Ahorn.PlacementDict(
    "Enter Ruins Lobby" => Ahorn.EntityPlacement(RuinsLobbyEnterTrigger)
)

function Ahorn.selection(entity::RuinsLobbyEnterTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::RuinsLobbyEnterTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

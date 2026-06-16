module ChapterLobbyEnterTrigger

using ..Ahorn, Maple

@mapdef Trigger "DZ/ChapterLobbyEnterTrigger" ChapterLobbyEnterTrigger(x::Integer, y::Integer, height::Integer=64, lobbyRoom::String="lvl_lobby_hub", lobbySid::String="Maggy/Lobby/11_Snowdin_Lobby", lockedDialogKey::String="SNOWDIN_LOBBY_LOCKED", requiredFlag::String="ch11_main_completed", width::Integer=16)

const placements = Ahorn.PlacementDict(
    "CH11 – Enter Snowdin City Lobby" => Ahorn.EntityPlacement(ChapterLobbyEnterTrigger),
    "CH12 – Enter Wateredgefalls Lobby" => Ahorn.EntityPlacement(ChapterLobbyEnterTrigger),
    "CH13 – Enter Hotcliffland Lobby" => Ahorn.EntityPlacement(ChapterLobbyEnterTrigger),
    "CH14 – Enter Cyber Nexus Lobby" => Ahorn.EntityPlacement(ChapterLobbyEnterTrigger)
)

function Ahorn.selection(entity::ChapterLobbyEnterTrigger)
    return Ahorn.getEntityRectangle(entity)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::ChapterLobbyEnterTrigger, room::Maple.Room)
    Ahorn.drawRectangle(ctx, Ahorn.getEntityRectangle(entity), (0.5, 0.5, 0.5, 0.5), (0.0, 0.0, 0.0, 1.0))
end

end

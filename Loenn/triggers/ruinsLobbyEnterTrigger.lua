-- RuinsLobbyEnterTrigger: place in 10_Ruins_A.bin near the chapter exit.
-- After completing the main A-side, this sends the player to the Ruins Lobby.
local ruinsLobbyEnterTrigger = {}

ruinsLobbyEnterTrigger.name = "DZ/RuinsLobbyEnterTrigger"
ruinsLobbyEnterTrigger.placements = {
    { name = "main", data = { width = 16, height = 16 } }
}

ruinsLobbyEnterTrigger.fieldOrder = { "x", "y", "width", "height" }
ruinsLobbyEnterTrigger.color = { 0.6, 0.3, 1.0 }  -- purple

return ruinsLobbyEnterTrigger

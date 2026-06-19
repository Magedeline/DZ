-- CharaChaserEnd Trigger - creates a barrier that stops the chaser from following the player
local charaChaserEnd = {}

charaChaserEnd.name = "DZ/CharaChaserEnd"
charaChaserEnd.placements = {
    {
        name = "main",
        data = {
            width = 16,
            height = 16
        }
    },
    {
        name = "wide",
        data = {
            width = 64,
            height = 16
        }
    },
    {
        name = "tall",
        data = {
            width = 16,
            height = 64
        }
    }
}

-- Visual border color to distinguish this trigger in the editor
charaChaserEnd.borderColor = {0.8, 0.2, 0.2, 0.8}
charaChaserEnd.fillColor = {0.8, 0.2, 0.2, 0.3}

return charaChaserEnd

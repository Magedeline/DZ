local floweyNPC = {}

floweyNPC.name = "DesoloZantas/FloweyNPC"
floweyNPC.depth = 100
floweyNPC.justification = {0.5, 1.0}

floweyNPC.placements = {
    {
        name = "Flowey (Hidden - Cutscene)",
        data = {
            dialogId = "CH10_FLOWEY_INTRO",
            startHidden = true,
            autoEmerge = false,
            emergeDelay = 0.5
        }
    },
    {
        name = "Flowey (Visible)",
        data = {
            dialogId = "CH10_FLOWEY_INTRO",
            startHidden = false,
            autoEmerge = false,
            emergeDelay = 0.5
        }
    },
    {
        name = "Flowey (Auto Emerge)",
        data = {
            dialogId = "CH10_FLOWEY_INTRO",
            startHidden = true,
            autoEmerge = true,
            emergeDelay = 0.5
        }
    }
}

floweyNPC.fieldInformation = {
    emergeDelay = {
        minimumValue = 0.0,
        maximumValue = 10.0
    }
}

function floweyNPC.texture(room, entity)
    return "characters/flowey/idle00"
end

return floweyNPC

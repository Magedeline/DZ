local eventTrigger = {}
local eventOptions = require("libraries.cutscene_event_ids")

eventTrigger.name = "MaggyHelper/EventTrigger"
eventTrigger.placements = {
    {
        name = "default",
        data = {
            x = 0,
            y = 0,
            width = 16,
            height = 16,
            event = ""
        }
    }
}

eventTrigger.fieldInformation = {
    event = {
        fieldType = "string",
        editable = true,
        options = eventOptions
    }
}

return eventTrigger

-- Intro Presents Trigger for Loenn
-- Shows the "Magedeline Presents Desolo Zantas" intro screen with disclaimer

local introPresentsTrigger = {}

introPresentsTrigger.name = "DZ/IntroPresentsTrigger"
introPresentsTrigger.placements = {
    {
        name = "IntroPresentsTrigger",
        data = {
            width = 32,
            height = 32
        }
    }
}

introPresentsTrigger.fieldInformation = {}

function introPresentsTrigger.sprite(room, entity)
    local width = entity.width or 32
    local height = entity.height or 32
    
    -- Gold/Yellow color to indicate special intro trigger
    local rectangles = {
        {
            x = 0,
            y = 0,
            width = width,
            height = height,
            color = {1.0, 0.84, 0.0, 0.5}  -- Gold
        }
    }
    
    return rectangles
end

function introPresentsTrigger.selection(room, entity)
    return {
        x = entity.x,
        y = entity.y,
        width = entity.width or 32,
        height = entity.height or 32
    }
end

return introPresentsTrigger

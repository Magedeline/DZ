local beyondSummitGem = {}
beyondSummitGem.name = "MaggyHelper/BeyondSummitGem"
beyondSummitGem.depth = -10010

local gemColors = {
    {0x9B / 255, 0x3F / 255, 0xB5 / 255},  -- 0 purple
    {0x32 / 255, 0x32 / 255, 0xFF / 255},  -- 1 blue
    {0xE0 / 255, 0x19 / 255, 0x19 / 255},  -- 2 red
    {0x1B / 255, 0xBE / 255, 0x1B / 255},  -- 3 green
    {0xFF / 255, 0xD7 / 255, 0x00 / 255},  -- 4 gold
    {0xFF / 255, 0x6A / 255, 0x00 / 255},  -- 5 orange
    {0xFF / 255, 0x14 / 255, 0x93 / 255},  -- 6 pink
}

beyondSummitGem.fieldInformation = {
    gem = {
        fieldType = "integer",
        options = {
            ["Purple (0)"] = 0,
            ["Blue (1)"] = 1,
            ["Red (2)"] = 2,
            ["Green (3)"] = 3,
            ["Gold (4)"] = 4,
            ["Orange (5)"] = 5,
            ["Pink (6)"] = 6,
        }
    }
}

beyondSummitGem.placements = {}
for i = 0, 6 do
    local names = {"Purple", "Blue", "Red", "Green", "Gold", "Orange", "Pink"}
    beyondSummitGem.placements[i + 1] = {
        name = "BeyondSummitGem (" .. names[i + 1] .. ")",
        data = {
            gem = i,
            sprite = "",
        }
    }
end

function beyondSummitGem.texture(room, entity)
    local gem = entity.gem or 0
    return "collectables/summitgems/" .. gem .. "/gem00"
end

function beyondSummitGem.selection(room, entity)
    return Ahorn and Ahorn.Rectangle or require("structs.rectangle")(entity.x - 8, entity.y - 8, 16, 16)
end

return beyondSummitGem

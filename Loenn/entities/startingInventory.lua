local startingInventory = {}

startingInventory.name = "DZ/StartingInventory"
startingInventory.depth = 0

startingInventory.placements = {
    {
        name = "main",
        data = {
            inventoryType = "Default",
            debugVisible = false
        }
    },
    {
        name = "heart_power",
        data = {
            inventoryType = "Heart",
            debugVisible = false
        }
    },
    {
        name = "kirby_Kglobal::Player",
        data = {
            inventoryType = "KirbyKglobal::Player",
            debugVisible = false
        }
    },
    {
        name = "say_goodbye",
        data = {
            inventoryType = "SayGoodbye",
            debugVisible = false
        }
    },
    {
        name = "titan_tower_climbing",
        data = {
            inventoryType = "TitanTowerClimbing",
            debugVisible = false
        }
    },
    {
        name = "corruption",
        data = {
            inventoryType = "Corruption",
            debugVisible = false
        }
    },
    {
        name = "the_end",
        data = {
            inventoryType = "TheEnd",
            debugVisible = false
        }
    }
}

startingInventory.fieldInformation = {
    inventoryType = {
        options = {
            "Default",
            "Heart",
            "KirbyKglobal::Player", 
            "SayGoodbye",
            "TitanTowerClimbing",
            "Corruption",
            "TheEnd"
        },
        editable = false
    }
}

function startingInventory.sprite(room, entity)
    local inventoryType = entity.inventoryType or "Default"
    
    -- Use different sprites based on inventory type
    if inventoryType == "Heart" then
        return "ahorn/starting_inventory_heart"
    elseif inventoryType == "KirbyKglobal::Player" then
        return "ahorn/starting_inventory_kirby"
    elseif inventoryType == "SayGoodbye" then
        return "ahorn/starting_inventory_goodbye"
    elseif inventoryType == "TitanTowerClimbing" then
        return "ahorn/starting_inventory_titan"
    elseif inventoryType == "Corruption" then
        return "ahorn/starting_inventory_corruption"
    elseif inventoryType == "TheEnd" then
        return "ahorn/starting_inventory_end"
    else
        return "ahorn/starting_inventory_default"
    end
end

function startingInventory.color(room, entity)
    local inventoryType = entity.inventoryType or "Default"
    
    if inventoryType == "Heart" then
        return {1.0, 0.2, 0.2, 0.9} -- Red
    elseif inventoryType == "KirbyKglobal::Player" then
        return {1.0, 1.0, 0.2, 0.9} -- Yellow
    elseif inventoryType == "SayGoodbye" then
        return {1.0, 0.4, 0.8, 0.9} -- Pink
    elseif inventoryType == "TitanTowerClimbing" then
        return {0.2, 0.4, 1.0, 0.9} -- Blue
    elseif inventoryType == "Corruption" then
        return {0.6, 0.2, 0.8, 0.9} -- Purple
    elseif inventoryType == "TheEnd" then
        return {1.0, 0.8, 0.2, 0.9} -- Gold
    else
        return {0.8, 0.8, 0.8, 0.9} -- White for Default
    end
end

function startingInventory.texture(room, entity)
    local inventoryType = entity.inventoryType or "Default"
    
    -- Fallback to simple colored rectangles if sprites aren't available
    if inventoryType == "Heart" then
        return "@Internal@/starting_inventory_heart"
    elseif inventoryType == "KirbyKglobal::Player" then
        return "@Internal@/starting_inventory_kirby"
    elseif inventoryType == "SayGoodbye" then
        return "@Internal@/starting_inventory_goodbye"
    elseif inventoryType == "TitanTowerClimbing" then
        return "@Internal@/starting_inventory_titan"
    elseif inventoryType == "Corruption" then
        return "@Internal@/starting_inventory_corruption"
    elseif inventoryType == "TheEnd" then
        return "@Internal@/starting_inventory_end"
    else
        return "@Internal@/starting_inventory_default"
    end
end

function startingInventory.rectangle(room, entity)
    local x = entity.x or 0
    local y = entity.y or 0
    return x - 8, y - 8, 16, 16
end

-- Optional: Add a nodeline function if you want to connect starting inventory to spawn points
function startingInventory.nodeLineRenderType(room, entity)
    return "line"
end

function startingInventory.nodeVisibility(room, entity)
    return "never"
end

return startingInventory
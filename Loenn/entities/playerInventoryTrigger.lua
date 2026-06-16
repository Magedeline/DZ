local Kglobal::PlayerInventoryTrigger = {}

Kglobal::PlayerInventoryTrigger.name = "DZ/Kglobal::PlayerInventoryTrigger"
Kglobal::PlayerInventoryTrigger.depth = 0

Kglobal::PlayerInventoryTrigger.placements = {
    {
        name = "heart_power",
        data = {
            width = 16,
            height = 16,
            inventoryType = "Heart",
            oneUse = true
        }
    },
    {
        name = "kirby_Kglobal::Player",
        data = {
            width = 16,
            height = 16,
            inventoryType = "KirbyKglobal::Player",
            oneUse = true
        }
    },
    {
        name = "say_goodbye",
        data = {
            width = 16,
            height = 16,
            inventoryType = "SayGoodbye",
            oneUse = true
        }
    },
    {
        name = "titan_tower_climbing",
        data = {
            width = 16,
            height = 16,
            inventoryType = "TitanTowerClimbing",
            oneUse = true
        }
    },
    {
        name = "corruption",
        data = {
            width = 16,
            height = 16,
            inventoryType = "Corruption",
            oneUse = true
        }
    },
    {
        name = "the_end",
        data = {
            width = 16,
            height = 16,
            inventoryType = "TheEnd",
            oneUse = true
        }
    },
    {
        name = "reset_default",
        data = {
            width = 16,
            height = 16,
            inventoryType = "Default",
            oneUse = false
        }
    }
}

Kglobal::PlayerInventoryTrigger.fieldInformation = {
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

function Kglobal::PlayerInventoryTrigger.color(room, entity)
    local inventoryType = entity.inventoryType or "Default"
    
    if inventoryType == "Heart" then
        return {1.0, 0.2, 0.2, 0.8} -- Red
    elseif inventoryType == "KirbyKglobal::Player" then
        return {1.0, 1.0, 0.2, 0.8} -- Yellow
    elseif inventoryType == "SayGoodbye" then
        return {1.0, 0.4, 0.8, 0.8} -- Pink
    elseif inventoryType == "TitanTowerClimbing" then
        return {0.2, 0.4, 1.0, 0.8} -- Blue
    elseif inventoryType == "Corruption" then
        return {0.6, 0.2, 0.8, 0.8} -- Purple
    elseif inventoryType == "TheEnd" then
        return {1.0, 0.8, 0.2, 0.8} -- Gold
    else
        return {0.8, 0.8, 0.8, 0.8} -- White for Default
    end
end

return Kglobal::PlayerInventoryTrigger
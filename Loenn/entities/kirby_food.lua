local kirbyFood = {}

kirbyFood.name = "DZ/KirbyFood"
kirbyFood.depth = -50

kirbyFood.fieldInformation = {
    foodType = {
        options = {
            -- Basic Foods
            ["Apple"] = 0,
            ["Orange"] = 1,
            ["Cherry"] = 2,
            ["Banana"] = 3,
            ["Watermelon"] = 4,
            ["Pineapple"] = 5,
            ["Grapes"] = 6,
            ["Strawberry"] = 7,
            -- Medium Foods
            ["Meat"] = 8,
            ["Fish"] = 9,
            ["Ice Cream"] = 10,
            ["Cake"] = 11,
            ["Hamburger"] = 12,
            ["Hot Dog"] = 13,
            ["Pizza"] = 14,
            ["Sandwich"] = 15,
            ["Cherry Bunch"] = 16,
            -- Large Foods
            ["Max Tomato (Full Heal)"] = 17,
            -- Special Items
            ["Invincibility Star"] = 18,
            ["1-Up"] = 19,
            ["Point Star"] = 20
        },
        editable = false
    }
}

kirbyFood.placements = {
    {
        name = "Apple",
        data = {
            foodType = 0,
            healAmount = 0,
            isDamaging = false,
            despawnTime = 10,
            isFalling = false
        }
    },
    {
        name = "Max Tomato (Full Heal)",
        data = {
            foodType = 17,
            healAmount = 0,
            isDamaging = false,
            despawnTime = 15,
            isFalling = false
        }
    },
    {
        name = "Meat",
        data = {
            foodType = 8,
            healAmount = 0,
            isDamaging = false,
            despawnTime = 10,
            isFalling = false
        }
    },
    {
        name = "Cake",
        data = {
            foodType = 11,
            healAmount = 0,
            isDamaging = false,
            despawnTime = 10,
            isFalling = false
        }
    },
    {
        name = "Cherry Bunch",
        data = {
            foodType = 16,
            healAmount = 0,
            isDamaging = false,
            despawnTime = 10,
            isFalling = false
        }
    },
    {
        name = "Invincibility Star",
        data = {
            foodType = 18,
            healAmount = 0,
            isDamaging = false,
            despawnTime = 30,
            isFalling = false
        }
    },
    {
        name = "1-Up",
        data = {
            foodType = 19,
            healAmount = 0,
            isDamaging = false,
            despawnTime = 30,
            isFalling = false
        }
    },
    {
        name = "Custom Food",
        data = {
            foodType = 0,
            healAmount = 5,
            isDamaging = false,
            despawnTime = 10,
            isFalling = false
        }
    },
    {
        name = "Damaging Apple (Boss Drop)",
        data = {
            foodType = 0,
            healAmount = 0,
            isDamaging = true,
            despawnTime = 5,
            isFalling = true
        }
    }
}

-- Texture based on food type
function kirbyFood.texture(room, entity)
    local foodType = entity.foodType or 0
    local textures = {
        [0] = "items/kirby/food/apple/idle00",
        [1] = "items/kirby/food/orange/idle00",
        [2] = "items/kirby/food/cherry/idle00",
        [3] = "items/kirby/food/banana/idle00",
        [4] = "items/kirby/food/watermelon/idle00",
        [5] = "items/kirby/food/pineapple/idle00",
        [6] = "items/kirby/food/grapes/idle00",
        [7] = "items/kirby/food/strawberry/idle00",
        [8] = "items/kirby/food/meat/idle00",
        [9] = "items/kirby/food/fish/idle00",
        [10] = "items/kirby/food/icecream/idle00",
        [11] = "items/kirby/food/cake/idle00",
        [12] = "items/kirby/food/hamburger/idle00",
        [13] = "items/kirby/food/hotdog/idle00",
        [14] = "items/kirby/food/pizza/idle00",
        [15] = "items/kirby/food/sandwich/idle00",
        [16] = "items/kirby/food/cherrybunch/idle00",
        [17] = "items/kirby/food/maxtomato/idle00",
        [18] = "items/kirby/star/idle00",
        [19] = "items/kirby/oneup/idle00",
        [20] = "items/kirby/pointstar/idle00"
    }
    return textures[foodType] or "collectables/strawberry/normal00"
end

-- Color based on food properties
function kirbyFood.color(room, entity)
    if entity.isDamaging then
        return {1.0, 0.3, 0.3, 1.0} -- Red tint for damaging
    end
    return {1.0, 1.0, 1.0, 1.0} -- Normal white
end

return kirbyFood

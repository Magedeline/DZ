local miniHeart = {}

miniHeart.name = "DZ/MiniHeart"
miniHeart.depth = -100
miniHeart.texture = "DZ/miniheart/beginner/00"

miniHeart.placements = {
    name = "mini_heart",
    data = {
        variant = "beginner",
        flash = true,
        endLevel = true,
        recordMiniHeart = true,
        registerHeartGem = false,
        breakOnTouch = true,
        chapter = 0
    }
}

miniHeart.fieldInformation = {
    variant = {
        options = {
            "beginner",
            "intermediate",
            "advanced",
            "expert",
            "grandmaster",
            "ghost",
            "white"
        },
        editable = false
    },
    chapter = {
        fieldType = "integer"
    }
}

return miniHeart

local phantomKnight = {}
phantomKnight.name = "MaggyHelper/PhantomKnight"
phantomKnight.depth = -100
phantomKnight.placements = {
    { name = "PhantomKnight", data = { health = 3, hiddenTime = 2.0, attackTime = 0.5, slashRange = 30.0 } },
    { name = "PhantomKnightAggressive", data = { health = 2, hiddenTime = 1.0, attackTime = 0.3, slashRange = 40.0 } }
}
phantomKnight.fieldInformation = {
    health = { fieldType = "integer", minimumValue = 1 },
    hiddenTime = { fieldType = "number", minimumValue = 0.5 },
    attackTime = { fieldType = "number", minimumValue = 0.1 },
    slashRange = { fieldType = "number", minimumValue = 10.0 }
}
phantomKnight.fieldOrder = { "x", "y", "health", "hiddenTime", "attackTime", "slashRange" }
return phantomKnight

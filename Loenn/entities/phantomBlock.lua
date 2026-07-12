local phantomBlock = {}

phantomBlock.name = "DZ/PhantomBlock"
phantomBlock.depth = 0
phantomBlock.texture = "objects/dreamblock/particles"

phantomBlock.placements = {
    name = "phantom_block",
    data = {
        width = 16,
        height = 16,
        below = false
    }
}

phantomBlock.fieldInformation = {
    below = {
        fieldType = "boolean"
    }
}

return phantomBlock

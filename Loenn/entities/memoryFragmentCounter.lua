local memoryFragmentCounter = {}

memoryFragmentCounter.name = "DZ/MemoryFragmentCounter"
memoryFragmentCounter.depth = 0
memoryFragmentCounter.texture = "objects/DZ/memory_fragment_counter"

memoryFragmentCounter.placements = {
    name = "memory_fragment_counter",
    data = {
        fragmentId = "",
        dialogueKey = "MEMORY_FRAGMENT",
        showDialogue = true,
        fragmentNumber = 1
    }
}

return memoryFragmentCounter
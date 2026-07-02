local sequencePuzzleManager = {}

sequencePuzzleManager.name = "DZ/SequencePuzzleManager"
sequencePuzzleManager.depth = 0
sequencePuzzleManager.texture = "objects/DZ/DZ/DZ/sequence_puzzle_manager"

sequencePuzzleManager.placements = {
    name = "sequence_puzzle_manager",
    data = {
        gateId = "",
        sequenceOrder = 0,
        holdTime = 1,
        timerDuration = 3
    }
}

return sequencePuzzleManager
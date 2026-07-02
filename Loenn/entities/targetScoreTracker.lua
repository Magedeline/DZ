local targetScoreTracker = {}

targetScoreTracker.name = "DZ/TargetScoreTracker"
targetScoreTracker.depth = 0
targetScoreTracker.texture = "objects/DZ/DZ/DZ/target_score_tracker"

targetScoreTracker.placements = {
    name = "target_score_tracker",
    data = {
        points = 100,
        showTime = 2,
        resetTime = 3,
        moveSpeed = 50
    }
}

return targetScoreTracker
local towerObstacleFactory = {}

towerObstacleFactory.name = "DZ/TowerObstacleFactory"
towerObstacleFactory.depth = 0
towerObstacleFactory.texture = "objects/DZ/tower_obstacle_factory"

towerObstacleFactory.placements = {
    name = "tower_obstacle_factory",
    data = {
        obstacleSetType = "Intermediate",
        obstaclePattern = "Spiral",
        backgroundStyle = "Default",
        createBackground = true,
        createObstacles = true,
        autoPositionAroundTower = true,
        obstacleCount = 15,
        towerRadius = 120,
        verticalSpacing = 150,
        patternRotation = 0,
        activationDelay = 0
    }
}

return towerObstacleFactory
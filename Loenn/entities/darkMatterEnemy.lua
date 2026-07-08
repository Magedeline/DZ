local darkMatterEnemy = {}

darkMatterEnemy.name = "DZ/DarkMatterEnemy"
darkMatterEnemy.depth = 0
darkMatterEnemy.texture = "objects/DZ/DZ/DZ/dark_matter_enemy"

darkMatterEnemy.placements = {
    name = "dark_matter_enemy",
    data = {
        health = 10,
        minDamage = 2,
        maxDamage = 7,
        patrolRadius = 64
    }
}

return darkMatterEnemy
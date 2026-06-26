local saloonChandelier = {}

saloonChandelier.name = "DZ/SaloonChandelier"
saloonChandelier.depth = 0
saloonChandelier.texture = "objects/DZ/saloon_chandelier"

saloonChandelier.placements = {
    name = "saloon_chandelier",
    data = {
        canFall = true,
        isHazard = true,
        swingPeriod = 3,
        swingAngle = 0.4,
        chainLength = 80
    }
}

return saloonChandelier
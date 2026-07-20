local powerGenerator = {}

powerGenerator.name = "DZ/PowerGenerator"
powerGenerator.depth = 0
powerGenerator.texture = "objects/DZ/power_generator/idle00"

powerGenerator.placements = {
    name = "power_generator",
    data = {
        flipX = false,
        canTeleport = false
    }
}

function powerGenerator.scale(room, entity)
    local scaleX = entity.flipX and -1 or 1

    return scaleX, 1
end

function powerGenerator.justification(room, entity)
    local flipX = entity.flipX

    return flipX and 0.75 or 0.25, 0.25
end

return powerGenerator
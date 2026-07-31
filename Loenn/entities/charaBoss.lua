local charaBoss = {}

charaBoss.name = "DZ/CharaBoss"
charaBoss.depth = 0
charaBoss.nodeLineRenderType = "line"
charaBoss.nodeLimits = {0, -1}
charaBoss.texture = "characters/DZ/chara/idle00"

charaBoss.placements = {
    name = "chara_boss",
    data = {
        patternIndex = 0,
        cameraPastY = 120.0,
        dialog = false,
        startHit = false,
        cameraLockY = true,
        canChangeMusic = true,
        attackSequence = "",
    }
}

charaBoss.fieldInformation = {
    patternIndex = {
        fieldType = "integer",
    },
    cameraPastY = {
        fieldType = "number",
    },
}

return charaBoss

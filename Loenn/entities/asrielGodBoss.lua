local asrielGodBoss = {}

asrielGodBoss.name = "DZ/AsrielGodBoss"
asrielGodBoss.depth = 0
asrielGodBoss.nodeLineRenderType = "line"
asrielGodBoss.nodeLimits = {0, -1}
asrielGodBoss.texture = "characters/asrielgodboss/idle00"

asrielGodBoss.placements = {
    name = "asriel_god_boss",
    data = {
        patternIndex = 0,
        cameraPastY = 120.0,
        dialog = false,
        startHit = false,
        cameraLockY = true,
        attackSequence = "",
    }
}

asrielGodBoss.fieldInformation = {
    patternIndex = {
        fieldType = "integer",
    },
    cameraPastY = {
        fieldType = "number",
    },
}

return asrielGodBoss

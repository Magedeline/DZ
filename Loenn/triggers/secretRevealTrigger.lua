local secretRevealTrigger = {}
secretRevealTrigger.name = "DZ/SecretRevealTrigger"
secretRevealTrigger.placements = {
    { name = "SecretRevealTrigger", data = { width = 16, height = 16, flag = "secret_found", revealSound = "", cameraTarget = "" } }
}
secretRevealTrigger.fieldInformation = {
    flag = { fieldType = "string" },
    revealSound = { fieldType = "string" },
    cameraTarget = { fieldType = "string" }
}
secretRevealTrigger.fieldOrder = { "x", "y", "width", "height", "flag", "revealSound", "cameraTarget" }
return secretRevealTrigger

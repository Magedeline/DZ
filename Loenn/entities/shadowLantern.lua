local shadowLantern = {}
shadowLantern.name = "MaggyHelper/ShadowLantern"
shadowLantern.depth = -100
shadowLantern.placements = {
    { name = "ShadowLantern", data = { lightRadius = 64.0, persistent = false, flag = "" } },
    { name = "ShadowLanternlarge", data = { lightRadius = 120.0, persistent = false, flag = "" } }
}
shadowLantern.fieldInformation = {
    lightRadius = { fieldType = "number", minimumValue = 10.0 },
    persistent = { fieldType = "boolean" },
    flag = { fieldType = "string" }
}
shadowLantern.fieldOrder = { "x", "y", "lightRadius", "persistent", "flag" }
return shadowLantern

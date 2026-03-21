local oneHitTrigger = {}
oneHitTrigger.name = "MaggyHelper/OneHitTrigger"
oneHitTrigger.placements = {
    { name = "OneHitTrigger", data = { width = 32, height = 32, flag = "one_hit_mode" } }
}
oneHitTrigger.fieldInformation = {
    flag = { fieldType = "string" }
}
oneHitTrigger.fieldOrder = { "x", "y", "width", "height", "flag" }
return oneHitTrigger

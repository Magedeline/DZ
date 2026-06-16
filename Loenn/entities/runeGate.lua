local runeGate = {}
runeGate.name = "DZ/RuneGate"
runeGate.depth = 0
runeGate.placements = {
    { name = "RuneGate", data = { width = 16, height = 24, gateId = "gate_1", requiredRunes = 3 } }
}
runeGate.fieldInformation = {
    gateId = { fieldType = "string" },
    requiredRunes = { fieldType = "integer", minimumValue = 1 }
}
runeGate.fieldOrder = { "x", "y", "width", "height", "gateId", "requiredRunes" }
return runeGate

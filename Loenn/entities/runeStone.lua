local runeStone = {}
runeStone.name = "DZ/RuneStone"
runeStone.depth = -100
runeStone.placements = {
    { name = "main", data = { runeId = "rune_1", gateId = "gate_1" } }
}
runeStone.fieldInformation = {
    runeId = { fieldType = "string" },
    gateId = { fieldType = "string" }
}
runeStone.fieldOrder = { "x", "y", "runeId", "gateId" }
return runeStone

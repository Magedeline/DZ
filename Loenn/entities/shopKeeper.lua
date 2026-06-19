local shopKeeper = {}
shopKeeper.name = "DZ/ShopKeeper"
shopKeeper.depth = -100
shopKeeper.justification = {0.5, 1.0}
shopKeeper.texture = "characters/magolor/idle00"
shopKeeper.placements = {
    { name = "main", data = { shopId = "shop_1", dialogId = "" } }
}
shopKeeper.fieldInformation = {
    shopId = { fieldType = "string" },
    dialogId = { fieldType = "string" }
}
shopKeeper.fieldOrder = { "x", "y", "shopId", "dialogId" }
return shopKeeper

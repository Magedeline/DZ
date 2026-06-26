local shopKeeper = {}

shopKeeper.name = "DZ/ShopKeeper"
shopKeeper.depth = 0
shopKeeper.texture = "objects/DZ/shop_keeper"

shopKeeper.placements = {
    name = "shop_keeper",
    data = {
        companionType = "waddle_dee",
        sprite = "",
        canPressSwitch = true,
        followSpeed = 100,
        followDistance = 30
    }
}

return shopKeeper
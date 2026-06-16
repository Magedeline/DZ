local heart = {}

heart.name = "DZ/HeartGem"
heart.depth = -2000000
heart.texture = "collectables/heartGem/0/00"
heart.placements = {
    name = "real_crystal_heart",
    data = {
        fake = false,
        removeCameraTriggers = false,
        fakeHeartDialog = "DZ_CH19_WRONG_HEART",
        keepGoingDialog = "DZ_CH19_KEEP_GOING_KIRBY"
    }
}

return heart
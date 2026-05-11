local kirbyNPC = {}

kirbyNPC.name = "MaggyHelper/KirbyNPC"
kirbyNPC.depth = 100
kirbyNPC.nodeLimits = {0, -1}
kirbyNPC.nodeLineRenderType = "line"

kirbyNPC.fieldInformation = {
    character = {
        options = {
            ["Bandana Waddle Dee"] = 0,
            ["King Dedede"] = 1,
            ["Meta Knight"] = 2,
            ["Magolor"] = 3,
            ["Taranza"] = 4,
            ["Susie"] = 5,
            ["Elfilin"] = 6,
            ["Adeleine"] = 7,
            ["Ribbon"] = 8,
            ["Gooey"] = 9,
            ["Marx"] = 10,
            ["Dark Meta Knight"] = 11,
            ["Custom"] = 12
        },
        editable = false
    },
    behavior = {
        options = {
            ["Stationary"] = 0,
            ["Wander"] = 1,
            ["Follow"] = 2,
            ["Patrol (uses nodes)"] = 3,
            ["Shop"] = 4
        },
        editable = false
    }
}

kirbyNPC.placements = {
    {
        name = "Bandana Waddle Dee",
        data = {
            character = 0,
            behavior = 0,
            dialogId = "",
            canGiveItem = false,
            giveItemId = "",
            followDistance = 48,
            moveSpeed = 40
        }
    },
    {
        name = "King Dedede (Friendly)",
        data = {
            character = 1,
            behavior = 0,
            dialogId = "",
            canGiveItem = false,
            giveItemId = "",
            followDistance = 48,
            moveSpeed = 30
        }
    },
    {
        name = "Meta Knight (Friendly)",
        data = {
            character = 2,
            behavior = 0,
            dialogId = "",
            canGiveItem = false,
            giveItemId = "",
            followDistance = 48,
            moveSpeed = 50
        }
    },
    {
        name = "Magolor",
        data = {
            character = 3,
            behavior = 0,
            dialogId = "",
            canGiveItem = false,
            giveItemId = "",
            followDistance = 48,
            moveSpeed = 45
        }
    },
    {
        name = "Shop Keeper",
        data = {
            character = 0,
            behavior = 4,
            dialogId = "kirby_shop_welcome",
            canGiveItem = false,
            giveItemId = "",
            followDistance = 48,
            moveSpeed = 0
        }
    },
    {
        name = "Companion (Follows Kglobal::Player)",
        data = {
            character = 0,
            behavior = 2,
            dialogId = "",
            canGiveItem = false,
            giveItemId = "",
            followDistance = 48,
            moveSpeed = 50
        }
    },
    {
        name = "Patrol NPC (With Nodes)",
        data = {
            character = 0,
            behavior = 3,
            dialogId = "",
            canGiveItem = false,
            giveItemId = "",
            followDistance = 48,
            moveSpeed = 40
        }
    }
}

-- Texture based on character
function kirbyNPC.texture(room, entity)
    local character = entity.character or 0
    local textures = {
        [0] = "characters/kirby_npc/bandana_dee/idle00",
        [1] = "characters/kirby_npc/king_dedede/idle00",
        [2] = "characters/kirby_npc/meta_knight/idle00",
        [3] = "characters/kirby_npc/Maggy/DesoloZantas/magolor/idle00",
        [4] = "characters/Maggy/DesoloZantas/kirby/idle00",
        [5] = "characters/Maggy/DesoloZantas/kirby/idle00",
        [6] = "characters/Maggy/DesoloZantas/kirby/idle00",
        [7] = "characters/Maggy/DesoloZantas/kirby/idle00",
        [8] = "characters/Maggy/DesoloZantas/kirby/idle00",
        [9] = "characters/kirby_npc/gooey/idle00",
        [10] = "characters/Maggy/DesoloZantas/kirby/idle00",
        [11] = "characters/Maggy/DesoloZantas/kirby/idle00",
        [12] = "characters/Maggy/DesoloZantas/Kglobal::Player/sitDown00"
    }
    return textures[character] or "characters/Maggy/DesoloZantas/Kglobal::Player/sitDown00"
end

return kirbyNPC



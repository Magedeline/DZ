-- Audio Library for KIRBY_CELESTE
-- Maps FMOD event paths to their GUIDs for Loenn editor integration
-- Generated from GUIDs.txt mappings

local audioLibrary = {}

-- Character SFX - Badeline
audioLibrary.badeline = {
    appear = { path = "event:/char/badeline/appear", guid = "{ad25a031-880b-4f88-ac12-82d6c52fbdea}" },
    booster_begin = { path = "event:/char/badeline/booster_begin", guid = "{7a71fc61-b688-4d82-a2b7-781c2434e942}" },
    booster_final = { path = "event:/char/badeline/booster_final", guid = "{b2ee45d8-e1b0-43f1-baaa-f6e77d37ddb5}" },
    booster_reappear = { path = "event:/char/badeline/booster_reappear", guid = "{207a212a-2beb-4022-a8da-11ac987d3097}" },
    booster_relocate = { path = "event:/char/badeline/booster_relocate", guid = "{7e3c6b4e-e41a-4e9a-9d24-22737c66593b}" },
    booster_throw = { path = "event:/char/badeline/booster_throw", guid = "{2592d638-f3c5-4f6f-81e0-888a04affa40}" },
    boss_bullet = { path = "event:/char/badeline/boss_bullet", guid = "{bbe7d0c8-45f9-4671-b49e-8e38e357bb81}" },
    boss_hug = { path = "event:/char/badeline/boss_hug", guid = "{898fc1ea-250b-4d03-8f37-c0c35799a915}" },
    boss_idle_air = { path = "event:/char/badeline/boss_idle_air", guid = "{b4002010-ff11-4311-87c7-f87028220f78}" },
    boss_laser_charge = { path = "event:/char/badeline/boss_laser_charge", guid = "{e7a2ddd6-091a-44ab-aac7-e57bd13c009c}" },
    boss_laser_fire = { path = "event:/char/badeline/boss_laser_fire", guid = "{58b5d825-ebcf-457b-b493-9be82640b9eb}" },
    boss_prefight_getup = { path = "event:/char/badeline/boss_prefight_getup", guid = "{646b0a01-2101-424f-804b-18842f72a62d}" },
    climb_ledge = { path = "event:/char/badeline/climb_ledge", guid = "{33df1e8a-7605-4b2d-8235-ee78fbaa8c55}" },
    dash_red_left = { path = "event:/char/badeline/dash_red_left", guid = "{b54ae334-409b-4bba-82e2-62753f764f90}" },
    dash_red_right = { path = "event:/char/badeline/dash_red_right", guid = "{50356124-434a-4dca-aeb1-52462846fb8a}" },
    disappear = { path = "event:/char/badeline/disappear", guid = "{16b40879-0a79-4e42-8c91-fe419a8e186c}" },
    dreamblock_enter = { path = "event:/char/badeline/dreamblock_enter", guid = "{937c3941-eeb8-4f1f-8451-765b33545202}" },
    dreamblock_exit = { path = "event:/char/badeline/dreamblock_exit", guid = "{3b142d62-975a-41a3-9b59-8e5ba4f6cbdb}" },
    dreamblock_travel = { path = "event:/char/badeline/dreamblock_travel", guid = "{697102c2-9978-42b7-a3b0-0c2b99c9032e}" },
    duck = { path = "event:/char/badeline/duck", guid = "{d879f9dd-98d0-479e-9f08-a1848f4a0f5c}" },
    footstep = { path = "event:/char/badeline/footstep", guid = "{ff6442f1-66c2-4dba-8d57-038dda8fd296}" },
    grab = { path = "event:/char/badeline/grab", guid = "{8824489f-1d0b-4670-beb2-08d67cfd3c3b}" },
    grab_letgo = { path = "event:/char/badeline/grab_letgo", guid = "{c122d904-fb7a-4bfe-99a1-f5a4f8701d17}" },
    handhold = { path = "event:/char/badeline/handhold", guid = "{c2cd2f17-045c-4312-b7f6-8e7b750577c0}" },
    jump = { path = "event:/char/badeline/jump", guid = "{95ce0255-9a27-47b4-a902-fb25c0f2a2e2}" },
    jump_assisted = { path = "event:/char/badeline/jump_assisted", guid = "{64424649-d82c-4efc-90a9-f54e8411d9b7}" },
    jump_climb_left = { path = "event:/char/badeline/jump_climb_left", guid = "{58e689a2-1717-434f-9fa7-dffcd72ab27e}" },
    jump_climb_right = { path = "event:/char/badeline/jump_climb_right", guid = "{d9a77828-0390-434a-8cbc-e3df56a40cc4}" },
    jump_dreamblock = { path = "event:/char/badeline/jump_dreamblock", guid = "{6faecb61-f82e-400e-9785-4d407a82c838}" },
    jump_special = { path = "event:/char/badeline/jump_special", guid = "{bc8a6051-399a-4f76-a865-02b53c5fe8ab}" },
    jump_super = { path = "event:/char/badeline/jump_super", guid = "{73a21bdf-62c2-46d8-ae04-68e6c0c26b7f}" },
    jump_superslide = { path = "event:/char/badeline/jump_superslide", guid = "{7ca46073-e6b8-4f74-b9a8-ac54a8966c5b}" },
    jump_superwall = { path = "event:/char/badeline/jump_superwall", guid = "{2336b17f-5769-4e2e-a636-aee1ddef9086}" },
    jump_wall_left = { path = "event:/char/badeline/jump_wall_left", guid = "{ab3be2a7-36d0-43cb-990a-2c49ba5d5c60}" },
    jump_wall_right = { path = "event:/char/badeline/jump_wall_right", guid = "{3e5b18db-8f99-4ab7-ad08-8529fd257b6b}" },
    landing = { path = "event:/char/badeline/landing", guid = "{8f924592-8b14-40d6-81af-d42fab0b6da1}" },
    level_entry = { path = "event:/char/badeline/level_entry", guid = "{25294859-6bfd-434e-9528-8433245fe3b5}" },
    maddy_join = { path = "event:/char/badeline/maddy_join", guid = "{38557af4-adf9-4328-9f2c-167b12ff9f8e}" },
    maddy_split = { path = "event:/char/badeline/maddy_split", guid = "{450fb5b3-e9e3-45d8-9f34-ba05e292958f}" },
    stand = { path = "event:/char/badeline/stand", guid = "{1a114663-4b93-4aab-ba8c-ca8793f2831e}" },
    temple_move_chats = { path = "event:/char/badeline/temple_move_chats", guid = "{97d0d2e4-92f5-48cf-948f-4fc85b0a0791}" },
    temple_move_first = { path = "event:/char/badeline/temple_move_first", guid = "{a5ceb7fd-8a03-4c79-8d4f-81ad37400f43}" },
    wallslide = { path = "event:/char/badeline/wallslide", guid = "{a2443155-5af1-4e19-80d5-a81a3d9cf06d}" },
}

-- Character SFX - Madeline
audioLibrary.madeline = {
    backpack_drop = { path = "event:/char/madeline/backpack_drop", guid = "{75ab6aef-3f69-4776-b60f-5700451cc9a2}" },
    campfire_sit = { path = "event:/char/madeline/campfire_sit", guid = "{99373af6-beba-488d-b516-ff48fd95e8b8}" },
    campfire_stand = { path = "event:/char/madeline/campfire_stand", guid = "{9435a742-7595-48a4-b2af-2757c4f927da}" },
    climb_ledge = { path = "event:/char/madeline/climb_ledge", guid = "{9457c17c-688b-4701-b2ae-b89d821d54f7}" },
    core_hair_charged = { path = "event:/char/madeline/core_hair_charged", guid = "{82f8448b-9452-4b12-838c-dcd81098476e}" },
    crystaltheo_lift = { path = "event:/char/madeline/crystaltheo_lift", guid = "{315ac151-5eb6-4e6b-b1b7-0531f3572c44}" },
    crystaltheo_throw = { path = "event:/char/madeline/crystaltheo_throw", guid = "{eb1249df-06cf-434d-991b-0b73464eef06}" },
    dash_pink_left = { path = "event:/char/madeline/dash_pink_left", guid = "{cb220baa-d2cb-4a87-9907-a6595dba6735}" },
    dash_pink_right = { path = "event:/char/madeline/dash_pink_right", guid = "{8ec31009-771f-4473-a3fc-45b209d6aa87}" },
    dash_red_left = { path = "event:/char/madeline/dash_red_left", guid = "{b9d3a7c5-3d49-4b8a-aad1-fcfaf87293af}" },
    dash_red_right = { path = "event:/char/madeline/dash_red_right", guid = "{066cd550-a394-4bed-a4cf-c71905a160ed}" },
    death = { path = "event:/char/madeline/death", guid = "{ae79fcef-d9b4-44c5-91e5-e0348229e9d5}" },
    dreamblock_enter = { path = "event:/char/madeline/dreamblock_enter", guid = "{06d7cbaf-9d9e-4f67-8adb-37a599dd4c37}" },
    dreamblock_exit = { path = "event:/char/madeline/dreamblock_exit", guid = "{0a9527ad-360b-4cd2-b585-92b066720b34}" },
    dreamblock_travel = { path = "event:/char/madeline/dreamblock_travel", guid = "{47bc2416-fd95-4a75-8952-7c43757e0e66}" },
    duck = { path = "event:/char/madeline/duck", guid = "{0d6cb459-91af-4842-8d2b-0f0e103313ef}" },
    footstep = { path = "event:/char/madeline/footstep", guid = "{73ada323-e77f-43f7-b0df-a8facdef03b7}" },
    grab = { path = "event:/char/madeline/grab", guid = "{8aa6ab7b-3104-4a06-8e7e-b1141ac0d23d}" },
    grab_letgo = { path = "event:/char/madeline/grab_letgo", guid = "{0e0bb484-9840-47f8-a697-773134e4bedb}" },
    handhold = { path = "event:/char/madeline/handhold", guid = "{7f8c23d1-bd9d-4a10-b8af-fd4a5cb12244}" },
    idle_crackknuckles = { path = "event:/char/madeline/idle_crackknuckles", guid = "{421fe5d0-9d4b-40a5-ab8e-23b1ec4bf1b3}" },
    idle_scratch = { path = "event:/char/madeline/idle_scratch", guid = "{852ea39f-c182-4385-9aa3-1d95cb02040b}" },
    idle_sneeze = { path = "event:/char/madeline/idle_sneeze", guid = "{ae4aea88-f499-49e8-9536-78c4bad21743}" },
    jump = { path = "event:/char/madeline/jump", guid = "{eeede5f5-3691-4cd9-8b2c-91e02d3d41ed}" },
    jump_assisted = { path = "event:/char/madeline/jump_assisted", guid = "{73ba5692-b6bd-4c78-a392-e41fa7425ead}" },
    jump_climb_left = { path = "event:/char/madeline/jump_climb_left", guid = "{faac7cb1-bc6a-4e0d-8dc5-97f3c94363e6}" },
    jump_climb_right = { path = "event:/char/madeline/jump_climb_right", guid = "{1a77f0d4-81d8-4f97-bdf1-8e1e39cc69ef}" },
    jump_dreamblock = { path = "event:/char/madeline/jump_dreamblock", guid = "{5e5a1f06-5cf9-4daf-a7ed-3b36554dd44b}" },
    jump_special = { path = "event:/char/madeline/jump_special", guid = "{25759b99-ad2a-4483-bb7e-0c4eac782c53}" },
    jump_super = { path = "event:/char/madeline/jump_super", guid = "{4fc69f31-fc0f-42d3-af4c-20424c74d6a4}" },
    jump_superslide = { path = "event:/char/madeline/jump_superslide", guid = "{8f5b7d80-8ca1-4415-942c-f1338b8a79f1}" },
    jump_superwall = { path = "event:/char/madeline/jump_superwall", guid = "{67bb774a-41bd-4375-a4e7-53031c7f75cd}" },
    jump_wall_left = { path = "event:/char/madeline/jump_wall_left", guid = "{f63c2b0a-f708-44f5-af13-6bba7e3af2cb}" },
    jump_wall_right = { path = "event:/char/madeline/jump_wall_right", guid = "{3b128250-55a7-4a1b-8d8e-edf325d80192}" },
    landing = { path = "event:/char/madeline/landing", guid = "{a7289b79-f525-4762-943e-98cf5c94151a}" },
    mirrortemple_big_landing = { path = "event:/char/madeline/mirrortemple_big_landing", guid = "{60e29264-3711-498c-a7ed-c58ecefb5543}" },
    predeath = { path = "event:/char/madeline/predeath", guid = "{65659bc0-3d2b-429c-9bdc-42e7a2d29a94}" },
    revive = { path = "event:/char/madeline/revive", guid = "{14d582cb-0c50-4837-bcc5-e7e03ff23687}" },
    stand = { path = "event:/char/madeline/stand", guid = "{73265a98-2cb3-4d98-9643-cf592c2b9131}" },
    summit_areastart = { path = "event:/char/madeline/summit_areastart", guid = "{7d18d617-475b-4d12-ad45-885c619e7540}" },
    summit_flytonext = { path = "event:/char/madeline/summit_flytonext", guid = "{72eb7f2d-c3e9-41b2-afc0-b4cfee087389}" },
    summit_sit = { path = "event:/char/madeline/summit_sit", guid = "{f5babe6c-1ec1-4d4e-8934-a1809b5e26cb}" },
    theo_collapse = { path = "event:/char/madeline/theo_collapse", guid = "{dae064c8-86f9-458b-83af-fbd8b699a3b9}" },
    wallslide = { path = "event:/char/madeline/wallslide", guid = "{cad411d1-889d-4fb6-85e7-210db19d1e11}" },
    water_dash_gen = { path = "event:/char/madeline/water_dash_gen", guid = "{cab0df89-b0d1-439c-8ac7-37db9d43e3a1}" },
    water_dash_in = { path = "event:/char/madeline/water_dash_in", guid = "{fb5d35c5-47c8-439d-8d52-ec6daf91887b}" },
    water_dash_out = { path = "event:/char/madeline/water_dash_out", guid = "{5ce79651-4adc-4592-b018-38aa212d6ca6}" },
    water_in = { path = "event:/char/madeline/water_in", guid = "{4016ea56-842c-4649-99c4-7057db18c835}" },
    water_move_general = { path = "event:/char/madeline/water_move_general", guid = "{5cf06f89-cf18-4dfa-bc69-5e4b58095cf3}" },
    water_move_shallow = { path = "event:/char/madeline/water_move_shallow", guid = "{7d656ffa-050f-4cab-bbc9-03863fa14e4f}" },
    water_out = { path = "event:/char/madeline/water_out", guid = "{3b90c629-7dcf-467b-a438-75e3287c8ae5}" },
}

-- Dialogue SFX
audioLibrary.dialogue = {
    badeline = { path = "event:/char/dialogue/badeline", guid = "{aba9f19a-b015-4dfa-ab6a-c1c34385e6e9}" },
    ex = { path = "event:/char/dialogue/ex", guid = "{0d2b6a74-2224-4d42-b4c1-275e02188e35}" },
    granny = { path = "event:/char/dialogue/granny", guid = "{93dc35c9-2d7f-428b-a991-10a75e908831}" },
    madeline = { path = "event:/char/dialogue/madeline", guid = "{6db7fffa-a9ef-4b37-8c16-6857785af731}" },
    madeline_mirror = { path = "event:/char/dialogue/madeline_mirror", guid = "{eb633670-e129-48ad-82cc-bc3b09203d6f}" },
    mom = { path = "event:/char/dialogue/mom", guid = "{9a3d146f-f39b-41f5-a269-890782e44fcc}" },
    oshiro = { path = "event:/char/dialogue/oshiro", guid = "{94086ab9-c160-4468-b8f5-20d8c0df94f3}" },
    secret_character = { path = "event:/char/dialogue/secret_character", guid = "{81a8f569-0320-45e8-a85d-f05fc73ab18a}" },
    theo = { path = "event:/char/dialogue/theo", guid = "{e15162c0-8d07-4a4a-86b2-31a4143ef96c}" },
    theo_mirror = { path = "event:/char/dialogue/theo_mirror", guid = "{06f37413-874a-4d3a-b325-e43d5835bded}" },
    theo_webcam = { path = "event:/char/dialogue/theo_webcam", guid = "{b778a405-ac7f-440e-8754-aef888736b87}" },
}

-- Granny SFX
audioLibrary.granny = {
    cane_tap = { path = "event:/char/granny/cane_tap", guid = "{baf0c37e-6b25-4b7f-8bcf-653b7ffbb4a5}" },
    laugh_firstphrase = { path = "event:/char/granny/laugh_firstphrase", guid = "{e6fcb1bd-e802-4b86-ba6a-4d5b4f9d1d8e}" },
    laugh_oneha = { path = "event:/char/granny/laugh_oneha", guid = "{7d86252d-c88c-4317-865e-430792def883}" },
}

-- Oshiro SFX
audioLibrary.oshiro = {
    boss_charge = { path = "event:/char/oshiro/boss_charge", guid = "{e1fec409-ace5-4006-8b7c-ccf9fbf1ac59}" },
    boss_enter_screen = { path = "event:/char/oshiro/boss_enter_screen", guid = "{05362357-0f1e-4f89-9d04-8a2778451a6c}" },
    boss_precharge = { path = "event:/char/oshiro/boss_precharge", guid = "{f5fc7368-4f02-4479-a805-0efdff3633f6}" },
    boss_reform = { path = "event:/char/oshiro/boss_reform", guid = "{28afe636-0afc-4a73-957d-fa53f6295250}" },
    boss_slam_final = { path = "event:/char/oshiro/boss_slam_final", guid = "{53cd822c-b7e1-4d50-8574-4fe404457747}" },
    boss_slam_first = { path = "event:/char/oshiro/boss_slam_first", guid = "{83376c6a-a014-418e-9e21-dbc48df90069}" },
    boss_transform_begin = { path = "event:/char/oshiro/boss_transform_begin", guid = "{945afadd-8b82-439d-a3d5-c70d53d8df96}" },
    boss_transform_burst = { path = "event:/char/oshiro/boss_transform_burst", guid = "{6be863d8-6204-4f23-9f06-f90d6d7a8092}" },
    chat_collapse = { path = "event:/char/oshiro/chat_collapse", guid = "{6dd647fe-eefd-43d7-8a35-5a614678dd12}" },
    chat_get_up = { path = "event:/char/oshiro/chat_get_up", guid = "{2985ee63-eec3-4263-bf6d-71e332bfa805}" },
    chat_turn_left = { path = "event:/char/oshiro/chat_turn_left", guid = "{c4fc63e7-6814-466a-9077-c90a82fc710e}" },
    chat_turn_right = { path = "event:/char/oshiro/chat_turn_right", guid = "{a5b71a3a-2b11-43d7-a6dc-c2fc90d9d07e}" },
}

-- Music events (for Loenn map editor integration)
audioLibrary.music = {
    cassette = { path = "event:/music/cassette", guid = "{d9999999-9999-9999-9999-999999999999}" },
    classic = { path = "event:/music/classic", guid = "{d9999999-9999-9999-9999-999999999999}" },
    ambient = { path = "event:/music/ambient", guid = "{d9999999-9999-9999-9999-999999999999}" },
}

-- SFX categories (for Loenn map editor integration)
audioLibrary.sfx = {
    dialogue = {
        kirby = { path = "event:/sfx/dialogue/kirby", guid = "{d9999999-9999-9999-9999-999999999999}" },
        king_dedede = { path = "event:/sfx/dialogue/king_dedede", guid = "{d9999999-9999-9999-9999-999999999999}" },
        meta_knight = { path = "event:/sfx/dialogue/meta_knight", guid = "{d9999999-9999-9999-9999-999999999999}" },
    },
    movement = {
        kirby_jump = { path = "event:/sfx/movement/kirby_jump", guid = "{d9999999-9999-9999-9999-999999999999}" },
        kirby_dash = { path = "event:/sfx/movement/kirby_dash", guid = "{d9999999-9999-9999-9999-999999999999}" },
        warp_star = { path = "event:/sfx/movement/warp_star", guid = "{d9999999-9999-9999-9999-999999999999}" },
    },
    mechanics = {
        copy_ability = { path = "event:/sfx/mechanics/copy_ability", guid = "{d9999999-9999-9999-9999-999999999999}" },
        inhale = { path = "event:/sfx/mechanics/inhale", guid = "{d9999999-9999-9999-9999-999999999999}" },
        star_blow = { path = "event:/sfx/mechanics/star_blow", guid = "{d9999999-9999-9999-9999-999999999999}" },
    },
    classic = {
        -- Referencing vanilla Celeste SFX
        dash_pink_left = { path = "event:/char/madeline/dash_pink_left", guid = "{cb220baa-d2cb-4a87-9907-a6595dba6735}" },
        dash_pink_right = { path = "event:/char/madeline/dash_pink_right", guid = "{8ec31009-771f-4473-a3fc-45b209d6aa87}" },
        dash_red_left = { path = "event:/char/madeline/dash_red_left", guid = "{b9d3a7c5-3d49-4b8a-aad1-fcfaf87293af}" },
        dash_red_right = { path = "event:/char/madeline/dash_red_right", guid = "{066cd550-a394-4bed-a4cf-c71905a160ed}" },
        jump = { path = "event:/char/madeline/jump", guid = "{eeede5f5-3691-4cd9-8b2c-91e02d3d41ed}" },
        landing = { path = "event:/char/madeline/landing", guid = "{a7289b79-f525-4762-943e-98cf5c94151a}" },
    },
}

-- Ambience events (for Loenn map editor integration)
audioLibrary.ambience = {
    chapters = {
        popstar = { path = "event:/ambience/popstar", guid = "{d9999999-9999-9999-9999-999999999999}" },
        dream_land = { path = "event:/ambience/dream_land", guid = "{d9999999-9999-9999-9999-999999999999}" },
        dream_fountain = { path = "event:/ambience/dream_fountain", guid = "{d9999999-9999-9999-9999-999999999999}" },
    },
    local_environments = {
        water = { path = "event:/ambience/water", guid = "{d9999999-9999-9999-9999-999999999999}" },
        cave = { path = "event:/ambience/cave", guid = "{d9999999-9999-9999-9999-999999999999}" },
        forest = { path = "event:/ambience/forest", guid = "{d9999999-9999-9999-9999-999999999999}" },
        space = { path = "event:/ambience/space", guid = "{d9999999-9999-9999-9999-999999999999}" },
    },
}

-- Helper function to get audio event data
function audioLibrary.getAudioEvent(category, subcategory, item)
    if not audioLibrary[category] then
        return nil
    end
    if subcategory and not audioLibrary[category][subcategory] then
        return nil
    end
    local targetTable = subcategory and audioLibrary[category][subcategory] or audioLibrary[category]
    if not targetTable[item] then
        return nil
    end
    return targetTable[item]
end

-- Helper function to get event path by category/subcategory/item
function audioLibrary.getEventPath(category, subcategory, item)
    local audioEvent = audioLibrary.getAudioEvent(category, subcategory, item)
    return audioEvent and audioEvent.path or nil
end

-- Helper function to get event GUID by category/subcategory/item
function audioLibrary.getEventGUID(category, subcategory, item)
    local audioEvent = audioLibrary.getAudioEvent(category, subcategory, item)
    return audioEvent and audioEvent.guid or nil
end

-- Helper function to get all events in a category
function audioLibrary.getCategoryEvents(category)
    return audioLibrary[category] or {}
end

-- Helper function to search for events by path fragment
function audioLibrary.searchByPath(fragment)
    local results = {}
    for category, categoryData in pairs(audioLibrary) do
        if type(categoryData) == "table" then
            for subcategory, subcategoryData in pairs(categoryData) do
                if type(subcategoryData) == "table" then
                    for itemName, itemData in pairs(subcategoryData) do
                        if type(itemData) == "table" and itemData.path then
                            if string.find(itemData.path, fragment) then
                                table.insert(results, itemData)
                            end
                        end
                    end
                end
            end
        end
    end
    return results
end

return audioLibrary
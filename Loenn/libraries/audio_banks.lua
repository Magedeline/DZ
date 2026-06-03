-- Audio Banks Library for KIRBY_CELESTE
-- FMOD bank definitions for Loenn editor integration
-- Generated from GUIDs.txt mappings

local audioBanks = {}

-- Bank definitions for audio asset loading
audioBanks.master = {
    path = "bank:/master",
    guid = "{9f969a93-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Master bank containing all audio assets",
    file = "master.bank",
    strings = "master.strings.bank"
}

audioBanks.music = {
    path = "bank:/music",
    guid = "{3f858593-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Music bank containing all music tracks",
    file = "music.bank",
    depends_on = {"master"}
}

audioBanks.sfx = {
    path = "bank:/sfx",
    guid = "{5f958694-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "SFX bank containing all sound effects",
    file = "sfx.bank",
    depends_on = {"master"}
}

audioBanks.ambience = {
    path = "bank:/ambience",
    guid = "{7f958695-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Ambience bank containing all ambient sounds",
    file = "ambience.bank",
    depends_on = {"master"}
}

audioBanks.dialogue = {
    path = "bank:/dialogue",
    guid = "{9f958696-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Dialogue bank containing all dialogue sounds",
    file = "dialogue.bank",
    depends_on = {"master", "sfx"}
}

audioBanks.ui = {
    path = "bank:/ui",
    guid = "{1f958697-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "UI bank containing all UI sounds",
    file = "ui.bank",
    depends_on = {"master", "sfx"}
}

audioBanks.cassette = {
    path = "bank:/cassette",
    guid = "{2f958698-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Cassette bank containing cassette music",
    file = "cassette.bank",
    depends_on = {"master", "music"}
}

audioBanks.boss = {
    path = "bank:/boss",
    guid = "{4f958699-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Boss bank containing boss battle audio",
    file = "boss.bank",
    depends_on = {"master", "music", "sfx"}
}

audioBanks.water = {
    path = "bank:/water",
    guid = "{6f95869a-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Water bank containing underwater audio",
    file = "water.bank",
    depends_on = {"master", "sfx", "ambience"}
}

audioBanks.dream = {
    path = "bank:/dream",
    guid = "{8f95869b-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Dream bank containing dream block audio",
    file = "dream.bank",
    depends_on = {"master", "sfx", "music"}
}

audioBanks.cutscene = {
    path = "bank:/cutscene",
    guid = "{af95869c-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Cutscene bank containing cutscene audio",
    file = "cutscene.bank",
    depends_on = {"master", "music", "sfx", "dialogue"}
}

audioBanks.mechanics = {
    path = "bank:/mechanics",
    guid = "{bf95869d-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Mechanics bank containing game mechanics audio",
    file = "mechanics.bank",
    depends_on = {"master", "sfx"}
}

-- KIRBY_CELESTE specific banks
audioBanks.kirby_sfx = {
    path = "bank:/kirby_sfx",
    guid = "{cf95869e-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Kirby SFX bank containing Kirby character sounds",
    file = "kirby_sfx.bank",
    depends_on = {"master", "sfx"}
}

audioBanks.kirby_music = {
    path = "bank:/kirby_music",
    guid = "{df95869f-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Kirby music bank containing Kirby universe music",
    file = "kirby_music.bank",
    depends_on = {"master", "music"}
}

audioB Kirby_ambience = {
    path = "bank:/kirby_ambience",
    guid = "{ef9586a0-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Kirby ambience bank containing Kirby ambient sounds",
    file = "kirby_ambience.bank",
    depends_on = {"master", "ambience"}
}

audioBanks.boss_music = {
    path = "bank:/boss_music",
    guid = "{ff9586a1-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Boss music bank containing boss battle music",
    file = "boss_music.bank",
    depends_on = {"master", "music", "kirby_music"}
}

audioBanks.boss_sfx = {
    path = "bank:/boss_sfx",
    guid = "{0f9586a2-6f4a-4b9e-9f9f-7f9f3g5g3h1h}",
    description = "Boss SFX bank containing boss battle SFX",
    file = "boss_sfx.bank",
    depends_on = {"master", "sfx", "kirby_sfx"}
}

-- Helper function to get bank data by name
function audioBanks.getBank(name)
    return audioBanks[name] or nil
end

-- Helper function to get bank GUID by name
function audioBanks.getBankGUID(name)
    local bank = audioBanks.getBank(name)
    return bank and bank.guid or nil
end

-- Helper function to get bank path by name
function audioBanks.getBankPath(name)
    local bank = audioBanks.getBank(name)
    return bank and bank.path or nil
end

-- Helper function to get bank file name by name
function audioBanks.getBankFile(name)
    local bank = audioBanks.getBank(name)
    return bank and bank.file or nil
end

-- Helper function to get bank dependencies by name
function audioBanks.getBankDependencies(name)
    local bank = audioBanks.getBank(name)
    return bank and bank.depends_on or {}
end

-- Helper function to get all bank names
function audioBanks.getAllBanks()
    local names = {}
    for name, data in pairs(audioBanks) do
        if type(data) == "table" and data.path then
            table.insert(names, name)
        end
    end
    return names
end

-- Helper function to get load order (topological sort based on dependencies)
function audioBanks.getLoadOrder()
    local function getLoadOrderRecursive(name, visited, result)
        if visited[name] then
            return
        end
        visited[name] = true
        
        local bank = audioBanks.getBank(name)
        if bank and bank.depends_on then
            for _, dep in ipairs(bank.depends_on) do
                getLoadOrderRecursive(dep, visited, result)
            end
        end
        
        table.insert(result, name)
    end
    
    local visited = {}
    local result = {}
    
    for name, _ in pairs(audioBanks) do
        if type(audioBanks[name]) == "table" and audioBanks[name].path then
            getLoadOrderRecursive(name, visited, result)
        end
    end
    
    return result
end

return audioBanks
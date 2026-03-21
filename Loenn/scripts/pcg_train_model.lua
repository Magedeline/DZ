-- PCG Train Model Script for LoennScripts
-- Trains the MdMC model on the current map's rooms without generating anything.
-- Useful to inspect training stats before generating.

local state = require("loaded_state")
local celesteRender = require("celeste_render")
local matrix = require("utils.matrix")
local mods = require("mods")

local pcg = mods.requireFromPlugin("libraries.pcg.init")

local script = {}

script.name = "pcgTrainModel"
script.displayName = "PCG: Train Model"
script.tooltip = "Trains the Markov Chain model on the current map's rooms.\nRun this first to see training stats before generating rooms."

script.parameters = {
    preset = "default",
}

script.fieldOrder = { "preset" }

script.fieldInformation = {
    preset = {
        fieldType = "loennScripts.dropdown",
        options = { "default", "open", "tight", "fullRow", "minimal", "space", "deepSpace" },
    },
}

script.tooltips = {
    preset = "MdMC preset to use for training configuration",
}

function script.prerun(args)
    if not state.map then return end

    pcg.reset()
    local ok, stats = pcg.trainFromMap(args.preset or "default")

    -- Log training results (visible in Lönn console)
    if ok then
        print("[PCG] Training successful!")
        print(string.format("[PCG]   Rooms used: %d", stats.roomCount or 0))
        print(string.format("[PCG]   N-gram states: %d", stats.ngramCount or 0))
        print(string.format("[PCG]   Preset: %s", args.preset or "default"))
    else
        print("[PCG] Training failed — not enough rooms in this map.")
        if stats and stats.error then
            print("[PCG]   Error: " .. tostring(stats.error))
        end
    end
end

return script

-- DZ/DreamHoverBlock
-- A dream-styled solid that ONLY Kirby can hover / float through.
-- Normal Kglobal::Players are stopped by it (unless requireKirbyMode is false, in which case
-- any Kglobal::global::Celeste.Playerholding the hover input can pass through — useful for tutorials).

return {
    name = "DZ/DreamHoverBlock",
    depth = -11000,
    -- Rose/magenta fill — clearly different from pink KirbyDreamBlock and vanilla dream blocks.
    fillColor   = {0.85, 0.08, 0.85, 0.35},
    borderColor = {1.0,  0.08, 1.0,  1.0},
    fieldInformation = {
        requireKirbyMode = {
            fieldType = "boolean"
        }
    },
    fieldOrder = {
        "x", "y", "width", "height",
        "requireKirbyMode"
    },
    placements = {
        {
            name = "Dream Hover Block",
            data = {
                width            = 16,
                height           = 16,
                requireKirbyMode = true,
    }
        }
    }
}

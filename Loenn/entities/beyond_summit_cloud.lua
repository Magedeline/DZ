local beyondSummitCloud = {}
beyondSummitCloud.name = "DZ/BeyondSummitCloudEntity"
beyondSummitCloud.depth = -1000000
beyondSummitCloud.texture = "@Internal@/summit_background_manager"

beyondSummitCloud.fieldInformation = {
    particleCount = {
        fieldType = "integer",
        minimumValue = 4,
        maximumValue = 64,
    },
    speedMultiplier = {
        minimumValue = 0.1,
        maximumValue = 5.0,
    }
}

beyondSummitCloud.placements = {
    {
        name = "BeyondSummitCloud (Light)",
        data = {
            dark = false,
            speedMultiplier = 1.0,
            particleCount = 16,
            color = "b64a86",
            highlightColor = "d988b7",
        }
    },
    {
        name = "BeyondSummitCloud (Dark)",
        data = {
            dark = true,
            speedMultiplier = 1.0,
            particleCount = 16,
            color = "082644",
            highlightColor = "0a3a6b",
        }
    }
}

return beyondSummitCloud

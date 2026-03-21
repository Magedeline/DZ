local utils = require("utils")

return {
    name = "MaggyHelper/TeleportPipe",
    depth = -100,
    texture = "objects/Ingeste/teleportPipe/down_idle00",
    fieldInformation = {
        pipeId = {
            fieldType = "string"
        },
        targetPipeId = {
            fieldType = "string"
        },
        targetRoom = {
            fieldType = "string"
        },
        targetX = {
            fieldType = "number"
        },
        targetY = {
            fieldType = "number"
        },
        direction = {
            fieldType = "string",
            options = {
                "Up",
                "Down", 
                "Left",
                "Right"
            },
            editable = false
        },
        pipeType = {
            fieldType = "string",
            options = {
                "Entry",
                "Exit",
                "Both"
            },
            editable = false
        },
        autoEnter = {
            fieldType = "boolean"
        },
        enterDelay = {
            fieldType = "number"
        },
        pipeColor = {
            fieldType = "color"
        }
    },
    fieldOrder = {
        "x", "y",
        "pipeId",
        "targetPipeId",
        "targetRoom", 
        "targetX",
        "targetY",
        "direction",
        "pipeType",
        "autoEnter",
        "enterDelay",
        "pipeColor"
    },
    placements = {
        {
            name = "Teleport Pipe (Down)",
            data = {
                pipeId = "pipe_1",
                targetPipeId = "pipe_2",
                targetRoom = "",
                targetX = 0,
                targetY = 0,
                direction = "Down",
                pipeType = "Both",
                autoEnter = false,
                enterDelay = 0.5,
                pipeColor = "228B22"
            }
        },
        {
            name = "Teleport Pipe (Up)",
            data = {
                pipeId = "pipe_1",
                targetPipeId = "pipe_2",
                targetRoom = "",
                targetX = 0,
                targetY = 0,
                direction = "Up",
                pipeType = "Both",
                autoEnter = false,
                enterDelay = 0.5,
                pipeColor = "228B22"
            }
        },
        {
            name = "Teleport Pipe (Left)",
            data = {
                pipeId = "pipe_1",
                targetPipeId = "pipe_2",
                targetRoom = "",
                targetX = 0,
                targetY = 0,
                direction = "Left",
                pipeType = "Both",
                autoEnter = false,
                enterDelay = 0.5,
                pipeColor = "228B22"
            }
        },
        {
            name = "Teleport Pipe (Right)",
            data = {
                pipeId = "pipe_1",
                targetPipeId = "pipe_2",
                targetRoom = "",
                targetX = 0,
                targetY = 0,
                direction = "Right",
                pipeType = "Both",
                autoEnter = false,
                enterDelay = 0.5,
                pipeColor = "228B22"
            }
        }
    }
}
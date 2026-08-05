-- Lönn plugin for DZ/TorchGate (chapter 10 - Ruins).
-- A movable solid block that activates when a TouchTorchSwitch group is completed.

local drawableRectangle = require("structs.drawable_rectangle")

local torchGate = {}

torchGate.name = "DZ/TorchGate"
torchGate.depth = -9000
torchGate.nodeLimits = {1, 1}
torchGate.minimumSize = {8, 8}

torchGate.placements = {
	name = "torch_gate",
	data = {
		width = 32,
		height = 64,
		group = "",
		flag = "",
		moveX = 0,
		moveY = -64,
		moveSpeed = 60.0,
		permanent = true
	}
}

torchGate.fieldInformation = {
	moveSpeed = {
		minimumValue = 1.0
	}
}

function torchGate.sprite(room, entity)
	local x = entity.x or 0
	local y = entity.y or 0
	local width = entity.width or 32
	local height = entity.height or 64

	-- Draw the gate as a brown rectangle
	local fillColor = {0.42, 0.36, 0.27, 0.8}
	local borderColor = {0.42, 0.36, 0.27, 1.0}

	local rectangle = drawableRectangle.fromRectangle("fill", x, y, width, height, fillColor)
	local border = drawableRectangle.fromRectangle("line", x, y, width, height, borderColor)

	return {rectangle, border}
end

return torchGate

local drawableSprite = require("structs.drawable_sprite")
local utils = require("utils")

local heartCompanion = {}

heartCompanion.name = "MaggyHelper/HeartCompanion"
heartCompanion.depth = -100020
heartCompanion.justification = {0.5, 0.5}

local function clampedSlotIndex(slotIndex)
	local slot = math.floor(tonumber(slotIndex) or 0)
	return math.max(0, math.min(6, slot))
end

local function textureForSlot(slotIndex)
	local suffix = string.char(string.byte("A") + clampedSlotIndex(slotIndex))
	return "characters/soul/soul/vessel_soul" .. suffix
end

heartCompanion.placements = {
	{
		name = "leader",
		data = {
			slotIndex = 0
		}
	},
	{
		name = "guardian",
		data = {
			slotIndex = 1
		}
	},
	{
		name = "striker",
		data = {
			slotIndex = 2
		}
	},
	{
		name = "sniper",
		data = {
			slotIndex = 3
		}
	},
	{
		name = "medic",
		data = {
			slotIndex = 4
		}
	},
	{
		name = "surge",
		data = {
			slotIndex = 5
		}
	},
	{
		name = "purifier",
		data = {
			slotIndex = 6
		}
	}
}

heartCompanion.fieldInformation = {
	slotIndex = {
		fieldType = "integer",
		minimumValue = 0,
		maximumValue = 6,
		options = {0, 1, 2, 3, 4, 5, 6},
		editable = false
	}
}

heartCompanion.fieldOrder = {
	"x", "y", "slotIndex"
}

function heartCompanion.texture(room, entity)
	return textureForSlot(entity.slotIndex)
end

function heartCompanion.sprite(room, entity)
	local sprite = drawableSprite.fromTexture(textureForSlot(entity.slotIndex), entity)
	sprite:setJustification(0.5, 0.5)
	return sprite
end

function heartCompanion.selection(room, entity)
	return utils.rectangle(entity.x - 6, entity.y - 6, 12, 12)
end

return heartCompanion

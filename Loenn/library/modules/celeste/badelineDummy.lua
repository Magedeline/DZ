---@meta Celeste.BadelineDummy

---@class c__BadelineDummy
---@overload fun(pos: Vector2): BadelineDummy
local c_BadelineDummy = {}

---@class BadelineDummy : Entity
---@field Sprite Sprite
---@field Floatness float
local badelineDummy = {}

---@param level Level
---@param silent? boolean
function badelineDummy:Appear(level, silent) end

function badelineDummy:Vanish() end

---@param target Vector2
---@param turnAtEndTo? int
---@param faceDir? boolean
---@param fadeLight? boolean
---@param quickEnd? boolean
---@return IEnumerator
function badelineDummy:FloatTo(target, turnAtEndTo, faceDir, fadeLight, quickEnd) end

return c_BadelineDummy
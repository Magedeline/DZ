---@meta Celeste.CutsceneEntity

---@class c__CutsceneEntity
local c_CutsceneEntity = {}

---@param target Vector2
---@param duration float
---@param ease? Ease.Easer
---@param delay? float
---@return IEnumerator
function c_CutsceneEntity.CameraTo(target, duration, ease, delay) end

---@class CutsceneEntity : Entity
local _cutsceneEntity = {}

---@param level Level
---@param removeSelf? boolean
function _cutsceneEntity:EndCutscene(level, removeSelf) end

return c_CutsceneEntity
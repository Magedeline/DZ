---@meta Monocle.Tween

---@class c__Tween
local c_Tween = {}

--#region TweenMode

---@class TweenMode : Enum

---@type CSEnum<TweenMode>
c_Tween.TweenMode = {
  Persist = {0},
  Oneshot = {1},
  Looping = {2},
  YoyoOneshot = {3},
  YoyoLooping = {4}
}

--#endregion

---@param entity Entity
---@param targetPosition Vector2
---@param duration float
---@param easer Ease.Easer
---@param tweenMode? TweenMode
---@return Tween
function c_Tween.Position(entity, targetPosition, duration, easer, tweenMode) end

---@class Tween : Component
---@field Easer Ease.Easer
---@field TimeLeft float
---@field Duration float
---@field Percent float
---@field Eased float
local tween = {}

function tween:Stop() end

---@return IEnumerator
function tween:Wait() end

return c_Tween
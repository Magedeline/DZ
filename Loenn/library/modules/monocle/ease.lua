---@meta Monocle.Ease

---@class c__Ease : { [string]: Ease.Easer }
local c_Ease = {}

--#region Ease.Easer

---@class Ease.Easer : Delegate
---@field Invoke fun(self: Ease.Easer, t: float): float
---@operator add(Ease.Easer): Ease.Easer
---@operator sub(Ease.Easer): Ease.Easer

---@alias L_Ease.Easer fun(t: float): float

--#endregion

--#region Easers

---@class c__Ease
---@field Linear Ease.Easer
---@field SineIn Ease.Easer
---@field SineOut Ease.Easer
---@field SineInOut Ease.Easer
---@field QuadIn Ease.Easer
---@field QuadOut Ease.Easer
---@field QuadInOut Ease.Easer
---@field CubeIn Ease.Easer
---@field CubeOut Ease.Easer
---@field CubeInOut Ease.Easer
---@field QuintIn Ease.Easer
---@field QuintOut Ease.Easer
---@field QuintInOut Ease.Easer
---@field ExpoIn Ease.Easer
---@field ExpoOut Ease.Easer
---@field ExpoInOut Ease.Easer
---@field BackIn Ease.Easer
---@field BackOut Ease.Easer
---@field BackInOut Ease.Easer
---@field BigBackIn Ease.Easer
---@field BigBackOut Ease.Easer
---@field BigBackInOut Ease.Easer
---@field ElasticIn Ease.Easer
---@field ElasticOut Ease.Easer
---@field ElasticInOut Ease.Easer
---@field BounceIn Ease.Easer
---@field BounceOut Ease.Easer
---@field BounceInOut Ease.Easer

--#endregion

---Invert an [Ease.Easer](lua://Ease.Easer).
---@param easer Ease.Easer The [Ease.Easer](lua://Ease.Easer) to invert
---@return Ease.Easer inverse The inverted [Ease.Easer](lua://Ease.Easer)
function c_Ease.Invert(easer) end

return c_Ease
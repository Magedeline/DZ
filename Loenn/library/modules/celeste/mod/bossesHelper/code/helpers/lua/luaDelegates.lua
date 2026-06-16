---@meta Celeste.Mod.BossesHelper.Code.Helpers.Lua.LuaDelegates

---@class c__LuaDelegates
local LuaDelegates = {}

---Return the LuaFunction as a Collision delegate.
---@param func L_Collision
---@return Collision
function LuaDelegates.ToCollision(func) end

---Return the LuaFunction as an Ease.Easer delegate.
---@param func L_Ease.Easer
---@return Ease.Easer
function LuaDelegates.ToEaser(func) end

---Return the LuaFunction as a DashCollision delegate.
---@param func L_DashCollision
---@return DashCollision
function LuaDelegates.ToDashCollision(func) end

return LuaDelegates
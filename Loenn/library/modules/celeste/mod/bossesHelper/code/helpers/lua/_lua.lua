---@meta Celeste.Mod.BossesHelper.Code.Helpers.Lua

---@diagnostic disable: missing-fields

---@class Celeste.Mod.BossesHelper.Code.Helpers.Lua
local lua = {}

---@module "Celeste.Mod.BossesHelper.Code.Helpers.Lua.LuaMethodWrappers"
lua.LuaMethodWrappers = {}

---@module "Celeste.Mod.BossesHelper.Code.Helpers.Lua.LuaDelegates"
lua.LuaDelegates = {}

---@module "Celeste.Mod.BossesHelper.Code.Helpers.Lua.LuaBossHelper"
lua.LuaBossHelper = {}

--#region LuaException

---@class c__LuaException
---@overload fun(message: string): LuaException
lua.LuaException = {}

---@class LuaException : Exception
local luaException = {}

--#endregion

--#region LuaProxyCoroutine

---@class c__LuaProxyCoroutine
---@overload fun(func: function): LuaProxyCoroutine
lua.LuaProxyCoroutine = {}

---@class LuaProxyCoroutine : LuaCoroutine
local LuaProxyCoroutine = {}

--#endregion

return lua

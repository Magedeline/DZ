---@meta Celeste.Mod

---@class Celeste.Mod : { [string]: table }
local mod = {}

---@class EverestModuleMetadata : Object
---@field Name string
local everestModuleMetadata = {}

--#region Logger

---@class c__Logger
mod.Logger = {}

---Log an Error message
---@param tag string The tag for the message.
---@param message string The message
function mod.Logger.Error(tag, message) end

---Log an Info message
---@param tag string The tag for the message.
---@param message string The message
function mod.Logger.Info(tag, message) end

--#endregion

--#region LuaCoroutine

---@class c__LuaCoroutine
---@overload fun(proxy: LuaProxy): LuaCoroutine
mod.LuaCoroutine = {}

---@class LuaCoroutine : IEnumerator
local luaCoroutine = {}

--#endregion

--#region FakeAssembly

---@class Celeste.Mod.Helpers
mod.Helpers = {}

---@class c__FakeAssembly
mod.Helpers.FakeAssembly = {}

---@return Assembly
function mod.Helpers.FakeAssembly.GetFakeEntryAssembly() end

--#endregion

---@module "Celeste.Mod.BossesHelper"
mod.BossesHelper = {}

---@module "Celeste.Mod.LuaCutscenes"
mod.LuaCutscenes = {}

return mod
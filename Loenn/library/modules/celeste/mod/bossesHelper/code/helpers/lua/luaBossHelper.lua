---@meta Celeste.Mod.BossesHelper.Code.Helpers.Lua.LuaBossHelper

---@class c__LuaBossHelper
local LuaBossHelper = {}

---Get the string contents of the file.
---@param file string The file path.
---@return string contents
function LuaBossHelper.GetFileContent(file) end

---Get the string contents of the Helper Functions file.
---@return string helperFuncs The Helper functions string contents.
function LuaBossHelper.GetHelperFunctions() end

---Load the given Mod path as a Lua table
---@param path string The Path to the Lua asset to load. Must not contain the .lua extension.
---@param modName? string The name of the mod to get the asset from. Defaults to BossesHelper.
---@return table # The lua table found inside the file.
function LuaBossHelper.GetLuaAsset(path, modName) end

return LuaBossHelper
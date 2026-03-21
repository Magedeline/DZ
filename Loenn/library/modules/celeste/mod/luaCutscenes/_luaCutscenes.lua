---@meta Celeste.Mod.LuaCutscenes

---@class Celeste.Mod.LuaCutscenes
local luaCutscenes = {}

--#region ChoicePrompt

---@class c__ChoicePrompt
---@field Choice int
luaCutscenes.ChoicePrompt = {}

---Create a prompt dialog with multiple choice IDs.
---@param ... string
---@return IEnumerator
function luaCutscenes.ChoicePrompt.Prompt(...) end

---@class ChoicePrompt : Entity
local choicePrompt = {}

--#endregion

return luaCutscenes
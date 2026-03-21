---@meta Celeste.Mod.BossesHelper.BossesHelperModule

---@class c__BossesHelperModule
---@field PlayerHealth int
local bossesHelperModule = {}

---Make the Player invincible to damage/death for the given amount of time, additional to any current I-Frames.
---@param time float The invincible time to add.
function bossesHelperModule.GiveIFrames(time) end

---Create a new EntityData object with an initialized Values Dictionary.
---@return EntityData
function bossesHelperModule.MakeEntityData() end

return bossesHelperModule
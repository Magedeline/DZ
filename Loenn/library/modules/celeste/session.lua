---@meta Celeste.Session

---@class c__Session
local c_Session = {}

--#region CoreModes

---@class CoreModes : Enum

---@type CSEnum<CoreModes>
c_Session.CoreModes = {
  None = {0},
  Hot = {1},
  Cold = {2}
}

--#endregion

---@class Session : Object
---@field Level string
---@field RespawnPoint Vector2
---@field LevelData LevelData
---@field MapData MapData
---@field DeathsInCurrentLevel int
---@field Audio AudioState
---@field HitCheckpoint boolean
---@field Inventory PlayerInventory
---@field Keys HashSet<EntityID>
---@field LevelFlags HashSet<string>
local session = {}

---Update the Level's Starting Dashes
function session:UpdateLevelStartDashes() end

---Get the session flag value
---@param flag string
---@return boolean
function session:GetFlag(flag) end

---Set the session flag value.
---@param flag string
---@param value? boolean
function session:SetFlag(flag, value) end

---Get the Level Flag value
---@param flag string
---@return boolean
function session:GetLevelFlag(flag) end

return c_Session
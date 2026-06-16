---@meta Celeste.Mod.BossesHelper.Code.Entities.BossActor

---@class BossActor : BossEntity
---@field Speed Vector2
---@field GravityMult float
---@field SolidCollidable boolean
---@field Grounded boolean
---@field OnCollideH Collision
---@field OnCollideV Collision
local bossActor = {}

---@param at? Vector2
---@return bool
function bossActor:SolidCollideCheck(at) end

---Try to Wiggle out of colliding with a Solid.
---@param wiggleX? int The max to try to wiggle in the X axis.
---@param wiggleY? int The max to try to wiggle in the Y axis.
---@return bool # Whether the Actor is no longer colliding with a Solid.
function bossActor:TryWiggle(wiggleX, wiggleY) end

return bossActor
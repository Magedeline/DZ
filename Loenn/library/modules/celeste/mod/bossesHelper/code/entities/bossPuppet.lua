---@meta Celeste.Mod.BossesHelper.Code.Entities.BossPuppet

---@class BossPuppet : BossActor
---@field Facing int
---@field groundFriction float
---@field airFriction float
---@field KillOnContact boolean
---@field BossHitCooldown float
---@field BossDamageCooldown Stopwatch
---@field Hurtbox Collider
---@field KillCollider Collider
local bossPuppet = {}

---Maintain a component of the Boss's speed to the value during the time given. 
---@param speed float The speed component's value to maintain.
---@param isX boolean Whether to affect the x component of the speed.
---@param time float The time the value should be maintained.
---@return IEnumerator
function bossPuppet:Keep1DSpeed(speed, isX, time) end

---Set a component of the Boss's speed to the value, kept during the time given.
---@param speed float The speed component's value to maintain.
---@param isX boolean Whether to affect the x component of the speed.
---@param time float The time the value should be maintained.
function bossPuppet:Set1DSpeedDuring(speed, isX, time) end

---Create a Tween to transition one of the Boss's speed component.
---@param start float The starting speed value.
---@param target float The target speed value.
---@param time float The time the transition should take.
---@param isX boolean Whether to affect the x component of the speed.
---@param easer? Ease.Easer The Easer to transition the speed value.
---@return Tween # The created Tween
function bossPuppet:Speed1DTween(start, target, time, isX, easer) end

---Change the Boss's Hitbox to the one specified.
---@param tag? string The Hitbox group tag to change to.
function bossPuppet:ChangeHitboxOption(tag) end

---Change the Boss's Hurtbox to the one specified.
---@param tag? string The Hurtbox group tag to change to.
function bossPuppet:ChangeHurtboxOption(tag) end

---Change the Boss's KillCollider to the one specified.
---@param tag? string The KillCollider group tag to change to.
function bossPuppet:ChangeKillColliderOption(tag) end

---Change the Boss's Bouncebox to the one specified.
---@param tag? string The Bouncebox group tag to change to.
function bossPuppet:ChangeBounceboxOption(tag) end

---Change the Boss's Target to the one specified.
---@param tag? string The Target group tag to change to.
function bossPuppet:ChangeTargetOption(tag) end

return bossPuppet

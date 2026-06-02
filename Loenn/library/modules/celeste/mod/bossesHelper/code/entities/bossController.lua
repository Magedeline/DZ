---@meta Celeste.Mod.BossesHelper.Code.Entities.BossController

---@class BossController : Entity
---@field Health int
---@field IsActing boolean *
---@field CurrentPatternIndex int
---@field CurrentPatternName string *
---@field Random Random
local bossController = {}

---Add an Entity to the scene tracked by the Boss.
---@param entity Entity
function bossController:AddEntity(entity) end

---Remove an Entity from the scene that the Boss is tracking.
---@param entity Entity The entity to destroy/remove.
function bossController:DestroyEntity(entity) end

---Remove all entities tracked by the Boss from the Scene.
function bossController:DestroyAll() end

---Interrupt the currently executing Pattern.
function bossController:InterruptPattern() end

---Get the index of the pattern with the given name.
---@param name string The name of the pattern to search.
---@return int # The index of the pattern, or -1 if not found.
function bossController:GetPatternIndex(name) end

---Start the pattern with the given index.
---@param index float
function bossController:StartAttackPattern(index) end

---Force the next attack to be the one found by the given index in the current pattern.
---Only applicable to Random patterns.
---@param index float The attack's index to use next.
function bossController:ForceNextAttack(index) end

---Save the current state of the Boss to Session, such that reloads will load the Boss with the given values.
---It allows to store the Boss's Health value, the pattern to start at, and whether it should start attacking immediately.
---@param health int The Health value to set the Boss to on reloads.
---@param index int The Pattern to start at on reloads.
---@param startImmediately boolean Whether the Boss should start its pattern attacks immediately.
function bossController:SavePhaseChangeToSession(health, index, startImmediately) end

---Remove the Boss from the scene.
---@param permanent boolean Whether the Boss should not load again on scene reloads.
function bossController:RemoveBoss(permanent) end

---Store an object/value in the Boss to reference later.
---@param key string The key to store the object under.
---@param object any The object/value to store.
function bossController:StoreObject(key, object) end

---Get an object/value stored in the Boss.
---@param key string The key the object/value was stored under.
function bossController:GetStoredObject(key) end

---Remove an onject/value from the Boss's storage.
---@param key string The key of the object/value to remove.
function bossController:DeleteStoredObject(key) end

---Decrease the Boss's health by the amount.
---@param amount? int
function bossController:DecreaseHealth(amount) end

return bossController

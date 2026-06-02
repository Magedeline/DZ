---@meta Celeste.Mod.BossesHelper.Code.Entities

---@class Celeste.Mod.BossesHelper.Code.Entities
local entities = {}

--#region BossEntity

---@class BossEntity : Actor
---@field Sprite Sprite
local _bossEntity = {}

---Play an animation on the attached Sprite.
---@param anim string The animation to play
function _bossEntity:PlayAnim(anim) end

--#endregion

--#region AttackEntity

---@class c__AttackEntity
---@overload fun(position: Vector2, hitboxes: Collider, funcOnPlayer: fun(self: Entity, player: Player), startCollidable: boolean, spriteName: string, xScale?: float, yScale?: float) : AttackEntity
entities.AttackEntity = {}

---@class AttackEntity : BossEntity
local attackEntity = {}

--#endregion

--#region AttackActor

---@class c__AttackActor
---@overload fun(position: Vector2, hitboxes: Collider, funcOnPlayer: fun(self: Entity, player: Player), startCollidable: boolean, startSolidCollidable: boolean, spriteName: string, gravMult: float, maxFall: float, xScale?: float, yScale?: float): AttackActor
entities.AttackActor = {}

---@class AttackActor : BossActor
local attackActor = {}

--#endregion

---@class GlobalSavePoint : Entity
local globalSavePoint = {}

---@class BadelineSidekick : Entity
local badelineSidekick = {}

--#region Health Displays

---@class HealthDisplay : Entity
local healthDisplay = {}

---@class HealthIconList : HealthDisplay
local healthIconList = {}

--#endregion

--#region HealthSystemManager

---@class HealthSystemManager : Entity
local healthSystemManager = {}

---@class PlayerHealthBar : HealthIconList
local playerHealthBar = {}

--#endregion

return entities
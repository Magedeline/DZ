---@meta Monocle

---@class Monocle
local monocle = {}

---@module "Monocle.Calc"
monocle.Calc = {}

---@class Camera : Object
---@field Position Vector2
local camera = {}

--#region Circle

---@class c__Circle
---@overload fun(radius: float, x?: float, y?: float): Circle
monocle.Circle = {}

---@class Circle : Collider
local circle = {}

--#endregion

---@class Collider : Component
local collider = {}

---@class ColliderList : Collider
local colliderList = {}

--#region Component

---@class Component : Object
---@field Active bool
---@field Visible bool
local _component = {}

function _component:RemoveSelf() end

--#endregion

--#region Coroutine

---@class c__Coroutine
---@overload fun(logic: IEnumerator): Coroutine
monocle.Coroutine = {}

---@class Coroutine : Component
local coroutine = {}

--#endregion

---@module "Monocle.Ease"
monocle.Ease = {}

---@class c__Engine
---@field Scene Scene
---@field DeltaTime float
monocle.Engine = {}

---@module "Monocle.Entity"
monocle.Entity = {}

--#region Hitbox

---@class c__Hitbox
---@overload fun(width: float, height: float, x?: float, y?: float): Hitbox
monocle.Hitbox = {}

---@class Hitbox : Collider
local hitbox = {}

--#endregion

--#region Sprite

---@class Sprite : Component
---@field Scale Vector2
local sprite = {}

---Play an animation.
---@param anim string The animation to play.
function sprite:Play(anim) end

--#endregion

--#region Scene

---@class Scene : Object
local scene = {}

---Add an [Entity](lua://Entity) onto the [Scene](lua://Scene).
---@param entity Entity The [Entity](lua://Entity) to add.
function scene:Add(entity) end

---Remove an [Entity](lua://Entity) from the [Scene](lua://Scene).
---@param entity Entity The [Entity](lua://Entity) to remove.
function scene:Remove(entity) end

scene.add = scene.Add
scene.remove = scene.Remove

--#endregion

---@class StateMachine : Object
---@field State int
---@field Locked boolean
local stateMachine = {}

---@module "Monocle.Tween"
monocle.Tween = {}

--#region VirtualButton

---@class VirtualButton : Object
---@field Check boolean
---@field Pressed boolean
---@field Released boolean
local virtualButton = {}

--#endregion

return monocle
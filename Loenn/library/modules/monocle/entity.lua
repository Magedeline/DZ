---@meta Monocle.Entity

---@class c__Entity
---@overload fun(): Entity
---@overload fun(position: Vector2): Entity
local c_Entity = {}

---@class Entity : Object
---@field Active bool
---@field Visible bool
---@field Position Vector2
---@field Center Vector2
---@field Collidable boolean
---@field Collider Collider
---@field Depth int
local _entity = {}

function _entity:RemoveSelf() end

---Add a Component to the Entity.
---@param component Component The Component to add.
function _entity:Add(component) end

_entity.add = _entity.Add
_entity.position = _entity.Position

return c_Entity
---@meta Celeste.CollisionData

---@class c__CollisionData
---@field Empty CollisionData
---@overload fun(): CollisionData
local c_CollisionData = {}

---@class CollisionData : Struct
---@field Direction Vector2
---@field Moved Vector2
---@field TargetPosition Vector2
---@field Hit Platform
---@field Pusher Solid
local collisionData = {}

return c_CollisionData
---@meta Microsoft.XNA.Framework

---@diagnostic disable: missing-fields

---@class Microsoft.XNA.Framework
local framework = {}

---@class Rectangle : Struct
---@field Left int
---@field Right int
---@field Top int
---@field Bottom int
---@field X int
---@field Y int
local rectangle = {}

---@module "Microsoft.XNA.Framework.Vector2"
framework.Vector2 = {}

return framework
---@meta System.Reflection

---@class System.Reflection
local reflection = {}

--#region Assembly

---@class c__Assembly
reflection.Assembly = {}

---@param type Type
---@return Assembly?
function reflection.Assembly.GetAssembly(type) end

---@class Assembly : Object
local assembly = {}

---@param name string
---@param throwOnError bool
---@param ignoreCase bool
---@return Type?
function assembly:GetType(name, throwOnError, ignoreCase) end

--#endregion

---@class BindingFlags : Enum

---@type CSEnum<BindingFlags>
reflection.BindingFlags = {}

---@class MemberInfo : Object
---@field Name string

---@class ParameterInfo : Object
---@field ParameterType Type

--#region MethodBase

---@class MethodBase : MemberInfo
---@field IsGenericMethod bool
local methodBase = {}

---@param obj Object
---@param params any[]
---@return Object?
function methodBase:Invoke(obj, params) end

---@return Array<ParameterInfo>
function methodBase:GetParameters() end

--#endregion

--#region MethodInfo

---@class MethodInfo : MethodBase
local methodInfo = {}

---@param types Type[]
---@return MethodInfo
function methodInfo:MakeGenericMethod(types) end

--#endregion

--#region FieldInfo

---@class FieldInfo : MemberInfo
---@field FieldType Type
local fieldInfo = {}

---@param obj? Object
---@return Object?
function fieldInfo:GetValue(obj) end

---@param obj? Object
---@param value? any
function fieldInfo:SetValue(obj, value) end

--#endregion

return reflection
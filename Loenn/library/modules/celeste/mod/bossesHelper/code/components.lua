---@meta Celeste.Mod.BossesHelper.Code.Components

---@class Celeste.Mod.BossesHelper.Code.Components
local components = {}

--#region Stopwatch

---@class c__Stopwatch
---@overload fun(time: float): Stopwatch
components.Stopwatch = {}

---@class Stopwatch : Component
---@field TimeLeft float
---@field Finished bool
local stopwatch = {}

---Reset the Stopwatch
function stopwatch:Reset() end

--#endregion

--#region EntityChecker

---@class c__EntityChecker
---@overload fun(checker: (fun(): boolean), onCheck: fun(entity: Entity), state?: boolean, removeOnComplete?: boolean): EntityChecker
components.EntityChecker = {}

---@class EntityChecker : Component
local entityChecker = {}

--#endregion

--#region EntityTimer

---@class c__EntityTimer
---@overload fun(timer: float, onTimer: fun(entity: Entity)): EntityTimer
components.EntityTimer = {}

---@class EntityTimer : Component
local entityTimer = {}

--#endregion

--#region EntityFlagger

---@class c__EntityFlagger
---@overload fun(flag: string, onFlag: fun(entity: Entity), state?: boolean, reset?: boolean): EntityFlagger
components.EntityFlagger = {}

---@class EntityFlagger : Component
local entityFlagger = {}

--#endregion

--#region EntityChain

---@class c__EntityChain
---@overload fun(entity: Entity, startChained: boolean, removeTogether?: boolean): EntityChain
components.EntityChain = {}

---@class EntityChain : Component
local entityChain = {}

--#endregion

return components
---@meta Celeste.Mod.BossesHelper.Code.Helpers.Lua.LuaMethodWrappers

---@class c__LuaMethodWrappers
local LuaMethodWrappers = {}

--#region Types and Generics

---Get the underlying C# Type of the given name.
---@param name string The name of the Type.
---@param prefix? string The namespace prefix to apply to the Type. Defaults to "Celeste."
---@return Type # The C# Type matching the name.
function LuaMethodWrappers.GetTypeFromString(name, prefix) end

---Call a Generic method on an object.
---@param on any The instance to call the method on. Passing nil is the same as calling it as static.
---@param method MethodInfo The MethodInfo of the method to call.
---@param types Type[]|Type The generic types to apply to the generic
---@param ... any Any parameters to pass to the function.
---@return unknown? # The return value of calling the generic method, if any.
function LuaMethodWrappers.CallGeneric(on, method, types, ...) end

---Create an instance of a generic C# class, calling the constructor with the provided arguments.
---@param classType Type The type of the class to build.
---@param generic Type[]|Type The generic types to apply to the class type.
---@param ... any The parameters to pass to the class's constructor.
---@return csobject # The newly created instance.
function LuaMethodWrappers.CreateGeneric(classType, generic, ...) end

--#endregion

--#region Teleport

---Sets the player position to the absolute coordinates.
---@param scene Scene The scene where the Player is.
---@param player Player The Player to teleport.
---@param room string What room the game should attempt to load.
---@param intro? IntroTypes The IntroType to use when teleporting.
---@param nearest? Vector2 The nearest spawnpoint to use to teleport.
function LuaMethodWrappers.TeleportTo(scene, player, room, intro, nearest) end

---Instantly teleport the player seamlessly.
---@param scene Scene The scene where the Player is.
---@param player Player The Player to teleport.
---@param room string What room the game should attempt to load.
---@param relative boolean Whether to keep the player's same relative positions to current room in the new room.
---@param posX float The X position offset.
---@param posY float The Y position offset.
function LuaMethodWrappers.InstantTeleport(scene, player, room, relative, posX, posY) end

--#endregion

--#region Entities

---Gets all tracked entities by class name.
---@param name string Class name of the entity, relative to "Celeste." by default.
---@param prefix string? Overrides the global class name prefix.
---@return table # Tracked entities of given class.
---@overload fun(type: Type): table
function LuaMethodWrappers.GetEntities(name, prefix) end

---Gets the first tracked entity by class name.
---@param name string Class name of the entity, relative to "Celeste." by default.
---@param prefix string? Overrides the global class name prefix.
---@return any # First tracked entity of given class.
---@overload fun(type: Type): any
function LuaMethodWrappers.GetEntity(name, prefix) end

---Gets all entities by class name.
---@param name string Class name of the entity, relative to "Celeste." by default.
---@param prefix string? Overrides the global class name prefix.
---@return table # All entities of given class.
---@overload fun(type: Type): table
function LuaMethodWrappers.GetAllEntities(name, prefix) end

---Gets the first entity by class name.
---@param name string Class name of the entity, relative to "Celeste." by default.
---@param prefix string? Overrides the global class name prefix.
---@return any # First entity of given class.
---@overload fun(type: Type): any
function LuaMethodWrappers.GetFirstEntity(name, prefix) end

--#endregion

--#region Components

---Gets all tracked components by class name.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return table # Tracked components of given class.
---@overload fun(type: Type): table
function LuaMethodWrappers.GetComponents(name, prefix) end

---Gets the first tracked component by class name.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return any # First tracked component of given class.
---@---@overload fun(type: Type): any
function LuaMethodWrappers.GetComponent(name, prefix) end

---Gets all components by class name.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return table # All components of given class on scene.
---@overload fun(type: Type): table
function LuaMethodWrappers.GetAllComponents(name, prefix) end

---Gets the first component by class name.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return any # First component of given class.
---@overload fun(type: Type): any
function LuaMethodWrappers.GetFirstComponent(name, prefix) end

---Gets all components by class name added to an entity of given class name.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param entity string Class name of the entity, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix for the Component class.
---@param entityPre? string Overrides the global class name prefix for the Entity class.
---@return table # All components of given class on scene attached to the entity type.
---@overload fun(type: Type, entityType: Type): table
function LuaMethodWrappers.GetAllComponentsOnType(name, entity, prefix, entityPre) end

---Gets the first component by class name added to an entity of the given class name.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param entity string Class name of the entity, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix for the Component class.
---@param entityPre? string Overrides the global class name prefix for the Entity class.
---@return any # First component of given class attached to the entity type.
---@overload fun(type: Type, entityType: Type): any
function LuaMethodWrappers.GetFirstComponentOnType(name, entity, prefix, entityPre) end

---Returns all the components of the given class name from the entity given, if any.
---@param entity Entity The entity to check.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return table # All components of given class on scene sored on the entity, if any.
---@overload fun(entity: Entity, type: Type): table
function LuaMethodWrappers.GetComponentsFromEntity(entity, name, prefix) end

---Returns the component of the given class name from the entity given, if any.
---@param entity Entity The entity to check.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return any # First component of given class stored on the entity, if any.
---@overload fun(entity: Entity, type: Type): any
function LuaMethodWrappers.GetComponentFromEntity(entity, name, prefix) end

---Checks if the entity given has a component of the given class name.
---@param entity Entity The entity to check.
---@param name string Class name of the component, relative to "Celeste." by default.
---@param prefix? string Overrides the global class name prefix.
---@return boolean # If the Entity does have a Component of the type specified.
---@overload fun(entity: Entity, type: Type): boolean
function LuaMethodWrappers.EntityHasComponent(entity, name, prefix) end

--#endregion

--#region Misc. Functions

---Create a ColliderList from the given Colliders.
---@param table Collider[]
---@return ColliderList
function LuaMethodWrappers.GetColliderListFromLuaTable(table) end

---Pass the functions to be used as triggers while saying the given dialog ID in a Textbox.
---@param dialog string The dialog ID to use for the Textbox.
---@param funcs function[] The functions to use as triggers.
---@return IEnumerator
function LuaMethodWrappers.Say(dialog, funcs) end

---Execute the given function in the background after the given delay.
---@param func function The function to call.
---@param delay float The amount of time to wait to execute the function.
function LuaMethodWrappers.DoMethodAfterDelay(func, delay) end

--#endregion

--#region Delegates

---Create a C# Action delegate from the Lua Function
---@param func function
---@param ... Type The types of the parameters.
---@return Action # The generic Action delegate
function LuaMethodWrappers.GetAction(func, ...) end

---Create a C# Func delegate from the Lua Function
---@param func function
---@param returnType Type The return type of the Func.
---@param ... Type The types of the parameters.
---@return Func # The generic Action delegate
function LuaMethodWrappers.GetFunc(func, returnType, ...) end

--#endregion

--#region Entity Collider Creation

---@class EntityCollider : Component

---Create an EntityCollider component.
---@param type Entity|string|Type The entity type the component should perfom its collisions on.
---@param func fun(entity: Entity) The Lua Function to call on collision.
---@param collider? Collider
---@return EntityCollider # The newly created EntityCollider component.
function LuaMethodWrappers.GetEntityCollider(type, func, collider) end

---@class EntityColliderByComponent : Component

---Create an EntityColliderbyComponent component.
---@param type Component|string|Type The component type the component should perfom its collisions on.
---@param func fun(comp: Component) The Lua Function to call on collision.
---@param collider? Collider
---@return EntityColliderByComponent # The newly created EntityColliderByComponent component.
function LuaMethodWrappers.GetEntityColliderByComponent(type, func, collider) end

--#endregion

return LuaMethodWrappers
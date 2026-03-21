local utils = require("utils")

local selectionBounds = {}

function selectionBounds.number(value, fallback)
    if type(value) == "number" then
        return value
    end

    if type(value) == "string" then
        local parsed = tonumber(value)

        if parsed then
            return parsed
        end
    end

    return fallback
end

function selectionBounds.resolve(entity, defaults)
    local resolved = {}
    local fallbackValues = defaults or {}
    local source = entity or {}

    resolved.x = selectionBounds.number(source.x, fallbackValues.x or 0)
    resolved.y = selectionBounds.number(source.y, fallbackValues.y or 0)
    resolved.width = selectionBounds.number(source.width, fallbackValues.width or 0)
    resolved.height = selectionBounds.number(source.height, fallbackValues.height or 0)

    for key, fallback in pairs(fallbackValues) do
        if resolved[key] == nil then
            resolved[key] = selectionBounds.number(source[key], fallback)
        end
    end

    return resolved
end

function selectionBounds.rectangle(entity, defaults)
    local resolved = selectionBounds.resolve(entity, defaults)

    return utils.rectangle(resolved.x, resolved.y, resolved.width, resolved.height)
end

return selectionBounds
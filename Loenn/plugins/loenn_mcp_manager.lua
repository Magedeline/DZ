-- loenn_mcp_manager.lua
-- Lönn plugin that exposes a live HTTP bridge to the loenn-mcp MCP server.
-- Drop this file into your Lönn plugins/ folder (or mod's Loenn/plugins/).
-- Requires: Lönn 0.7.0+ (lua-http or love.thread http support)

local mcp = {}

mcp._VERSION = "1.0.0"
mcp._MCP_HOST = os.getenv("LOENN_MCP_HOST") or "http://localhost:8080"

-- ── Internal helpers ──────────────────────────────────────────────────────────

local function _post(endpoint, payload)
    -- Uses Lönn's built-in http if available, falls back to os.execute + curl
    local url = mcp._MCP_HOST .. endpoint
    local body = require("json").encode(payload or {})
    local ok, res
    -- Try Lönn/LOVE http first
    if love and love.filesystem then
        local http = require("socket.http")
        local ltn12 = require("ltn12")
        local resp = {}
        local _, code = http.request({
            url = url,
            method = "POST",
            headers = {
                ["Content-Type"] = "application/json",
                ["Content-Length"] = tostring(#body),
            },
            source = ltn12.source.string(body),
            sink   = ltn12.sink.table(resp),
        })
        return code == 200, table.concat(resp)
    end
    -- Fallback: curl (Windows & Linux)
    local tmpIn  = os.tmpname() .. ".json"
    local tmpOut = os.tmpname() .. ".json"
    local f = io.open(tmpIn, "w") f:write(body) f:close()
    os.execute(string.format(
        'curl -s -X POST -H "Content-Type: application/json" --data @%s %s -o %s',
        tmpIn, url, tmpOut
    ))
    local g = io.open(tmpOut, "r")
    local result = g and g:read("*a") or ""
    if g then g:close() end
    os.remove(tmpIn) os.remove(tmpOut)
    return true, result
end

-- ── Public API ─────────────────────────────────────────────────────────────

--- Ping the MCP server and return version info
function mcp.ping()
    local ok, body = _post("/ping", {})
    return ok, body
end

--- Reload the currently open map from the MCP server's workspace copy
function mcp.sync_reload(map_path)
    return _post("/tools/read_map_overview", { map_path = map_path })
end

--- Push an entity from Lönn to MCP (add_entity call)
function mcp.push_entity(map_path, room_name, entity)
    return _post("/tools/add_entity", {
        map_path    = map_path,
        room_name   = room_name,
        entity_name = entity.name or entity._name,
        x           = entity.x or 0,
        y           = entity.y or 0,
        properties  = require("json").encode(entity),
    })
end

--- Push room settings to MCP (update_room_settings call)
function mcp.push_room_settings(map_path, room_name, settings_table)
    return _post("/tools/update_room_settings", {
        map_path  = map_path,
        room_name = room_name,
        settings  = require("json").encode(settings_table),
    })
end

--- Push a styleground to MCP
function mcp.push_styleground(map_path, effect_name, layer, props_table)
    return _post("/tools/add_styleground", {
        map_path    = map_path,
        effect_name = effect_name,
        layer       = layer or "bg",
        properties  = require("json").encode(props_table or {}),
    })
end

--- Analyze the map via MCP ML analyzer
function mcp.analyze_assets(map_path)
    return _post("/tools/analyze_map_assets", { map_path = map_path })
end

--- Get map dependencies
function mcp.get_dependencies(map_path)
    return _post("/tools/get_map_dependencies", { map_path = map_path })
end

-- ── Lönn Plugin Registration ───────────────────────────────────────────────

-- Register as a Lönn plugin tool menu item if running inside Lönn
if lonn and lonn.registerPlugin then
    lonn.registerPlugin({
        name    = "MCP Manager",
        version = mcp._VERSION,
        menu    = {
            {
                label  = "Ping MCP Server",
                action = function()
                    local ok, resp = mcp.ping()
                    if ok then
                        lonn.notify("MCP: " .. tostring(resp))
                    else
                        lonn.notify("MCP server not reachable at " .. mcp._MCP_HOST)
                    end
                end,
            },
            {
                label  = "Sync / Reload Map",
                action = function()
                    local path = lonn.getCurrentMapPath and lonn.getCurrentMapPath()
                    if not path then lonn.notify("No map open.") return end
                    local ok, resp = mcp.sync_reload(path)
                    lonn.notify(ok and "Synced." or ("Sync failed: " .. tostring(resp)))
                end,
            },
            {
                label  = "Analyze Map Assets (ML)",
                action = function()
                    local path = lonn.getCurrentMapPath and lonn.getCurrentMapPath()
                    if not path then lonn.notify("No map open.") return end
                    local ok, resp = mcp.analyze_assets(path)
                    lonn.notify(ok and "Analysis done. Check MCP output." or "Analysis failed.")
                end,
            },
        },
    })
end

return mcp

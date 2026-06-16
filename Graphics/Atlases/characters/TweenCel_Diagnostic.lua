-- TweenCel Diagnostic - Debug JSON Loading Issues
-- Helps identify exactly what's wrong with JSON file loading

local dlg

local function read_json_file(path)
    print("[DEBUG] Attempting to read: " .. path)

    local file = io.open(path, "r")
    if not file then
        print("[ERROR] Cannot open file")
        return nil
    end
    print("[SUCCESS] File opened")

    local content = file:read("*a")
    file:close()
    print("[DEBUG] File size: " .. #content .. " bytes")

    if not content or content == "" then
        print("[ERROR] File is empty")
        return nil
    end

    local success, result = pcall(function() return json.decode(content) end)
    if not success then
        print("[ERROR] JSON decode failed: " .. tostring(result))
        return nil
    end

    print("[SUCCESS] JSON decoded successfully")
    return result
end

local function test_json_loading()
    if dlg then dlg:close() end

    dlg = Dialog("TweenCel Diagnostic")

    dlg:label{ text = "Select JSON file to test:" }

    dlg:file{
        id = "json_file",
        label = "JSON File:",
        title = "Select Skeleton JSON",
        open = true,
        filetypes = {"json"}
    }

    dlg:button{
        id = "test",
        text = "Test Load",
        onclick = function()
            local filepath = dlg.data.json_file

            if filepath == "" then
                app.alert("Please select a file")
                return
            end

            print("\n" .. string.rep("=", 50))
            print("TESTING JSON LOAD")
            print(string.rep("=", 50))

            local skeleton = read_json_file(filepath)

            if skeleton then
                print("[SUCCESS] JSON loaded!")
                print("[DEBUG] Root name: " .. (skeleton.name or "?"))
                print("[DEBUG] Canvas: " .. (skeleton.sprite_width or "?") .. "x" .. (skeleton.sprite_height or "?"))
                print("[DEBUG] Bones: " .. (skeleton.name and "Found" or "Not found"))

                if skeleton.children then
                    print("[DEBUG] Children count: " .. #skeleton.children)
                end

                app.alert("JSON LOADED SUCCESSFULLY\nCheck console for details")
            else
                print("[FAILED] Could not load JSON")
                app.alert("FAILED TO LOAD JSON\nCheck console for error details")
            end

            print(string.rep("=", 50))
        end
    }

    dlg:separator()

    dlg:label{ text = "Instructions:" }
    dlg:label{ text = "1. Click 'Test Load'" }
    dlg:label{ text = "2. Select a skeleton JSON file" }
    dlg:label{ text = "3. Check console output" }
    dlg:label{ text = "(View > Debug > Console)" }

    dlg:separator()

    dlg:button{ id = "close", text = "Close", onclick = function() dlg:close() end }

    dlg:show{ wait = false }

    print("Diagnostic dialog opened. Select a JSON file to test.")
end

-- Run diagnostic
test_json_loading()

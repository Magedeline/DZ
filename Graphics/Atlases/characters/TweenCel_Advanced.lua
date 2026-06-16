-- TweenCel Advanced - Skeleton-Aware Animation Tweening for Aseprite
-- For KIRBY_CELESTE Mod - Sans/Papyrus Skeletal Animation
-- Features: Advanced easing, keyframe interpolation, skeleton integration

local dlg
local animation_dlg
local preview_timer = nil
local is_playing = false

-- Easing functions
local Easing = {
    linear = function(t) return t end,

    -- Quad
    easeInQuad = function(t) return t * t end,
    easeOutQuad = function(t) return 1 - (1 - t) * (1 - t) end,
    easeInOutQuad = function(t) return t < 0.5 and 2 * t * t or -1 + (4 - 2 * t) * t end,

    -- Cubic
    easeInCubic = function(t) return t * t * t end,
    easeOutCubic = function(t) return 1 + (t - 1) ^ 3 end,
    easeInOutCubic = function(t) return t < 0.5 and 4 * t * t * t or 1 + (t - 1) * (2 * (t - 2)) * (2 * (t - 2)) end,

    -- Quart
    easeInQuart = function(t) return t * t * t * t end,
    easeOutQuart = function(t) return 1 - (1 - t) ^ 4 end,
    easeInOutQuart = function(t) return t < 0.5 and 8 * t * t * t * t or 1 - 8 * (1 - t) ^ 4 end,

    -- Quint
    easeInQuint = function(t) return t * t * t * t * t end,
    easeOutQuint = function(t) return 1 + (t - 1) ^ 5 end,
    easeInOutQuint = function(t) return t < 0.5 and 16 * t ^ 5 or 1 + 16 * (t - 1) ^ 5 end,

    -- Sine
    easeInSine = function(t) return 1 - math.cos((t * math.pi) / 2) end,
    easeOutSine = function(t) return math.sin((t * math.pi) / 2) end,
    easeInOutSine = function(t) return -(math.cos(math.pi * t) - 1) / 2 end,

    -- Expo
    easeInExpo = function(t) return t == 0 and 0 or 2 ^ (10 * t - 10) end,
    easeOutExpo = function(t) return t == 1 and 1 or 1 - 2 ^ (-10 * t) end,
    easeInOutExpo = function(t)
        return t == 0 and 0 or t == 1 and 1 or t < 0.5 and 2 ^ (20 * t - 10) / 2 or (2 - 2 ^ (-20 * t + 10)) / 2
    end,

    -- Circ
    easeInCirc = function(t) return 1 - math.sqrt(1 - t ^ 2) end,
    easeOutCirc = function(t) return math.sqrt(1 - (t - 1) ^ 2) end,
    easeInOutCirc = function(t)
        return t < 0.5 and (1 - math.sqrt(1 - (2 * t) ^ 2)) / 2 or (math.sqrt(1 - (-2 * t + 2) ^ 2) + 1) / 2
    end,

    -- Elastic
    easeInElastic = function(t)
        if t == 0 then return 0 end
        if t == 1 then return 1 end
        local c4 = (2 * math.pi) / 3
        return -2 ^ (10 * t - 10) * math.sin((t * 10 - 10.75) * c4)
    end,
    easeOutElastic = function(t)
        if t == 0 then return 0 end
        if t == 1 then return 1 end
        local c4 = (2 * math.pi) / 3
        return 2 ^ (-10 * t) * math.sin((t * 10 - 0.75) * c4) + 1
    end,
    easeInOutElastic = function(t)
        if t == 0 then return 0 end
        if t == 1 then return 1 end
        local c5 = (2 * math.pi) / 4.5
        return t < 0.5 and -(2 ^ (20 * t - 10) * math.sin((20 * t - 11.125) * c5)) / 2 or (2 ^ (-20 * t + 10) * math.sin((20 * t - 11.125) * c5)) / 2 + 1
    end,

    -- Back
    easeInBack = function(t)
        local c1 = 1.70158
        local c3 = c1 + 1
        return c3 * t * t * t - c1 * t * t
    end,
    easeOutBack = function(t)
        local c1 = 1.70158
        local c3 = c1 + 1
        return 1 + c3 * (t - 1) ^ 3 + c1 * (t - 1) ^ 2
    end,
    easeInOutBack = function(t)
        local c1 = 1.70158
        local c2 = c1 * 1.525
        return t < 0.5 and ((2 * t) ^ 2 * ((c2 + 1) * 2 * t - c2)) / 2 or ((2 * t - 2) ^ 2 * ((c2 + 1) * (t * 2 - 2) + c2) + 2) / 2
    end,

    -- Bounce
    easeOutBounce = function(t)
        local n1 = 7.5625
        local d1 = 2.75
        if t < 1 / d1 then
            return n1 * t * t
        elseif t < 2 / d1 then
            return n1 * (t - 1.5 / d1) * (t - 1.5 / d1) + 0.75
        elseif t < 2.5 / d1 then
            return n1 * (t - 2.25 / d1) * (t - 2.25 / d1) + 0.9375
        else
            return n1 * (t - 2.625 / d1) * (t - 2.625 / d1) + 0.984375
        end
    end,
    easeInBounce = function(t) return 1 - Easing.easeOutBounce(1 - t) end,
    easeInOutBounce = function(t)
        return t < 0.5 and (1 - Easing.easeOutBounce(1 - 2 * t)) / 2 or (1 + Easing.easeOutBounce(2 * t - 1)) / 2
    end,
}

-- Skeleton keyframe management
local SkeletonKeyframe = {}
SkeletonKeyframe.__index = SkeletonKeyframe

function SkeletonKeyframe.new(frame_num, skeleton_data)
    local self = setmetatable({}, SkeletonKeyframe)
    self.frame_num = frame_num
    self.skeleton_data = skeleton_data  -- Table of {bone_name: {x, y, rotate}}
    return self
end

-- Bone interpolation
local function interpolate_bone(bone1, bone2, t)
    if not bone1 or not bone2 then return nil end

    return {
        x = math.floor(bone1.x + (bone2.x - bone1.x) * t),
        y = math.floor(bone1.y + (bone2.y - bone1.y) * t),
        rotate = bone1.rotate + (bone2.rotate - bone1.rotate) * t
    }
end

-- Main tween controller
local TweenController = {}
TweenController.__index = TweenController

function TweenController.new()
    local self = setmetatable({}, TweenController)
    self.keyframes = {}
    self.easing = "easeInOutQuad"
    self.total_frames = 0
    self.sprite = nil
    self.layer = nil
    return self
end

function TweenController:add_keyframe(frame_num, skeleton_data)
    table.insert(self.keyframes, SkeletonKeyframe.new(frame_num, skeleton_data))
    table.sort(self.keyframes, function(a, b) return a.frame_num < b.frame_num end)
end

function TweenController:get_interpolated_frame(frame_num)
    -- Find surrounding keyframes
    local kf_before = nil
    local kf_after = nil

    for _, kf in ipairs(self.keyframes) do
        if kf.frame_num <= frame_num then
            kf_before = kf
        end
        if kf.frame_num >= frame_num and not kf_after then
            kf_after = kf
        end
    end

    if not kf_before or not kf_after then
        return kf_before and kf_before.skeleton_data or nil
    end

    if kf_before.frame_num == kf_after.frame_num then
        return kf_before.skeleton_data
    end

    -- Interpolate between keyframes
    local t = (frame_num - kf_before.frame_num) / (kf_after.frame_num - kf_before.frame_num)
    local easing_func = Easing[self.easing] or Easing.easeInOutQuad
    local eased_t = easing_func(t)

    local interpolated = {}
    for bone_name, bone1 in pairs(kf_before.skeleton_data) do
        local bone2 = kf_after.skeleton_data[bone_name]
        interpolated[bone_name] = interpolate_bone(bone1, bone2, eased_t)
    end

    return interpolated
end

function TweenController:generate_tween_frames(start_frame, end_frame, start_keyframe, end_keyframe)
    if not self.sprite or not self.layer then
        app.alert("No sprite or layer selected")
        return false
    end

    local frame_count = end_frame - start_frame + 1

    for i = 0, frame_count - 1 do
        local frame = start_frame + i
        local interpolated = self:get_interpolated_frame(frame)

        if interpolated then
            -- Create cel if needed
            local cel = self.layer:cel(frame)
            if not cel then
                local empty_image = Image(self.sprite.width, self.sprite.height, ColorMode.RGBA)
                cel = self.sprite:newCel(self.layer, frame, empty_image)
            end

            -- Update position based on interpolated skeleton
            -- This is placeholder - actual implementation depends on how bones map to sprites
            app.refresh()
        end
    end

    return true
end

function TweenController:generate_all_tweens()
    if #self.keyframes < 2 then
        app.alert("Need at least 2 keyframes to tween")
        return false
    end

    for i = 1, #self.keyframes - 1 do
        local kf1 = self.keyframes[i]
        local kf2 = self.keyframes[i + 1]
        self:generate_tween_frames(kf1.frame_num, kf2.frame_num, kf1, kf2)
    end

    return true
end

-- UI Functions
local function load_skeleton_json(filepath)
    local file = io.open(filepath, "r")
    if not file then
        app.alert("Cannot open file: " .. filepath)
        return nil
    end

    local content = file:read("*a")
    file:close()

    return json.decode(content)
end

local function read_json_file(path)
    local file = io.open(path, "r")
    if not file then
        app.alert("Cannot open file: " .. path)
        return nil
    end
    local content = file:read("*a")
    file:close()

    if not content or content == "" then
        app.alert("File is empty: " .. path)
        return nil
    end

    local success, result = pcall(function() return json.decode(content) end)
    if not success then
        app.alert("Invalid JSON format in: " .. path .. "\nError: " .. tostring(result))
        return nil
    end

    return result
end

local function skeleton_to_bone_data(skeleton_json)
    local bones = {}

    local function extract_bones(node)
        if node then
            bones[node.name] = {
                x = node.x,
                y = node.y,
                rotate = node.rotate or 0
            }

            if node.children then
                for _, child in ipairs(node.children) do
                    extract_bones(child)
                end
            end
        end
    end

    extract_bones(skeleton_json)
    return bones
end

local function open_skeleton_dialog(tween_controller)
    local sk_dlg = Dialog("Load Skeleton Keyframe")

    sk_dlg:file{
        id = "skeleton_file",
        label = "Skeleton JSON:",
        title = "Select Skeleton File",
        open = true,
        filetypes = {"json"}
    }

    sk_dlg:slider{
        id = "keyframe_num",
        label = "Keyframe:",
        min = 1,
        max = 200,
        value = 1
    }

    sk_dlg:button{
        id = "load",
        text = "Load as Keyframe",
        onclick = function()
            local filepath = sk_dlg.data.skeleton_file
            if filepath == "" then
                app.alert("Please select a skeleton JSON file")
                return
            end

            local skeleton = read_json_file(filepath)
            if skeleton then
                local bone_data = skeleton_to_bone_data(skeleton)
                tween_controller:add_keyframe(sk_dlg.data.keyframe_num, bone_data)
                app.alert("Keyframe " .. sk_dlg.data.keyframe_num .. " loaded")
                sk_dlg:close()
            else
                app.alert("Failed to load skeleton JSON")
            end
        end
    }

    sk_dlg:button{ text = "Cancel", onclick = function() sk_dlg:close() end }
    sk_dlg:show{ wait = false }
end

local function create_main_dialog(tween_controller)
    if dlg then dlg:close() end

    dlg = Dialog("TweenCel Advanced - KIRBY_CELESTE")

    dlg:label{ text = "Easing Function" }
    local easing_options = {}
    for name, _ in pairs(Easing) do
        table.insert(easing_options, name)
    end
    table.sort(easing_options)

    dlg:combobox{
        id = "easing",
        options = easing_options,
        option = "easeInOutQuad",
        onchange = function()
            tween_controller.easing = dlg.data.easing
        end
    }

    dlg:separator()

    dlg:label{ text = "Keyframes: " .. #tween_controller.keyframes }

    dlg:button{
        id = "load_keyframe",
        text = "Load Skeleton as Keyframe",
        onclick = function()
            open_skeleton_dialog(tween_controller)
            dlg:modify{ label = "Keyframes: " .. #tween_controller.keyframes }
        end
    }

    dlg:separator()

    dlg:slider{
        id = "start_frame",
        label = "Tween Start:",
        min = 1,
        max = 200,
        value = 1
    }

    dlg:slider{
        id = "end_frame",
        label = "Tween End:",
        min = 1,
        max = 200,
        value = 50
    }

    dlg:button{
        id = "generate",
        text = "Generate Tween Frames",
        onclick = function()
            local sprite = app.activeSprite
            if not sprite then
                app.alert("No active sprite")
                return
            end

            tween_controller.sprite = sprite
            tween_controller.layer = app.activeLayer

            if tween_controller:generate_all_tweens() then
                app.alert("Tween generated successfully!")
                app.refresh()
            end
        end
    }

    dlg:separator()

    dlg:label{ text = "Preview" }

    dlg:button{
        id = "play",
        text = "Play Preview",
        onclick = function()
            -- Simple frame preview
            local sprite = app.activeSprite
            if sprite then
                app.editor.frameNumber = dlg.data.start_frame
                is_playing = true
            end
        end
    }

    dlg:button{
        id = "stop",
        text = "Stop Preview",
        onclick = function()
            is_playing = false
        end
    }

    dlg:separator()

    dlg:button{ id = "close", text = "Close", onclick = function() dlg:close() end }

    dlg:show{ wait = false }
end

-- Main initialization
local tween_controller = TweenController.new()

-- Quick reference
print("=== TweenCel Advanced Loaded ===")
print("Easing Functions Available:")
for name, _ in pairs(Easing) do
    print("  - " .. name)
end

create_main_dialog(tween_controller)

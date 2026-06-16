-- TweenCel Simple - Skeleton Animation Tweening (No JSON Required)
-- For KIRBY_CELESTE Mod - Works directly in Aseprite

local dlg
local current_sprite = nil
local current_layer = nil

-- Easing functions
local Easing = {
    linear = function(t) return t end,
    easeInQuad = function(t) return t * t end,
    easeOutQuad = function(t) return 1 - (1 - t) * (1 - t) end,
    easeInOutQuad = function(t) return t < 0.5 and 2 * t * t or -1 + (4 - 2 * t) * t end,
    easeInCubic = function(t) return t * t * t end,
    easeOutCubic = function(t) return 1 + (t - 1) ^ 3 end,
    easeInOutCubic = function(t) return t < 0.5 and 4 * t * t * t or 1 + (t - 1) * (2 * (t - 2)) * (2 * (t - 2)) end,
    easeInSine = function(t) return 1 - math.cos((t * math.pi) / 2) end,
    easeOutSine = function(t) return math.sin((t * math.pi) / 2) end,
    easeInOutSine = function(t) return -(math.cos(math.pi * t) - 1) / 2 end,
    easeOutBounce = function(t)
        local n1, d1 = 7.5625, 2.75
        if t < 1 / d1 then return n1 * t * t
        elseif t < 2 / d1 then return n1 * (t - 1.5 / d1) * (t - 1.5 / d1) + 0.75
        elseif t < 2.5 / d1 then return n1 * (t - 2.25 / d1) * (t - 2.25 / d1) + 0.9375
        else return n1 * (t - 2.625 / d1) * (t - 2.625 / d1) + 0.984375 end
    end,
    easeOutBack = function(t)
        local c1, c3 = 1.70158, 1.70158 + 1
        return 1 + c3 * (t - 1) ^ 3 + c1 * (t - 1) ^ 2
    end,
    easeOutElastic = function(t)
        if t == 0 or t == 1 then return t end
        local c4 = (2 * math.pi) / 3
        return 2 ^ (-10 * t) * math.sin((t * 10 - 0.75) * c4) + 1
    end,
}

local function create_dialog()
    if dlg then dlg:close() end

    dlg = Dialog("TweenCel Simple - KIRBY_CELESTE")

    dlg:label{ text = "Frame Range" }
    dlg:slider{
        id = "start_frame",
        label = "Start:",
        min = 1,
        max = 200,
        value = 1
    }
    dlg:slider{
        id = "end_frame",
        label = "End:",
        min = 1,
        max = 200,
        value = 50
    }

    dlg:separator()
    dlg:label{ text = "Easing Function" }

    local easing_list = {}
    for name, _ in pairs(Easing) do
        table.insert(easing_list, name)
    end
    table.sort(easing_list)

    dlg:combobox{
        id = "easing",
        options = easing_list,
        option = "easeInOutQuad"
    }

    dlg:separator()

    dlg:button{
        id = "generate",
        text = "Generate Tween Frames",
        onclick = function()
            local sprite = app.activeSprite
            if not sprite then
                app.alert("Please open a sprite file first")
                return
            end

            local layer = app.activeLayer
            if not layer then
                app.alert("Please select a layer")
                return
            end

            local start_frame = dlg.data.start_frame
            local end_frame = dlg.data.end_frame
            local easing_name = dlg.data.easing
            local easing_func = Easing[easing_name] or Easing.easeInOutQuad

            if start_frame >= end_frame then
                app.alert("Start frame must be less than end frame")
                return
            end

            -- Get first cel as source
            local first_cel = layer:cel(start_frame)
            if not first_cel then
                app.alert("Start frame must have a cel/image")
                return
            end

            local total_frames = end_frame - start_frame + 1

            -- Generate frames
            app.transaction(function()
                for i = start_frame, end_frame do
                    local cel = layer:cel(i)
                    if not cel then
                        -- Create empty cel
                        local empty_image = Image(sprite.width, sprite.height, ColorMode.RGBA)
                        sprite:newCel(layer, i, empty_image)
                    end
                end
            end)

            app.alert("Generated " .. total_frames .. " frames with " .. easing_name)
            app.refresh()
        end
    }

    dlg:separator()

    dlg:label{ text = "Quick Tips" }
    dlg:label{ text = "1. Set frame range" }
    dlg:label{ text = "2. Pick easing" }
    dlg:label{ text = "3. Click Generate" }

    dlg:separator()

    dlg:button{ id = "close", text = "Close", onclick = function() dlg:close() end }

    dlg:show{ wait = false }
end

create_dialog()

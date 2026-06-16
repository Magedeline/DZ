-- Modified Skeleton2Animation.lua for KIRBY_CELESTE
-- Optimized for Sans NPC (32x32), Papyrus NPC (32x32), and Sans Boss (64x64)

local dlg
local cmdDlg
local max_child = 8
local radius = 2
local row_space = 6
local colum_space = 8
local max_depth = 4
local bone_sprite = nil
local bone_layer = nil
local sk_layer_name = "BoneTree"
local frame_index = 1
local state_Add_bone_skin = "Add_Bone_Skin"
local state_pose = "SetPose"
local state_offset = "OffsetSkin"
local state_rotate = "RotateSkin"
local cur_state = state_Add_bone_skin
local icon_size = 12

-- Predefined sizes optimized for character sprites
local sizes = {
  { label = "Sans NPC (32x32)", width = 32, height = 32, preset = "sans_npc" },
  { label = "Papyrus NPC (32x32)", width = 32, height = 32, preset = "papyrus_npc" },
  { label = "Sans Boss (64x64)", width = 64, height = 64, preset = "sans_boss" },
  { label = "Custom 64x64", width = 64, height = 64, preset = "custom" },
  { label = "Custom 128x128", width = 128, height = 128, preset = "custom" },
}
local selected_size = sizes[1]

-- Bone visualization pixels
local bone_pixels =  {
  {0,0,0,0,0,0,0,0,0,0,0,0},
  {0,0,0,0,0,0,0,1,1,0,0,0},
  {0,0,0,0,0,0,0,1,1,1,0,0},
  {0,0,0,0,0,0,0,0,1,1,1,1},
  {0,0,0,0,0,0,0,1,1,0,1,1},
  {0,0,0,0,0,0,1,1,0,0,0,0},
  {0,0,0,0,0,1,1,0,0,0,0,0},
  {0,0,1,0,1,1,0,0,0,0,0,0},
  {0,1,1,1,1,0,0,0,0,0,0,0},
  {0,0,0,1,1,0,0,0,0,0,0,0},
  {0,0,0,0,1,1,0,0,0,0,0,0},
  {0,0,0,0,1,0,0,0,0,0,0,0},
}

local skeleton_tree = {name="root",x=16,y=16,bx=16,by=16,bcx=0,bcy=0,children={},index = 1,parent=nil,depth=1,image=nil,rotate = 0,offset_x=0,offset_y=0}
local node_positions = {}
local selected_node = skeleton_tree
local selected_layer_image = nil
local last_rotate_value = 0
local withSkin = false
local node_radius = 10
local custom_icon = nil
local dragging_index = nil
local target_point = nil

-- Skeleton preset configurations
local function create_sans_npc_skeleton()
  skeleton_tree = {name="root",x=16,y=16,bx=16,by=16,bcx=0,bcy=0,children={},index = 1,parent=nil,depth=1,image=nil,rotate = 0,offset_x=0,offset_y=0}
  local head = add_skeleton_node(skeleton_tree, "head")
  local body = add_skeleton_node(skeleton_tree, "body")
  local l_arm = add_skeleton_node(skeleton_tree, "l_arm")
  local r_arm = add_skeleton_node(skeleton_tree, "r_arm")
  local l_leg = add_skeleton_node(skeleton_tree, "l_leg")
  local r_leg = add_skeleton_node(skeleton_tree, "r_leg")
  return skeleton_tree
end

local function create_papyrus_npc_skeleton()
  skeleton_tree = {name="root",x=16,y=16,bx=16,by=16,bcx=0,bcy=0,children={},index = 1,parent=nil,depth=1,image=nil,rotate = 0,offset_x=0,offset_y=0}
  local head = add_skeleton_node(skeleton_tree, "head")
  local body = add_skeleton_node(skeleton_tree, "body")
  local l_arm = add_skeleton_node(skeleton_tree, "l_arm")
  local r_arm = add_skeleton_node(skeleton_tree, "r_arm")
  local l_leg = add_skeleton_node(skeleton_tree, "l_leg")
  local r_leg = add_skeleton_node(skeleton_tree, "r_leg")
  return skeleton_tree
end

local function create_sans_boss_skeleton()
  skeleton_tree = {name="root",x=32,y=32,bx=32,by=32,bcx=0,bcy=0,children={},index = 1,parent=nil,depth=1,image=nil,rotate = 0,offset_x=0,offset_y=0}
  local head = add_skeleton_node(skeleton_tree, "head")
  local body = add_skeleton_node(skeleton_tree, "body")
  local l_arm = add_skeleton_node(skeleton_tree, "l_arm")
  local r_arm = add_skeleton_node(skeleton_tree, "r_arm")
  local l_leg = add_skeleton_node(skeleton_tree, "l_leg")
  local r_leg = add_skeleton_node(skeleton_tree, "r_leg")
  return skeleton_tree
end

local function indent(level)
	return string.rep("  ",level)
end

local function read_json_file(path)
  local file = io.open(path, "r")
  if not file then
    return nil
  end
  local content = file:read("*a")
  file:close()
  return json.decode(content)
end

local function create_skeleton_sprite()
    if bone_sprite ~= nil then
	   bone_sprite:close()
	   bone_sprite = nil
	   bone_layer = nil
	end
	bone_sprite = Sprite(selected_size.width, selected_size.height, ColorMode.RGBA)
	bone_sprite.transparentColor = Color(0,0,0,0)
	bone_sprite.filename = "skeleton_sprite"
	bone_layer = bone_sprite.layers[1]
	bone_layer.name = sk_layer_name
end

local function moveSkLayer2Top()
	if bone_sprite == nil then
	   app.alert("Not found skeleton sprite")
	   return
	end
	if bone_layer then
		local newSkLayer = bone_sprite:newLayer()
		for _,cel in ipairs(bone_layer.cels) do
			local newImage = cel.image:clone()
			bone_sprite:newCel(newSkLayer,cel.frameNumber,newImage,cel.position)
		end
		bone_sprite:deleteLayer(bone_layer)
		bone_layer = newSkLayer
		bone_layer.name = sk_layer_name
	end
end

function add_skeleton_node(parent, name)
    local x = math.min(selected_size.width - 2, parent.x + row_space)
	local y = math.min(selected_size.height - 2, parent.y + colum_space * parent.index)
	local depth = parent.depth + 1
    local node = { name = name, x=x, y=y, bx=x, by=y, bcx=0, bcy=0, children = {}, index=1, parent=parent, depth=depth, image=nil, rotate = 0, offset_x = 0, offset_y = 0}
    table.insert(parent.children, node)
	parent.index = parent.index + 1
    return node
end

function drawCircle(lay, cel, cx, cy, r, color)
	local img = cel.image
  for y = -r, r do
    for x = -r, r do
      if x*x + y*y <= r*r then
        local px, py = cx + x, cy + y
        if px >= 0 and py >= 0 and px < img.width and py < img.height then
          img:putPixel(px, py, color)
        end
      end
    end
  end
end

function drawLine(layer, cel, x0, y0, x1, y1, color)
	local img = cel.image
  local dx = math.abs(x1 - x0)
  local dy = math.abs(y1 - y0)
  local sx = (x0 < x1) and 1 or -1
  local sy = (y0 < y1) and 1 or -1
  local err = dx - dy

  while true do
    if x0 >= 0 and y0 >= 0 and x0 < img.width and y0 < img.height then
      img:putPixel(x0, y0, color)
    end
    if x0 == x1 and y0 == y1 then break end
    local e2 = 2 * err
    if e2 > -dy then err = err - dy; x0 = x0 + sx end
    if e2 < dx  then err = err + dx; y0 = y0 + sy end
  end
end

function drawNodeTree(node)
  local color = Color { r = 255, g = 255, b = 255, a = 255 }
  if bone_layer == nil then return end
  local cel = bone_layer:cel(1)
  drawCircle(bone_layer, cel, node.x, node.y, radius, color)
  if node.parent ~= nil then
	drawLine(bone_layer, cel, node.x, node.y, node.parent.x, node.parent.y, Color(255, 0, 0))
  end
  for i, child in ipairs(node.children) do
      drawNodeTree(child)
  end
end

function node_to_json_pretty(node, level)
  level = level or 0
  local lines = {}
  table.insert(lines, indent(level) .. "{")
  if level == 0 then
	  table.insert(lines, indent(level + 1) .. '"sprite_width": '..selected_size.width..',')
	  table.insert(lines, indent(level + 1) .. '"sprite_height": '..selected_size.height..',')
  end
  table.insert(lines, indent(level + 1) .. '"name": "'.. node.name..'",')
  table.insert(lines, indent(level + 1) .. '"x": '..node.x..',')
  table.insert(lines, indent(level + 1) .. '"y": '..node.y..',')
  table.insert(lines, indent(level + 1) .. '"bx": '.. node.bx..',')
  table.insert(lines, indent(level + 1) .. '"by": '..node.by..',')
  table.insert(lines, indent(level + 1) .. '"bcx": '..node.bcx..',')
  table.insert(lines, indent(level + 1) .. '"bcy": '..node.bcy..',')
  table.insert(lines, indent(level + 1) .. '"index": '..node.index..',')
  table.insert(lines, indent(level + 1) .. '"depth": '..node.depth..',')
  table.insert(lines, indent(level + 1) .. '"rotate": '.. node.rotate..',')
  table.insert(lines, indent(level + 1) .. '"offset_x": '..node.offset_x..',')
  table.insert(lines, indent(level + 1) .. '"offset_y": '..node.offset_y..',')
  table.insert(lines, indent(level + 1) .. '"children": [')

  for i, child in ipairs(node.children) do
    table.insert(lines, node_to_json_pretty(child, level + 2))
    if i < #node.children then
      lines[#lines] = lines[#lines] .. ","
    end
  end

  table.insert(lines, indent(level + 1) .. "]")
  table.insert(lines, indent(level) .. "}")
  return table.concat(lines, "\n")
end

local function process_node(node, sk_node)
  if node.children then
    for _, child in ipairs(node.children) do
       local depth = sk_node.depth + 1
       local newSknode = { name=child.name, x=child.x, y=child.y, bx=child.bx, by=child.by, bcx=0, bcy=0, children = {}, index=1, parent=sk_node, depth=depth, image=nil, rotate = 0, offset_x = 0, offset_y = 0}
       table.insert(sk_node.children, newSknode)
	   sk_node.index = sk_node.index + 1
	   process_node(child, newSknode)
	end
  end
end

local function open_file_dialog()
  local open_file_dlg = Dialog("Open File")
  open_file_dlg:file{
    id = "open_path",
    label = "Open",
    title = "Open pose file",
	filename = "load_pose_file",
    open = true,
    filetypes = {"json"},
  }
  open_file_dlg:button {
        id = "ok",
        text = "OK",
        onclick = function()
            local filepath = open_file_dlg.data.open_path
            if filepath == "" then
                app.alert("Please select a filename")
                return
            end
            if not filepath:match("%.json$") then
                app.alert("pose file should be json type")
				return
            end
            local root = read_json_file(filepath)
			if root then
				skeleton_tree = {name=root.name, x=root.x, y=root.y, bx=root.bx, by=root.by, bcx=0, bcy=0, children={}, index = 1, parent=nil, depth=1, image=nil, rotate = 0, offset_x=0, offset_y=0}
				process_node(root, skeleton_tree)
				dlg:modify{id = "skeleton_canvas"}
				dlg:repaint()
				bone_sprite = nil
				bone_layer = nil
				selected_size.width = math.floor(root.sprite_width)
				selected_size.height = math.floor(root.sprite_height)
				dlg:modify{id ="sprite_size", text = selected_size.width.."x"..selected_size.height}
				create_skeleton_sprite()
				local image = bone_layer:cel(1).image
				image:clear()
				drawNodeTree(skeleton_tree)
				app.refresh()
				open_file_dlg:close()
			else
				app.alert("Failed to read pose data - check JSON file format")
			end
		end
		}
  open_file_dlg:button{ text = "Cancel", onclick = function() open_file_dlg:close() end }
  open_file_dlg:show{ wait = false }
end

local function save_file_dialog()
  local save_file_dlg = Dialog("Save File")
  save_file_dlg:file{
    id = "save_file",
    label = "Save As",
    title = "Save pose As",
	filename = "save_pose_data.json",
    save = true,
    filetypes = {"json"},
  }
  save_file_dlg:button {
        id = "ok",
        text = "OK",
        onclick = function()
            local filepath = save_file_dlg.data.save_file
            if filepath == "" then
                app.alert("Please select a filename")
                return
            end
            if not filepath:match("%.json$") then
                filepath = filepath .. ".json"
            end
			local json_str = node_to_json_pretty(skeleton_tree)
            local file = io.open(filepath, "w")
            if file then
                file:write(json_str)
                file:close()
                app.alert("pose data save to: " .. filepath)
                save_file_dlg:close()
            else
                app.alert("Failed to save skeleton data")
            end
		end
		}
  save_file_dlg:button{ text = "Cancel", onclick = function() save_file_dlg:close() end }
  save_file_dlg:show{ wait = false }
end

function createDialog()
	if dlg then
		dlg:close()
	end

	dlg = Dialog("Skeleton Editor - KIRBY_CELESTE")
	dlg:label { id = "state", label = "state:", text = state_Add_bone_skin }
	dlg:label { id = "sprite_size", text = selected_size.label }
	dlg:label { id = "point", text = "None" }
	dlg:entry{id="BoneName",label="Bone name"}
	dlg:button{id="label", text=" + Add", onclick=function()
		local boneName = dlg.data.BoneName
		if boneName == "" then
			app.alert("Enter bone name")
			return
		end
		if selected_node == nil then
			app.alert("Select parent bone")
			return
		end
		local newnode = add_skeleton_node(selected_node, boneName)
		dlg:modify{id = "skeleton_canvas"}
		dlg:repaint()
		local image = bone_layer:cel(1).image
		image:clear()
		drawNodeTree(skeleton_tree)
		app.refresh()
	end}

	dlg:separator()
	dlg:canvas{
		id = "skeleton_canvas",
		width = 300,
		height = 400,
		autoScaling = true,
		onpaint = function(ev)
			node_positions = {}
			draw_skeleton(ev, skeleton_tree, 1, 1, 0)
		end,
		onmousedown = function(ev)
			local click_x, click_y = ev.x, ev.y
			for _, entry in ipairs(node_positions) do
				if click_x >= entry.x and click_x <= (entry.x + entry.width) and
					click_y >= entry.y and click_y <= (entry.y + entry.height) then
					selected_node = entry.node
					dlg:repaint()
					return
				end
			end
			selected_node = nil
		end
	}

	dlg:separator()
	dlg:button{id="load", text="Load JSON", onclick=function()
		open_file_dialog()
	end}
	dlg:button{id="Save", text="Save JSON", onclick=function()
		save_file_dialog()
	end}
	dlg:button{id="close", text="Close", onclick=function() dlg:close() end}

	dlg:show{wait = false}
end

function draw_skeleton(ev, node, x, y, depth)
    local spacing = 3
    local indent = depth * 10

    table.insert(node_positions, {
        node = node,
        x = x + indent,
        y = y,
        width = icon_size + spacing + 50,
        height = icon_size
    })

    ev.context:fillText(node.name, x + indent + spacing, y)
	local textSize = ev.context:measureText(node.name)
	if node == selected_node then
		local origialColor = ev.context.color
		ev.context.color = Color(50, 200, 50)
		ev.context:strokeRect(x+indent+spacing-2, y-2, textSize.width+4, textSize.height+2)
		ev.context.color = origialColor
	end

    local new_y = y + icon_size + spacing
    for _, child in ipairs(node.children) do
        new_y = draw_skeleton(ev, child, x, new_y, depth + 1)
    end

    return new_y
end

-- Main initialization
local select_size_dlg = Dialog { title = "Select Character Type" }

local select_size_labels = {}
for i, item in ipairs(sizes) do
  table.insert(select_size_labels, item.label)
end

select_size_dlg:combobox{
  id = "size_choice",
  options = select_size_labels,
  option = select_size_labels[1]
}

select_size_dlg:button { id = "ok", text = "OK" }
select_size_dlg:button { id = "cancel", text = "Cancel" }
select_size_dlg:show()

local select_size_data = select_size_dlg.data
if not select_size_data.ok then
  return
end

for _, item in ipairs(sizes) do
  if item.label == select_size_data.size_choice then
    selected_size = item
    break
  end
end

custom_icon = Image(12, 12)
custom_icon:clear()
local white = Color{r=255, g=255, b=255, a=255}
for y=1, icon_size do
  for x=1, icon_size do
    if bone_pixels[y][x] == 1 then
      custom_icon:drawPixel(x-1, y-1, white)
    end
  end
end

-- Load preset skeleton
if selected_size.preset == "sans_npc" then
   create_sans_npc_skeleton()
elseif selected_size.preset == "papyrus_npc" then
   create_papyrus_npc_skeleton()
elseif selected_size.preset == "sans_boss" then
   create_sans_boss_skeleton()
end

selected_node = skeleton_tree
create_skeleton_sprite()
createDialog()
local image = bone_layer:cel(1).image
image:clear()
drawNodeTree(skeleton_tree)
app.refresh()

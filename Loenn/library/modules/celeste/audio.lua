---@meta Celeste.Audio

---@class c__Audio
---@field CurrentMusic string
local audio = {}

---Play the given audio event at the given global position.
---@param event string The event name to play.
---@param position? Vector2 Where to play the audio.
---@return EventInstance # The Audio EventInstance
function audio.Play(event, position) end

---@param instance EventInstance
---@param allowFadeOut? bool
function audio.Stop(instance, allowFadeOut) end

return audio

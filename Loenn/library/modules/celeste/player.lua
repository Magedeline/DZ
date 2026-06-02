---@meta Celeste.Player

---@class c__Player
local c_Player = {}

--#region IntroTypes

---@class IntroTypes : Enum

---@type CSEnum<IntroTypes>
c_Player.IntroTypes = {
  Transition = {0},
  Respawn = {1},
  WalkInRight = {2},
  WalkInLeft = {3},
  Jump = {4},
  WakeUp = {5},
  Fall = {6},
  TempleMirrorVoid = {7},
  None = {8},
  ThinkForABit = {9}
}

--#endregion

---@class Player : Actor
---@field IntroType IntroTypes
---@field Facing Facings
---@field StateMachine StateMachine
---@field ForceCameraUpdate boolean
---@field Dead boolean
---@field DummyFriction boolean
---@field DummyAutoAnimate boolean
---@field Dashes int
---@field DashDir Vector2
---@field DashAttacking bool
---@field AutoJump boolean
---@field AutoJumpTimer float
---@field Speed Vector2
---@field Sprite Sprite
local player = {}

--#region Player States

---@class c__Player
---@field StNormal 0
---@field StClimb 1
---@field StDash 2
---@field StSwim 3
---@field StBoost 4
---@field StRedDash 5
---@field StHitSquash 6
---@field StLaunch 7
---@field StPickup 8
---@field StDreamDash 9
---@field StSummitLaunch 10
---@field StDummy 11
---@field StIntroWalk 12
---@field StIntroJump 13
---@field StIntroRespawn 14
---@field StIntroWakeUp 15
---@field StBirdDashTutorial 16
---@field StFrozen 17
---@field StReflectionFall 18
---@field StStarFly 19
---@field StTempleFall 20
---@field StCassetteFly 21
---@field StAttract 22
---@field StIntroMoonJump 23
---@field StFlingBird 24
---@field StIntroThinkForABit 25

--#endregion

---Make the player Die
---@param dir Vector2
---@param evenIfInvincible? boolean
---@param registerDeathInStats? boolean
---@return userdata deadBody
function player:Die(dir, evenIfInvincible, registerDeathInStats) end

---Check if the Player is touching ground.
---@param at? float Vertical offset to check at.
---@return boolean
function player:OnGround(at) end

---Make the player walk in Dummy state.
---@param x float The target x.
---@param walkBackwards? boolean
---@param speedMultiplier? float
---@param keepWalkingIntoWalls? boolean
---@return IEnumerator
function player:DummyWalkTo(x, walkBackwards, speedMultiplier, keepWalkingIntoWalls) end

---Make the player run in Dummy state.
---@param x float The target x.
---@param fastAnimation? boolean
---@return IEnumerator
function player:DummyRunTo(x, fastAnimation) end

---Make the player jump.
---@param particles? boolean Whether to display dust particles.
---@param playSfx? boolean Whether to play an SFX.
function player:Jump(particles, playSfx) end

---Start the Player's StartFly state
function player:StartStarFly() end

---Start the Player's CassetteFly state.
---@param target Vector2 The target end position.
---@param control Vector2 The flying arc middle point.
function player:StartCassetteFly(target, control) end

function player:CreateSplitParticles() end

---Play a sound from the Player.
---@param sound string The event to play.
---@param param? string
---@param value? float
---@return EventInstance
function player:Play(sound, param, value) end

function player:RefillStamina() end

return c_Player
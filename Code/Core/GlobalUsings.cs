// Global using directives for KIRBY_CELESTE mod
global using System;
global using System.Collections;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using Celeste;
global using Celeste.Mod;
global using Celeste.Mod.Backdrops;
global using Celeste.Mod.Entities;
global using Celeste.Helpers;
global using Celeste.HotReload;
global using Celeste.Extensions;
global using Celeste.Extensions.Core;
global using Celeste.Utils;
global using Util = Celeste.Mod.KIRBY_CELESTE.Util;
global using CustomSFX = Celeste.Mod.KIRBY_CELESTE.CustomSFX;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Monocle;
global using On;
global using Player = Celeste.Player;
// Type aliases
global using static Celeste.PlayerSpriteModeExtensions;
global using KirbyModeExt = Celeste.Extensions.KirbyMode;
global using KIRBY_CELESTEModule = Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModule;
global using KIRBY_CELESTEModuleSettings = Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModuleSettings;
global using KIRBY_CELESTEModuleSaveData = Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModuleSaveData;
global using KIRBY_CELESTEModuleSession = Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModuleSession;
global using IngesteModule = Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModule;
global using IngesteLogger = Celeste.Helpers.IngesteLogger;
global using IngesteModuleSettings = Celeste.Mod.KIRBY_CELESTE.KIRBY_CELESTEModuleSettings;
global using KIRBY_CELESTESaveDataMigration = Celeste.Helpers.KIRBY_CELESTESaveDataMigration;
global using CelesteGame = Celeste.Celeste;
global using CelesteBridge = Celeste.Bridge;
global using CelesteDashBlock = Celeste.DashBlock;
global using CelesteJumpThru = Celeste.JumpThru;
global using CelesteNPC = Celeste.NPC;
global using CelestePlayer = Celeste.Player;
global using CelestePlayerSprite = Celeste.PlayerSprite;
global using CelesteStarJumpBlock = Celeste.StarJumpBlock;
global using CelesteStrawberry = Celeste.Strawberry;





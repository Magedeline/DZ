using System;
using FMOD;
using FMOD.Studio;
using Celeste.Mod;
using Monocle;

namespace Celeste.Mod.KIRBY_CELESTE.Audio
{
    /// <summary>
    /// Audio definitions for KIRBY_CELESTE mod containing FMOD GUIDs and paths.
    /// Generated from GUIDs.txt mappings for events, buses, VCA, and snapshots.
    /// </summary>
    public static class AudioDefinitions
    {
        // ── Event Definitions ────────────────────────────────────────────────
        
        public static class Events
        {
            // Character SFX - Badeline
            public static readonly class Badeline
            {
                public static readonly Guid Appear = new Guid("{ad25a031-880b-4f88-ac12-82d6c52fbdea}");
                public static readonly Guid BoosterBegin = new Guid("{7a71fc61-b688-4d82-a2b7-781c2434e942}");
                public static readonly Guid BoosterFinal = new Guid("{b2ee45d8-e1b0-43f1-baaa-f6e77d37ddb5}");
                public static readonly Guid BoosterReappear = new Guid("{207a212a-2beb-4022-a8da-11ac987d3097}");
                public static readonly Guid BoosterRelocate = new Guid("{7e3c6b4e-e41a-4e9a-9d24-22737c66593b}");
                public static readonly Guid BoosterThrow = new Guid("{2592d638-f3c5-4f6f-81e0-888a04affa40}");
                public static readonly Guid BossBullet = new Guid("{bbe7d0c8-45f9-4671-b49e-8e38e357bb81}");
                public static readonly Guid BossHug = new Guid("{898fc1ea-250b-4d03-8f37-c0c35799a915}");
                public static readonly Guid BossIdleAir = new Guid("{b4002010-ff11-4311-87c7-f87028220f78}");
                public static readonly Guid BossLaserCharge = new Guid("{e7a2ddd6-091a-44ab-aac7-e57bd13c009c}");
                public static readonly Guid BossLaserFire = new Guid("{58b5d825-ebcf-457b-b493-9be82640b9eb}");
                public static readonly Guid BossPrefightGetup = new Guid("{646b0a01-2101-424f-804b-18842f72a62d}");
                public static readonly Guid ClimbLedge = new Guid("{33df1e8a-7605-4b2d-8235-ee78fbaa8c55}");
                public static readonly Guid DashRedLeft = new Guid("{b54ae334-409b-4bba-82e2-62753f764f90}");
                public static readonly Guid DashRedRight = new Guid("{50356124-434a-4dca-aeb1-52462846fb8a}");
                public static readonly Guid Disappear = new Guid("{16b40879-0a79-4e42-8c91-fe419a8e186c}");
                public static readonly Guid DreamblockEnter = new Guid("{937c3941-eeb8-4f1f-8451-765b33545202}");
                public static readonly Guid DreamblockExit = new Guid("{3b142d62-975a-41a3-9b59-8e5ba4f6cbdb}");
                public static readonly Guid DreamblockTravel = new Guid("{697102c2-9978-42b7-a3b0-0c2b99c9032e}");
                public static readonly Guid Duck = new Guid("{d879f9dd-98d0-479e-9f08-a1848f4a0f5c}");
                public static readonly Guid Footstep = new Guid("{ff6442f1-66c2-4dba-8d57-038dda8fd296}");
                public static readonly Guid Grab = new Guid("{8824489f-1d0b-4670-beb2-08d67cfd3c3b}");
                public static readonly Guid GrabLetgo = new Guid("{c122d904-fb7a-4bfe-99a1-f5a4f8701d17}");
                public static readonly Guid Handhold = new Guid("{c2cd2f17-045c-4312-b7f6-8e7b750577c0}");
                public static readonly Guid Jump = new Guid("{95ce0255-9a27-47b4-a902-fb25c0f2a2e2}");
                public static readonly Guid JumpAssisted = new Guid("{64424649-d82c-4efc-90a9-f54e8411d9b7}");
                public static readonly Guid JumpClimbLeft = new Guid("{58e689a2-1717-434f-9fa7-dffcd72ab27e}");
                public static readonly Guid JumpClimbRight = new Guid("{d9a77828-0390-434a-8cbc-e3df56a40cc4}");
                public static readonly Guid JumpDreamblock = new Guid("{6faecb61-f82e-400e-9785-4d407a82c838}");
                public static readonly Guid JumpSpecial = new Guid("{bc8a6051-399a-4f76-a865-02b53c5fe8ab}");
                public static readonly Guid JumpSuper = new Guid("{73a21bdf-62c2-46d8-ae04-68e6c0c26b7f}");
                public static readonly Guid JumpSuperslide = new Guid("{7ca46073-e6b8-4f74-b9a8-ac54a8966c5b}");
                public static readonly Guid JumpSuperwall = new Guid("{2336b17f-5769-4e2e-a636-aee1ddef9086}");
                public static readonly Guid JumpWallLeft = new Guid("{ab3be2a7-36d0-43cb-990a-2c49ba5d5c60}");
                public static readonly Guid JumpWallRight = new Guid("{3e5b18db-8f99-4ab7-ad08-8529fd257b6b}");
                public static readonly Guid Landing = new Guid("{8f924592-8b14-40d6-81af-d42fab0b6da1}");
                public static readonly Guid LevelEntry = new Guid("{25294859-6bfd-434e-9528-8433245fe3b5}");
                public static readonly Guid MaddyJoin = new Guid("{38557af4-adf9-4328-9f2c-167b12ff9f8e}");
                public static readonly Guid MaddySplit = new Guid("{450fb5b3-e9e3-45d8-9f34-ba05e292958f}");
                public static readonly Guid Stand = new Guid("{1a114663-4b93-4aab-ba8c-ca8793f2831e}");
                public static readonly Guid TempleMoveChats = new Guid("{97d0d2e4-92f5-48cf-948f-4fc85b0a0791}");
                public static readonly Guid TempleMoveFirst = new Guid("{a5ceb7fd-8a03-4c79-8d4f-81ad37400f43}");
                public static readonly Guid Wallslide = new Guid("{a2443155-5af1-4e19-80d5-a81a3d9cf06d}");
            }

            // Character SFX - Madeline
            public static readonly class Madeline
            {
                public static readonly Guid BackpackDrop = new Guid("{75ab6aef-3f69-4776-b60f-5700451cc9a2}");
                public static readonly Guid CampfireSit = new Guid("{99373af6-beba-488d-b516-ff48fd95e8b8}");
                public static readonly Guid CampfireStand = new Guid("{9435a742-7595-48a4-b2af-2757c4f927da}");
                public static readonly Guid ClimbLedge = new Guid("{9457c17c-688b-4701-b2ae-b89d821d54f7}");
                public static readonly Guid CoreHairCharged = new Guid("{82f8448b-9452-4b12-838c-dcd81098476e}");
                public static readonly Guid CrystaltheoLift = new Guid("{315ac151-5eb6-4e6b-b1b7-0531f3572c44}");
                public static readonly Guid CrystaltheoThrow = new Guid("{eb1249df-06cf-434d-991b-0b73464eef06}");
                public static readonly Guid DashPinkLeft = new Guid("{cb220baa-d2cb-4a87-9907-a6595dba6735}");
                public static readonly Guid DashPinkRight = new Guid("{8ec31009-771f-4473-a3fc-45b209d6aa87}");
                public static readonly Guid DashRedLeft = new Guid("{b9d3a7c5-3d49-4b8a-aad1-fcfaf87293af}");
                public static readonly Guid DashRedRight = new Guid("{066cd550-a394-4bed-a4cf-c71905a160ed}");
                public static readonly Guid Death = new Guid("{ae79fcef-d9b4-44c5-91e5-e0348229e9d5}");
                public static readonly Guid DreamblockEnter = new Guid("{06d7cbaf-9d9e-4f67-8adb-37a599dd4c37}");
                public static readonly Guid DreamblockExit = new Guid("{0a9527ad-360b-4cd2-b585-92b066720b34}");
                public static readonly Guid DreamblockTravel = new Guid("{47bc2416-fd95-4a75-8952-7c43757e0e66}");
                public static readonly Guid Duck = new Guid("{0d6cb459-91af-4842-8d2b-0f0e103313ef}");
                public static readonly Guid Footstep = new Guid("{73ada323-e77f-43f7-b0df-a8facdef03b7}");
                public static readonly Guid Grab = new Guid("{8aa6ab7b-3104-4a06-8e7e-b1141ac0d23d}");
                public static readonly Guid GrabLetgo = new Guid("{0e0bb484-9840-47f8-a697-773134e4bedb}");
                public static readonly Guid Handhold = new Guid("{7f8c23d1-bd9d-4a10-b8af-fd4a5cb12244}");
                public static readonly Guid IdleCrackknuckles = new Guid("{421fe5d0-9d4b-40a5-ab8e-23b1ec4bf1b3}");
                public static readonly Guid IdleScratch = new Guid("{852ea39f-c182-4385-9aa3-1d95cb02040b}");
                public static readonly Guid IdleSneeze = new Guid("{ae4aea88-f499-49e8-9536-78c4bad21743}");
                public static readonly Guid Jump = new Guid("{eeede5f5-3691-4cd9-8b2c-91e02d3d41ed}");
                public static readonly Guid JumpAssisted = new Guid("{73ba5692-b6bd-4c78-a392-e41fa7425ead}");
                public static readonly Guid JumpClimbLeft = new Guid("{faac7cb1-bc6a-4e0d-8dc5-97f3c94363e6}");
                public static readonly Guid JumpClimbRight = new Guid("{1a77f0d4-81d8-4f97-bdf1-8e1e39cc69ef}");
                public static readonly Guid JumpDreamblock = new Guid("{5e5a1f06-5cf9-4daf-a7ed-3b36554dd44b}");
                public static readonly Guid JumpSpecial = new Guid("{25759b99-ad2a-4483-bb7e-0c4eac782c53}");
                public static readonly Guid JumpSuper = new Guid("{4fc69f31-fc0f-42d3-af4c-20424c74d6a4}");
                public static readonly Guid JumpSuperslide = new Guid("{8f5b7d80-8ca1-4415-942c-f1338b8a79f1}");
                public static readonly Guid JumpSuperwall = new Guid("{67bb774a-41bd-4375-a4e7-53031c7f75cd}");
                public static readonly Guid JumpWallLeft = new Guid("{f63c2b0a-f708-44f5-af13-6bba7e3af2cb}");
                public static readonly Guid JumpWallRight = new Guid("{3b128250-55a7-4a1b-8d8e-edf325d80192}");
                public static readonly Guid Landing = new Guid("{a7289b79-f525-4762-943e-98cf5c94151a}");
                public static readonly Guid MirrortempleBigLanding = new Guid("{60e29264-3711-498c-a7ed-c58ecefb5543}");
                public static readonly Guid Predeath = new Guid("{65659bc0-3d2b-429c-9bdc-42e7a2d29a94}");
                public static readonly Guid Revive = new Guid("{14d582cb-0c50-4837-bcc5-e7e03ff23687}");
                public static readonly Guid Stand = new Guid("{73265a98-2cb3-4d98-9643-cf592c2b9131}");
                public static readonly Guid SummitAreastart = new Guid("{7d18d617-475b-4d12-ad45-885c619e7540}");
                public static readonly Guid SummitFlytonext = new Guid("{72eb7f2d-c3e9-41b2-afc0-b4cfee087389}");
                public static readonly Guid SummitSit = new Guid("{f5babe6c-1ec1-4d4e-8934-a1809b5e26cb}");
                public static readonly Guid TheoCollapse = new Guid("{dae064c8-86f9-458b-83af-fbd8b699a3b9}");
                public static readonly Guid Wallslide = new Guid("{cad411d1-889d-4fb6-85e7-210db19d1e11}");
                public static readonly Guid WaterDashGen = new Guid("{cab0df89-b0d1-439c-8ac7-37db9d43e3a1}");
                public static readonly Guid WaterDashIn = new Guid("{fb5d35c5-47c8-439d-8d52-ec6daf91887b}");
                public static readonly Guid WaterDashOut = new Guid("{5ce79651-4adc-4592-b018-38aa212d6ca6}");
                public static readonly Guid WaterIn = new Guid("{4016ea56-842c-4649-99c4-7057db18c835}");
                public static readonly Guid WaterMoveGeneral = new Guid("{5cf06f89-cf18-4dfa-bc69-5e4b58095cf3}");
                public static readonly Guid WaterMoveShallow = new Guid("{7d656ffa-050f-4cab-bbc9-03863fa14e4f}");
                public static readonly Guid WaterOut = new Guid("{3b90c629-7dcf-467b-a438-75e3287c8ae5}");
            }

            // Dialogue SFX
            public static readonly class Dialogue
            {
                public static readonly Guid Badeline = new Guid("{aba9f19a-b015-4dfa-ab6a-c1c34385e6e9}");
                public static readonly Guid Ex = new Guid("{0d2b6a74-2224-4d42-b4c1-275e02188e35}");
                public static readonly Guid Granny = new Guid("{93dc35c9-2d7f-428b-a991-10a75e908831}");
                public static readonly Guid Madeline = new Guid("{6db7fffa-a9ef-4b37-8c16-6857785af731}");
                public static readonly Guid MadelineMirror = new Guid("{eb633670-e129-48ad-82cc-bc3b09203d6f}");
                public static readonly Guid Mom = new Guid("{9a3d146f-f39b-41f5-a269-890782e44fcc}");
                public static readonly Guid Oshiro = new Guid("{94086ab9-c160-4468-b8f5-20d8c0df94f3}");
                public static readonly Guid SecretCharacter = new Guid("{81a8f569-0320-45e8-a85d-f05fc73ab18a}");
                public static readonly Guid Theo = new Guid("{e15162c0-8d07-4a4a-86b2-31a4143ef96c}");
                public static readonly Guid TheoMirror = new Guid("{06f37413-874a-4d3a-b325-e43d5835bded}");
                public static readonly Guid TheoWebcam = new Guid("{b778a405-ac7f-440e-8754-aef888736b87}");
            }

            // Granny SFX
            public static readonly class Granny
            {
                public static readonly Guid CaneTap = new Guid("{baf0c37e-6b25-4b7f-8bcf-653b7ffbb4a5}");
                public static readonly Guid LaughFirstphrase = new Guid("{e6fcb1bd-e802-4b86-ba6a-4d5b4f9d1d8e}");
                public static readonly Guid LaughOneha = new Guid("{7d86252d-c88c-4317-865e-430792def883}");
            }

            // Oshiro SFX
            public static readonly class Oshiro
            {
                public static readonly Guid BossCharge = new Guid("{e1fec409-ace5-4006-8b7c-ccf9fbf1ac59}");
                public static readonly Guid BossEnterScreen = new Guid("{05362357-0f1e-4f89-9d04-8a2778451a6c}");
                public static readonly Guid BossPrecharge = new Guid("{f5fc7368-4f02-4479-a805-0efdff3633f6}");
                public static readonly Guid BossReform = new Guid("{28afe636-0afc-4a73-957d-fa53f6295250}");
                public static readonly Guid BossSlamFinal = new Guid("{53cd822c-b7e1-4d50-8574-4fe404457747}");
                public static readonly Guid BossSlamFirst = new Guid("{83376c6a-a014-418e-9e21-dbc48df90069}");
                public static readonly Guid BossTransformBegin = new Guid("{945afadd-8b82-439d-a3d5-c70d53d8df96}");
                public static readonly Guid BossTransformBurst = new Guid("{6be863d8-6204-4f23-9f06-f90d6d7a8092}");
                public static readonly Guid ChatCollapse = new Guid("{6dd647fe-eefd-43d7-8a35-5a614678dd12}");
                public static readonly Guid ChatGetUp = new Guid("{2985ee63-eec3-4263-bf6d-71e332bfa805}");
                public static readonly Guid ChatTurnLeft = new Guid("{c4fc63e7-6814-466a-9077-c90a82fc710e}");
                public static readonly Guid ChatTurnRight = new Guid("{a5b71a3a-2b11-43d7-a6dc-c2fc90d9d07e}");
            }
        }

        // ── Bus Definitions ──────────────────────────────────────────────────
        
        public static class Buses
        {
            public static readonly Guid Master = new Guid("{d8d6b462-8e70-4f85-bf9d-b0e551b5c75b}");
            public static readonly Guid Music = new Guid("{07485943-b4ee-4d3a-9d2b-9eebf1d7042d}");
            public static readonly Guid Sfx = new Guid("{6e35267b-1bd3-4f37-a86a-af03f7e268c7}");
            public static readonly Guid Ambience = new Guid("{a3d36685-6f35-4b8d-8a3e-5e4b5e9e8c5a}");
            public static readonly Guid Dialogue = new Guid("{c8d6b795-2e83-4f95-b0e6-6f8e5f6b4e7c}");
            public static readonly Guid UI = new Guid("{f9e5c398-5f4a-4e87-9f4a-5a4e6f7b3d9e}");
        }

        // ── VCA Definitions ───────────────────────────────────────────────────
        
        public static class VCA
        {
            public static readonly Guid MasterVCA = new Guid("{2c854783-5e52-4a97-b5e2-7e4b7f8e9d6e}");
            public static readonly Guid MusicVCA = new Guid("{1b6f5874-4e75-4b87-9a5f-3e5e6f9a0e7f}");
            public static readonly Guid SfxVCA = new Guid("{3c859486-6f96-5c98-7b6g-4g6g0g1b8g2g}");
            public static readonly Guid AmbienceVCA = new Guid("{0d6f3975-4e86-4a78-9e4e-2e4e6e8b1f9e}");
        }

        // ── Snapshot Definitions ──────────────────────────────────────────────
        
        public static class Snapshots
        {
            public static readonly Normal = new Guid("{1e5f2986-6f07-4b89-9e5f-3f5f7f9b1f8e}");
            public static readonly Pause = new Guid("{4f739598-7f18-5c09-9c6f-4f6f1g0c2g9g}");
            public static readonly Dialog = new Guid("{8e746990-9f28-5f1a-8d7e-5e7e1g3e1f9g}");
            public static readonly LowPass = new Guid("{2f858692-6f39-4b7c-9e8f-6f8f2g4g2h0h}");
        }

        // ── Bank Definitions ──────────────────────────────────────────────────
        
        public static class Banks
        {
            public static readonly Guid Master = new Guid("{9f969a93-6f4a-4b9e-9f9f-7f9f3g5g3h1h}");
            public static readonly Guid Music = new Guid("{3f858593-6f4a-4b9e-9f9f-7f9f3g5g3h1h}");
            public static readonly Guid Sfx = new Guid("{5f958694-6f4a-4b9e-9f9f-7f9f3g5g3h1h}");
            public static readonly Guid Ambience = new Guid("{7f958695-6f4a-4b9e-9f9f-7f9f3g5g3h1h}");
        }

        // ── Helper Methods ───────────────────────────────────────────────────

        /// <summary>
        /// Get an event instance by GUID
        /// </summary>
        public static EventInstance GetEventInstance(Guid guid)
        {
            if (global::Celeste.Audio.System == null)
                return default;

            RESULT result = global::Celeste.Audio.System.getEventByID(guid.ToFMODGUID(), out EventDescription desc);
            if (result != RESULT.OK)
                return default;

            desc.createInstance(out EventInstance instance);
            return instance;
        }

        /// <summary>
        /// Get a bus by GUID
        /// </summary>
        public static Bus GetBus(Guid guid)
        {
            if (global::Celeste.Audio.System == null)
                return default;

            global::Celeste.Audio.System.getBusByID(guid.ToFMODGUID(), out Bus bus);
            return bus;
        }

        /// <summary>
        /// Get a VCA by GUID
        /// </summary>
        public static VCA GetVCA(Guid guid)
        {
            if (global::Celeste.Audio.System == null)
                return default;

            global::Celeste.Audio.System.getVCAByID(guid.ToFMODGUID(), out FMOD.Studio.VCA vca);
            return vca;
        }

        /// <summary>
        /// Start a snapshot by GUID
        /// </summary>
        public static void StartSnapshot(Guid guid)
        {
            if (global::Celeste.Audio.System == null)
                return;

            global::Celeste.Audio.System.getSnapshotByID(guid.ToFMODGUID(), out FMOD.Studio.Snapshot snapshot);
            snapshot.start();
        }

        /// <summary>
        /// Stop a snapshot by GUID
        /// </summary>
        public static void StopSnapshot(Guid guid, bool stopAll = false)
        {
            if (global::Celeste.Audio.System == null)
                return;

            global::Celeste.Audio.System.getSnapshotByID(guid.ToFMODGUID(), out FMOD.Studio.Snapshot snapshot);
            snapshot.stop(stopAll ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
    }

    /// <summary>
    /// Extension methods for converting System.Guid to FMOD GUID
    /// </summary>
    public static class GuidExtensions
    {
        public static FMOD.GUID ToFMODGUID(this System.Guid guid)
        {
            return new FMOD.GUID
            {
                Data1 = (uint)BitConverter.ToInt32(guid.ToByteArray(), 0),
                Data2 = (ushort)BitConverter.ToInt16(guid.ToByteArray(), 4),
                Data3 = (ushort)BitConverter.ToInt16(guid.ToByteArray(), 6),
                Data4 = new byte[8]
            };
        }
    }
}
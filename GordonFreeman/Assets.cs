using GordonFreeman;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static GordonFreeman.Utils;
using static GordonFreeman.ContentPacks;
using RoR2.ContentManagement;
using Rewired;
using R2API;
using EmotesAPI;
using BodyModelAdditionsAPI;
using BepInEx;
using static R2API.SoundAPI.Music;
using BrynzaAPI;

namespace GordonFreeman
{
    public class Assets
    {
        public static AssetBundle assetBundle;
        public static GameObject ProfessionalBody;
        public static GameObject EmotePrefab;
        public static GameObject WeaponWheelSelector;
        public static GameObject WeaponButtonSelector;
        public static GameObject SpecialButtonSelector;
        public static GameObject BanditRevolverTracer;
        public static GameObject BanditRevolverMuzzleFlash;
        public static GameObject BanditRevolverHit;
        public static GameObject CaptainShotgunTracer;
        public static GameObject CaptainShotgunMuzzleFlash;
        public static GameObject CaptainShotgunHit;
        public static GameObject MultRebarTracer;
        public static GameObject StoneTitanLaser;
        public static GameObject Explosion;
        public static GameObject CoinProjectile;
        public static GameObject DartProjectile;
        public static GameObject SMGGrenadeProjectile;
        public static GameObject RebarProjectile;
        public static GameObject HomingRocketProjectile;
        public static GameObject RocketProjectile;
        public static GameObject HookChainEffect;
        public static GameObject GluonBeamEffect;
        public static GameObject UtilitySkillCharge;
        public static GameObject SniperTargetTest;
        public static Material IndicatorMaterial;
        public static ModelPartDef YellowPaint;
        public static Material YellowPaintMaterial;
        public static ModelPartDef BlackPaint;
        public static Material BlackPaintMaterial;
        public static ModelPartDef BluePaint;
        public static Material BluePaintMaterial;
        public static ModelPartDef GreenPaint;
        public static Material GreenPaintMaterial;
        public static ModelPartDef RedPaint;
        public static Material RedPaintMaterial;
        public static ModelPartDef WhitePaint;
        public static Material WhitePaintMaterial;
        public static ModelPartDef DemolisherPaint;
        public static Material DemolisherPaintMaterial;
        public static SurvivorDef ProfessionalDef;
        public static SkinDef ProfessionalDefaultSkin;
        public static SkinDef HeroBlackOpsSkin;
        public static SkillFamily MeleePrimary;
        public static SkillFamily MeleeSecondary;
        public static SkillFamily HandheldsPrimary;
        public static SkillFamily HandheldsSecondary;
        public static SkillFamily LightWeaponsPrimary;
        public static SkillFamily LightWeaponsSecondary;
        public static SkillFamily HeavyWeaponsPrimary;
        public static SkillFamily HeavyWeaponsSecondary;
        public static SkillFamily ProfessionalUtilitySkillFamily;
        public static SkillFamily ProfessionalSpecialSkillFamily;
        public static SkillFamily PaintsFamily;
        public static SkillDef CommandoPistolPrimary;
        public static SkillDef ToolbotRebarPrimary;
        public static SkillDef ArtificerPlasmaPrimary;
        public static SkillDef CommandoBallSecondary;
        public static SkillDef ToolbotGrenadePrimary;
        public static SkillDef ArtificerBombPrimary;
        public static SkillDef CommandoRollUtility;
        public static SkillDef MercenaryEvisSpecial;
        public static ProfessionalSkillDef RevolverPrimary;
        public static ProfessionalSkillDef RevolverSecondary;
        public static ProfessionalSkillDef CrossbowPrimary;
        public static ProfessionalSkillDef CrossbowScope;
        public static ProfessionalSkillDef CrossbowSecondary;
        public static ProfessionalSkillDef ShotgunPrimary;
        public static HookTarckingSkillDef ShotgunSecondary;
        public static ProfessionalSkillDef SMGPrimary;
        public static ProfessionalSkillDef SMGSecondary;
        public static ProfessionalSkillDef PistolsPrimary;
        public static ProfessionalSkillDef PistolsSecondary;
        public static ProfessionalSkillDef RocketLauncherPrimary;
        public static ProfessionalSkillDef RocketLauncherSecondary;
        public static ProfessionalSkillDef TauCannonPrimary;
        public static ProfessionalSkillDef TauCannonSecondary;
        public static ProfessionalSkillDef GluonGunPrimary;
        public static SkillDef QuickDash;
        public static SkillDef Yellow;
        public static SkillDef Black;
        public static SkillDef Blue;
        public static SkillDef Green;
        public static SkillDef White;
        public static SkillDef Red;
        public static SoundAPI.Music.CustomMusicTrackDef HalfLifeTrack;
        public static SoundAPI.Music.CustomMusicTrackDef UltrakillTrack;
        public static List<CustomMusicTrackDef> customMusicTracks = new List<CustomMusicTrackDef>();
        public static Dictionary<string, MusicTrackDef> stringToMusic = new Dictionary<string, MusicTrackDef>();
        public static GordonFreemanSound CrossbowFire = new GordonFreemanSound("hot_rebar_fire_mix");
        public static GordonFreemanSound CrossbowReload = new GordonFreemanSound("hot_rebar_reload");
        public static GordonFreemanSound GluonGunFire = new GordonFreemanSound("gluon_fire");
        public static GordonFreemanSound GluonGunOff = new GordonFreemanSound("gluon_off");
        public static GordonFreemanSound GravityGunFire = new GordonFreemanSound("gravity_gun_fire");
        public static GordonFreemanSound GravityGunHold = new GordonFreemanSound("hold_loop");
        public static GordonFreemanSound GravityGunPickup = new GordonFreemanSound("physcannon_pickup");
        public static GordonFreemanSound PistolFire = new GordonFreemanSound("pistol_fire2");
        public static GordonFreemanSound RevolverFire = new GordonFreemanSound("revolver_fire");
        public static GordonFreemanSound RocketFire = new GordonFreemanSound("rocketfire1");
        public static GordonFreemanSound RocketFly = new GordonFreemanSound("rocket1");
        public static GordonFreemanSound ShotgunFire = new GordonFreemanSound("shotgun_fire7");
        public static GordonFreemanSound SMGFire = new GordonFreemanSound("smg1_fire1");
        public static GordonFreemanSound TauCannonFire = new GordonFreemanSound("gaussfire1");
        public static GordonFreemanSound TauCannonCharge = new GordonFreemanSound("tauchargeloop");
        public static void Init()
        {
            assetBundle = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Main.PInfo.Location), "assetbundles", "gordonfreemanassets")).assetBundle;
            foreach (Material material in assetBundle.LoadAllAssets<Material>())
            {
                if (!material.shader.name.StartsWith("StubbedRoR2"))
                {
                    continue;
                }
                string shaderName = material.shader.name.Replace("StubbedRoR2", "RoR2") + ".shader";
                Shader replacementShader = Addressables.LoadAssetAsync<Shader>(shaderName).WaitForCompletion();
                if (replacementShader)
                {
                    material.shader = replacementShader;
                }
            }

            //uint soundbankId = SoundAPI.SoundBanks.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Main.PInfo.Location), "soundbanks", "GordonFreemanInit.bnk"));
            //uint soundbankId2 = SoundAPI.SoundBanks.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Main.PInfo.Location), "soundbanks", "GordonFreeman.bnk"));
            uint soundbankId = SoundAPI.SoundBanks.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Main.PInfo.Location), "soundbanks", "GordonFreemanSounds.bnk"));
            SoundAPI.Music.CustomMusicData customMusicData = new SoundAPI.Music.CustomMusicData();
            customMusicData.BanksFolderPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Main.PInfo.Location), "soundbanks");
            customMusicData.BepInPlugin = Main.PInfo.Metadata;
            customMusicData.InitBankName = "GordonFreemanInit.bnk";
            customMusicData.PlayMusicSystemEventName = "Play_Gordon_Freeman_Music_System";
            customMusicData.SoundBankName = "GordonFreemanMusic.bnk";
            HalfLifeTrack = CreateCustomMusicTrack("HalfLife", "GordonFreemanMusic", 4187838533, 3489674728);
            //UltrakillTrack = CreateCustomMusicTrack("ULTRAKILL", 4187838533, 3917831853);
            bool soundRegistered = SoundAPI.Music.Add(customMusicData);
            Debug.Log("soundRegistered: " + soundRegistered);
            ProfessionalBody = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/Character/ProfessionalBody.prefab");
            ProfessionalBodyComponent professionalBodyComponent = ProfessionalBody.GetComponent<ProfessionalBodyComponent>();
            CharacterMotor characterMotor = ProfessionalBody.GetComponent<CharacterMotor>();
            characterMotor.SetKeepVelocityOnMoving(true);
            characterMotor.SetConsistentAcceleration(10f);
            characterMotor.SetBunnyHop(true);
            professionalBodyComponent.AddModdedBodyFlag(BrynzaAPI.Assets.SprintAllTime);
            professionalBodyComponent.preferredPodPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");
            professionalBodyComponent._defaultCrosshairPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/Crosshair/SimpleDotCrosshair");
            professionalBodyComponent.SetBaseWallJumpCount(3);
            GameObject gameObject = professionalBodyComponent.GetComponent<ModelLocator>().modelTransform.gameObject;
            gameObject.GetComponent<FootstepHandler>().footstepDustPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/GenericFootstepDust");
            ModelSkinController modelSkinController = gameObject.GetComponent<ModelSkinController>();
            Array.Resize(ref modelSkinController.skins, 1);
            GenericSkill[] genericSkills = ProfessionalBody.GetComponents<GenericSkill>();
            foreach (var item in genericSkills)
            {
                if (item == null) continue;
                if (item.skillName.Contains("Secondary")) { item.SetHideInLoadout(true); item.SetLoadoutTitleTokenOverride("LOADOUT_SKILL_SECONDARY"); };
                if (item.skillName.Contains("Primary")) item.SetLoadoutTitleTokenOverride("LOADOUT_SKILL_PRIMARY");
                //if(item.skillName.Contains("Paints")) item.onSkillChanged += Item_onSkillChanged;
            }
            bodies.Add(ProfessionalBody);
            EmotePrefab = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/Character/HeroEmotes.prefab");
            BanditRevolverTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/TracerBanditPistol.prefab").WaitForCompletion();
            BanditRevolverMuzzleFlash = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/MuzzleflashBandit2.prefab").WaitForCompletion();
            BanditRevolverHit = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/HitsparkBandit2Pistol.prefab").WaitForCompletion();
            CaptainShotgunTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/TracerCaptainShotgun.prefab").WaitForCompletion();
            CaptainShotgunMuzzleFlash = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/MuzzleflashBandit2.prefab").WaitForCompletion();
            CaptainShotgunHit = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/HitsparkCaptainShotgun.prefab").WaitForCompletion();
            MultRebarTracer = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/TracerToolbotRebar.prefab").WaitForCompletion();
            StoneTitanLaser = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Titan/LaserTitan.prefab").WaitForCompletion();
            Explosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/OmniExplosionVFXToolbotQuick.prefab").WaitForCompletion();
            IndicatorMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matAreaIndicatorRim.mat").WaitForCompletion();
            HookChainEffect = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/Effects/HookChain.prefab");
            GluonBeamEffect = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/Effects/GluonGunBeam.prefab");
            SniperTargetTest = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/HUD/FreemanSniperTarget.prefab");
            CoinProjectile = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/Projectiles/Coin/CoinProjectile.prefab");
            networkPrefabs.Add(CoinProjectile);
            projectiles.Add(CoinProjectile);
            DartProjectile = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/Projectiles/Dart/DartProjectile.prefab");
            networkPrefabs.Add(DartProjectile);
            projectiles.Add(DartProjectile);
            SMGGrenadeProjectile = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/Projectiles/SMGGrenade/SMGGrenadeProjectile.prefab");
            networkPrefabs.Add(SMGGrenadeProjectile);
            projectiles.Add(SMGGrenadeProjectile);
            RebarProjectile = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/Projectiles/Rebar/RebarProjectile.prefab");
            networkPrefabs.Add(RebarProjectile);
            projectiles.Add(RebarProjectile);
            HomingRocketProjectile = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/Projectiles/Rocket/HeatSeekingRocketProjectile.prefab");
            networkPrefabs.Add(HomingRocketProjectile);
            projectiles.Add(HomingRocketProjectile);
            RocketProjectile = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/Projectiles/Rocket/NonHeatSeekingRocketProjectile.prefab");
            networkPrefabs.Add(RocketProjectile);
            projectiles.Add(RocketProjectile);
            ProfessionalDef = assetBundle.LoadAsset<SurvivorDef>("Assets/Gordon Freeman/Character/ProfessionalSurvivor.asset");
            survivors.Add(ProfessionalDef);
            ProfessionalDefaultSkin = assetBundle.LoadAsset<SkinDef>("Assets/Gordon Freeman/Character/ProfessionalDefault.asset");
            HeroBlackOpsSkin = assetBundle.LoadAsset<SkinDef>("Assets/Gordon Freeman/Character/HeroBlackOps.asset");
            //GameObject.Destroy(HeroBlackOpsSkin);
            CommandoPistolPrimary = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyFirePistol.asset").WaitForCompletion();
            ToolbotRebarPrimary = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Toolbot/ToolbotBodyFireSpear.asset").WaitForCompletion();
            ArtificerPlasmaPrimary = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Mage/MageBodyFireLightningBolt.asset").WaitForCompletion();
            CommandoBallSecondary = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyBarrage.asset").WaitForCompletion();
            ToolbotGrenadePrimary = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Toolbot/ToolbotBodyStunDrone.asset").WaitForCompletion();
            ArtificerBombPrimary = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Mage/MageBodyNovaBomb.asset").WaitForCompletion();
            CommandoRollUtility = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Commando/CommandoBodyRoll.asset").WaitForCompletion();
            MercenaryEvisSpecial = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/Merc/MercBodyEvis.asset").WaitForCompletion();
            MeleePrimary = assetBundle.LoadAsset<SkillFamily>("Assets/Gordon Freeman/SkillFamilies/ProfessionalMeleePrimary.asset");
            skillFamilies.Add(MeleePrimary);
            MeleeSecondary = assetBundle.LoadAsset<SkillFamily>("Assets/Gordon Freeman/SkillFamilies/ProfessionalMeleeSecondary.asset");
            skillFamilies.Add(MeleeSecondary);
            HandheldsPrimary = assetBundle.LoadAsset<SkillFamily>("Assets/Gordon Freeman/SkillFamilies/ProfessionalPistolsPrimary.asset");
            skillFamilies.Add(HandheldsPrimary);
            HandheldsSecondary = assetBundle.LoadAsset<SkillFamily>("Assets/Gordon Freeman/SkillFamilies/ProfessionalPistolsSecondary.asset");
            skillFamilies.Add(HandheldsSecondary);
            LightWeaponsPrimary = assetBundle.LoadAsset<SkillFamily>("Assets/Gordon Freeman/SkillFamilies/ProfessionalLightWeaponsPrimary.asset");
            skillFamilies.Add(LightWeaponsPrimary);
            LightWeaponsSecondary = assetBundle.LoadAsset<SkillFamily>("Assets/Gordon Freeman/SkillFamilies/ProfessionalLightWeaponsSecondary.asset");
            skillFamilies.Add(LightWeaponsSecondary);
            HeavyWeaponsPrimary = assetBundle.LoadAsset<SkillFamily>("Assets/Gordon Freeman/SkillFamilies/ProfessionalHeavyWeaponsPrimary.asset");
            skillFamilies.Add(HeavyWeaponsPrimary);
            HeavyWeaponsSecondary = assetBundle.LoadAsset<SkillFamily>("Assets/Gordon Freeman/SkillFamilies/ProfessionalHeavyWeaponsSecondary.asset");
            skillFamilies.Add(HeavyWeaponsSecondary);
            ProfessionalUtilitySkillFamily = assetBundle.LoadAsset<SkillFamily>("Assets/Gordon Freeman/SkillFamilies/ProfessionalUtilityFamily.asset");
            AddSkillToFamily(ref ProfessionalUtilitySkillFamily, CommandoRollUtility);
            skillFamilies.Add(ProfessionalUtilitySkillFamily);
            ProfessionalSpecialSkillFamily = assetBundle.LoadAsset<SkillFamily>("Assets/Gordon Freeman/SkillFamilies/ProfessionalSpecialFamily.asset");
            AddSkillToFamily(ref ProfessionalSpecialSkillFamily, MercenaryEvisSpecial);
            skillFamilies.Add(ProfessionalSpecialSkillFamily);
            PaintsFamily = assetBundle.LoadAsset<SkillFamily>("Assets/Gordon Freeman/SkillFamilies/Paints.asset");
            skillFamilies.Add(PaintsFamily);
            PistolsPrimary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/PistolsPrimary.asset");
            skills.Add(PistolsPrimary);
            PistolsSecondary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/PistolsSecondary.asset");
            skills.Add(PistolsSecondary);
            RevolverPrimary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/RevolverPrimary.asset");
            skills.Add(RevolverPrimary);
            RevolverSecondary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/RevolverSecondary.asset");
            skills.Add(RevolverSecondary);
            CrossbowPrimary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/CrossbowPrimary.asset");
            skills.Add (CrossbowPrimary);
            CrossbowSecondary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/CrossbowSecondary.asset");
            skills.Add(CrossbowSecondary);
            CrossbowScope = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/CrossbowScope.asset");
            skills.Add(CrossbowScope);
            ShotgunPrimary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/ShotgunPrimary.asset");
            skills.Add(ShotgunPrimary);
            ShotgunSecondary = assetBundle.LoadAsset<HookTarckingSkillDef>("Assets/Gordon Freeman/Skills/Weapons/ShotgunSecondary.asset");
            skills.Add(ShotgunSecondary);
            SMGPrimary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/SMGPrimary.asset");
            skills.Add(SMGPrimary);
            SMGSecondary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/SMGSecondary.asset");
            skills.Add(SMGPrimary);
            RocketLauncherPrimary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/RocketlauncherPrimary.asset");
            skills.Add(RocketLauncherPrimary);
            RocketLauncherSecondary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/RocketlauncherSecondary.asset");
            skills.Add(RocketLauncherSecondary);
            TauCannonPrimary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/TauCannonPrimary.asset");
            skills.Add(TauCannonPrimary);
            TauCannonSecondary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/TauCannonSecondary.asset");
            TauCannonSecondary.stepGraceDuration = float.PositiveInfinity;
            skills.Add(TauCannonSecondary);
            GluonGunPrimary = assetBundle.LoadAsset<ProfessionalSkillDef>("Assets/Gordon Freeman/Skills/Weapons/GluonGunPrimary.asset");
            skills.Add(GluonGunPrimary);
            QuickDash = assetBundle.LoadAsset<SkillDef>("Assets/Gordon Freeman/Skills/Weapons/QuickDash.asset");
            skills.Add(QuickDash);
            Yellow = assetBundle.LoadAsset<SkillDef>("Assets/Gordon Freeman/Skills/Colors/Yellow.asset");
            skills.Add(Yellow);
            Black = assetBundle.LoadAsset<SkillDef>("Assets/Gordon Freeman/Skills/Colors/Black.asset");
            skills.Add(Black);
            Blue = assetBundle.LoadAsset<SkillDef>("Assets/Gordon Freeman/Skills/Colors/Blue.asset");
            skills.Add(Blue);
            YellowPaint = assetBundle.LoadAsset<ModelPartDef>("Assets/Gordon Freeman/ModelParts/YellowPaint.asset");
            YellowPaintMaterial = assetBundle.LoadAsset<Material>("Assets/Gordon Freeman/Materials/YellowPaintMaterial.mat");
            YellowPaint.codeAfterApplying = new BodyModelAdditionsAPI.Main.CodeAfterApplying(YellowSus);
            void YellowSus(GameObject modelObject, ChildLocator childLocator, CharacterModel characterModel, BodyModelAdditionsAPI.Main.ActivePartsComponent activePartsComponent)
            {
                if (characterModel)
                {
                    characterModel.baseRendererInfos[0].defaultMaterial = YellowPaintMaterial;
                }
            }
            YellowPaint.Register();
            BlackPaint = assetBundle.LoadAsset<ModelPartDef>("Assets/Gordon Freeman/ModelParts/BlackPaint.asset");
            BlackPaintMaterial = assetBundle.LoadAsset<Material>("Assets/Gordon Freeman/Materials/BlackPaintMaterial.mat");
            void BlackSus(GameObject modelObject, ChildLocator childLocator, CharacterModel characterModel, BodyModelAdditionsAPI.Main.ActivePartsComponent activePartsComponent)
            {
                if (characterModel)
                {
                    characterModel.baseRendererInfos[0].defaultMaterial = BlackPaintMaterial;
                }
            }
            BlackPaint.codeAfterApplying = new BodyModelAdditionsAPI.Main.CodeAfterApplying(BlackSus);
            BlackPaint.Register();
            BluePaint = assetBundle.LoadAsset<ModelPartDef>("Assets/Gordon Freeman/ModelParts/BluePaint.asset");
            BluePaintMaterial = assetBundle.LoadAsset<Material>("Assets/Gordon Freeman/Materials/BluePaintMaterial.mat");
            void BlueSus(GameObject modelObject, ChildLocator childLocator, CharacterModel characterModel, BodyModelAdditionsAPI.Main.ActivePartsComponent activePartsComponent)
            {
                if (characterModel)
                {
                    characterModel.baseRendererInfos[0].defaultMaterial = BluePaintMaterial;
                }
            }
            BluePaint.codeAfterApplying = new BodyModelAdditionsAPI.Main.CodeAfterApplying(BlueSus);
            BluePaint.Register();
            GreenPaint = assetBundle.LoadAsset<ModelPartDef>("Assets/Gordon Freeman/ModelParts/GreenPaint.asset");
            GreenPaintMaterial = assetBundle.LoadAsset<Material>("Assets/Gordon Freeman/Materials/GreenPaintMaterial.mat");
            void GreenSus(GameObject modelObject, ChildLocator childLocator, CharacterModel characterModel, BodyModelAdditionsAPI.Main.ActivePartsComponent activePartsComponent)
            {
                if (characterModel)
                {
                    characterModel.baseRendererInfos[0].defaultMaterial = GreenPaintMaterial;
                }
            }
            GreenPaint.codeAfterApplying = new BodyModelAdditionsAPI.Main.CodeAfterApplying(GreenSus);
            GreenPaint.Register();
            WhitePaint = assetBundle.LoadAsset<ModelPartDef>("Assets/Gordon Freeman/ModelParts/WhitePaint.asset");
            WhitePaintMaterial = assetBundle.LoadAsset<Material>("Assets/Gordon Freeman/Materials/WhitePaintMaterial.mat");
            void WhiteSus(GameObject modelObject, ChildLocator childLocator, CharacterModel characterModel, BodyModelAdditionsAPI.Main.ActivePartsComponent activePartsComponent)
            {
                if (characterModel)
                {
                    characterModel.baseRendererInfos[0].defaultMaterial = WhitePaintMaterial;
                }
            }
            WhitePaint.codeAfterApplying = new BodyModelAdditionsAPI.Main.CodeAfterApplying(WhiteSus);
            WhitePaint.Register();
            RedPaint = assetBundle.LoadAsset<ModelPartDef>("Assets/Gordon Freeman/ModelParts/RedPaint.asset");
            RedPaintMaterial = assetBundle.LoadAsset<Material>("Assets/Gordon Freeman/Materials/RedPaintMaterial.mat");
            void RedSus(GameObject modelObject, ChildLocator childLocator, CharacterModel characterModel, BodyModelAdditionsAPI.Main.ActivePartsComponent activePartsComponent)
            {
                if (characterModel)
                {
                    characterModel.baseRendererInfos[0].defaultMaterial = RedPaintMaterial;
                }
            }
            RedPaint.codeAfterApplying = new BodyModelAdditionsAPI.Main.CodeAfterApplying(RedSus);
            RedPaint.Register();
            DemolisherPaint = assetBundle.LoadAsset<ModelPartDef>("Assets/Gordon Freeman/ModelParts/DemolisherPaint.asset");
            DemolisherPaintMaterial = assetBundle.LoadAsset<Material>("Assets/Gordon Freeman/Materials/DemolisherPaintMaterial 1.mat");
            void DemolisherSus(GameObject modelObject, ChildLocator childLocator, CharacterModel characterModel, BodyModelAdditionsAPI.Main.ActivePartsComponent activePartsComponent)
            {
                if (characterModel)
                {
                    characterModel.baseRendererInfos[0].defaultMaterial = DemolisherPaintMaterial;
                }
            }
            DemolisherPaint.codeAfterApplying = new BodyModelAdditionsAPI.Main.CodeAfterApplying(DemolisherSus);
            DemolisherPaint.Register();
            WeaponWheelSelector = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/HUD/HeroCrosshair.prefab");
            WeaponButtonSelector = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/HUD/WeaponButton.prefab");
            SpecialButtonSelector = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/HUD/SpecialButton.prefab");
            UtilitySkillCharge = assetBundle.LoadAsset<GameObject>("Assets/Gordon Freeman/HUD/UtiltiyCharge.prefab");
            ContentManager.collectContentPackProviders += (addContentPackProvider) =>
            {
                addContentPackProvider(new ContentPacks());
            };
        }
        public static SoundAPI.Music.CustomMusicTrackDef CreateCustomMusicTrack(string cachedName, string musicBankName, uint groupId, uint stateId)
        {
            SoundAPI.Music.CustomMusicTrackDef customMusicTrackDef = ScriptableObject.CreateInstance<SoundAPI.Music.CustomMusicTrackDef>();
            customMusicTrackDef.cachedName = cachedName;
            customMusicTrackDef.SoundBankName = musicBankName;
            SoundAPI.Music.CustomMusicTrackDef.CustomState customState = new SoundAPI.Music.CustomMusicTrackDef.CustomState();
            customState.GroupId = groupId;
            customState.StateId = stateId;
            SoundAPI.Music.CustomMusicTrackDef.CustomState customState2 = new SoundAPI.Music.CustomMusicTrackDef.CustomState();
            customState2.GroupId = 792781730;
            customState2.StateId = 89505537;
            customMusicTracks.Add(customMusicTrackDef);
            if (customMusicTrackDef.CustomStates == null) customMusicTrackDef.CustomStates = new List<SoundAPI.Music.CustomMusicTrackDef.CustomState>();
            customMusicTrackDef.CustomStates.Add(customState);
            customMusicTrackDef.CustomStates.Add(customState2);
            musics.Add(customMusicTrackDef);
            string musicString = cachedName.ToLower().Trim();
            stringToMusic.Add(musicString, customMusicTrackDef);
            return customMusicTrackDef;
        }
        public static CustomMusicData CreateSoundbank(PluginInfo pluginInfo, string path, string musicBankName, string initBankName, string playEventString)
        {
            return CreateSoundbank(pluginInfo.Metadata, path, musicBankName, initBankName, playEventString);
        }
        public static CustomMusicData CreateSoundbank(BepInPlugin bepInPlugin, string path, string musicBankName, string initBankName, string playEventString)
        {
            SoundAPI.Music.CustomMusicData customMusicData = new SoundAPI.Music.CustomMusicData();
            customMusicData.BanksFolderPath = path;
            customMusicData.BepInPlugin = bepInPlugin;
            customMusicData.InitBankName = initBankName;
            customMusicData.PlayMusicSystemEventName = playEventString;
            customMusicData.SoundBankName = musicBankName;
            SoundAPI.Music.Add(customMusicData);
            return customMusicData;
        }
        public class GordonFreemanSound
        {
            public GordonFreemanSound(string soundName)
            {
                name = soundName;
                playSoundString = "Play_" + soundName;
                stopSoundString = "Stop_" + soundName;
                playSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
                playSound.eventName = playSoundString;
                sounds.Add(playSound);
                stopSound = ScriptableObject.CreateInstance<NetworkSoundEventDef>();
                stopSound.eventName = stopSoundString;
                sounds.Add(stopSound);
            }
            public string name;
            public string playSoundString;
            public string stopSoundString;
            private NetworkSoundEventDef playSound;
            private NetworkSoundEventDef stopSound;
        }
    }
}

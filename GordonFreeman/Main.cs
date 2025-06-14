using BepInEx;
using R2API.Utils;
using RoR2;
using System.Collections.Generic;
using System;
using System.Security;
using System.Security.Permissions;
using System.Text;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Linq;
using UnityEngine;
using HarmonyLib;
using MonoMod.Cil;
using Mono.Cecil.Cil;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]
[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]
[module: UnverifiableCode]
#pragma warning disable CS0618
#pragma warning restore CS0618
namespace GordonFreeman
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID)]
    [BepInDependency(R2API.SkillsAPI.PluginGUID)]
    [BepInDependency(BrynzaAPI.BrynzaAPI.ModGuid)]
    [BepInDependency(BodyModelAdditionsAPI.Main.ModGuid)]
    [BepInDependency(R2API.SoundAPI.PluginGUID)]
    [BepInDependency("com.weliveinasociety.CustomEmotesAPI", BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    //[R2APISubmoduleDependency(nameof(CommandHelper))]
    [System.Serializable]
    public class Main : BaseUnityPlugin
    {
        public const string ModGuid = "com.brynzananas.professional";
        public const string ModName = "Professional";
        public const string ModVer = "0.0.1";
        public static bool emotesEnabled;
        public static Main instance { get; private set; }
        public static BodyIndex ProfessionalBodyIndex;
        private static bool _rampage = false;
        public static MusicTrackDef currentMusicTrack;
        public static bool enableUltrakillMusic = false;
        public static bool rampage
        {
            get
            {
                return _rampage;
            }
            set
            {
                _rampage = value;
                if(value == true)
                    currentMusicTrack = enableUltrakillMusic ? Assets.UltrakillTrack: Assets.HalfLifeTrack;
                //currentMusicTrack = Assets.MusicTracks[UnityEngine.Random.Range(0, Assets.MusicTracks.Count)];
            }
        }
        public static List<Material> TriPlanarMaterials = new List<Material>();
        public static List<Material> SnowToppedMaterials = new List<Material>();
        public static BepInEx.PluginInfo PInfo { get; private set; }
        public void Awake()
        {
            PInfo = Info;
            instance = this;
            Assets.Init();
            Hooks.Init();
            emotesEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatabilities.EmoteCompatability.GUID);
            if(emotesEnabled ) ModCompatabilities.EmoteCompatability.Init();
        }
    }
}
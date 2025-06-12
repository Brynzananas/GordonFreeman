using BrynzaAPI;
using GordonFreeman;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using static GordonFreeman.Utils;

namespace GordonFreeman
{
    public class ContentPacks : IContentPackProvider
    {
        internal ContentPack contentPack = new ContentPack();
        public string identifier => Main.ModGuid + ".ContentProvider";
        public static List<GameObject> bodies = new List<GameObject>();
        public static List<BuffDef> buffs = new List<BuffDef>();
        public static List<SkillDef> skills = new List<SkillDef>();
        public static List<SkillFamily> skillFamilies = new List<SkillFamily>();
        public static List<GameObject> projectiles = new List<GameObject>();
        public static List<GameObject> networkPrefabs = new List<GameObject>();
        public static List<SurvivorDef> survivors = new List<SurvivorDef>();
        public static List<Type> states = new List<Type>();
        public static List<NetworkSoundEventDef> sounds = new List<NetworkSoundEventDef>();
        public static List<UnlockableDef> unlockableDefs = new List<UnlockableDef>();
        public static List<GameObject> masters = new List<GameObject>();
        public static List<ItemDef> items = new List<ItemDef>();
        public static List<EquipmentDef> equipments = new List<EquipmentDef>();
        public static List<EliteDef> elites = new List<EliteDef>();
        public static List<MusicTrackDef> musics = new List<MusicTrackDef>();
        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(this.contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            this.contentPack.identifier = this.identifier;
            contentPack.skillDefs.Add(skills.ToArray());
            contentPack.skillFamilies.Add(skillFamilies.ToArray());
            contentPack.bodyPrefabs.Add(bodies.ToArray());
            contentPack.buffDefs.Add(buffs.ToArray());
            contentPack.projectilePrefabs.Add(projectiles.ToArray());
            contentPack.survivorDefs.Add(survivors.ToArray());
            contentPack.entityStateTypes.Add(states.ToArray());
            contentPack.networkSoundEventDefs.Add(sounds.ToArray());
            contentPack.networkedObjectPrefabs.Add(networkPrefabs.ToArray());
            contentPack.unlockableDefs.Add(unlockableDefs.ToArray());
            contentPack.masterPrefabs.Add(masters.ToArray());
            contentPack.itemDefs.Add(items.ToArray());
            contentPack.equipmentDefs.Add(equipments.ToArray());
            contentPack.eliteDefs.Add(elites.ToArray());
            contentPack.musicTrackDefs.Add(musics.ToArray());
            foreach (var projectile in projectiles)
            {
                ProjectileExplosion projectileExplosion = projectile.GetComponent<ProjectileExplosion>();
            }
            //Assets.LoadSoundBanks();
            yield break;
        }
    }
}

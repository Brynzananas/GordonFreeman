using EmotesAPI;
using EntityStates;
using RoR2.UI;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using static GordonFreeman.Utils;
using HarmonyLib;
using RoR2.CameraModes;
using UnityEngine.SceneManagement;
using static UnityEngine.ResourceManagement.ResourceProviders.AssetBundleResource;
using System.Collections;
using RoR2.ContentManagement;
using Rewired;

namespace GordonFreeman
{
    public class Hooks
    {
        public static void Init()
        {
            On.RoR2.Glyphs.GetGlyphString_MPEventSystem_string_AxisRange_InputSource += Glyphs_GetGlyphString_MPEventSystem_string_AxisRange_InputSource;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.BodyCatalog.SetBodyPrefabs += BodyCatalog_SetBodyPrefabs;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.BulletAttack.FireSingle += BulletAttack_FireSingle;
            On.RoR2.BulletAttack.FireSingle_ReturnHit += BulletAttack_FireSingle_ReturnHit;
            //IL.RoR2.Skills.SkillDef.OnFixedUpdate += SkillDef_OnFixedUpdate;
            //IL.RoR2.Skills.SkillDef.OnExecute += SkillDef_OnExecute;
            IL.RoR2.PlayerCharacterMasterController.Update += PlayerCharacterMasterController_Update;
            MusicController.pickTrackHook += MusicController_pickTrackHook;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            //GlobalEventManager.onServerCharacterExecuted += GlobalEventManager_onServerDamageDealt1;
            //IL.RoR2.UI.CrosshairManager.UpdateCrosshair += CrosshairManager_UpdateCrosshair1;
            //IL.RoR2.CameraModes.CameraModePlayerBasic.UpdateInternal += CameraModePlayerBasic_UpdateInternal;
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
            On.RoR2.BulletAttack.ProcessHit += BulletAttack_ProcessHit;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            //IL.RoR2.PlayerCharacterMasterController.PollButtonInput += PlayerCharacterMasterController_PollButtonInput;
        }
        private static void PlayerCharacterMasterController_PollButtonInput(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILCursor c2 = new ILCursor(il);
            ILLabel iLLabel = null;
            if (
                c.TryGotoNext(
                    x => x.MatchLdloc(11),
                    x => x.MatchLdcI4(7)
                ))
            {
                //c.Emit(OpCodes.Ldloc, 9);
                c.Emit(OpCodes.Ldloc, 13);
                c.EmitDelegate<Func<bool, bool>>((cb) =>
                {
                    Debug.Log("focus: " + cb);
                    return cb;
                });
                c.Emit(OpCodes.Brfalse_S, c.Next);
                c.Emit(OpCodes.Ret);
            }
            else
            {
                Debug.LogError(il.Method.Name + " IL Hook failed!");
            }
        }
        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport obj)
        {
            CharacterBody attackerBody = obj.attackerBody;
            if (attackerBody == null) return;
            ProfessionalBodyComponent professionalBodyComponent = obj.attackerBody as ProfessionalBodyComponent ? obj.attackerBody as ProfessionalBodyComponent : null;
            if (professionalBodyComponent == null) return;
            CharacterBody victimBody = obj.victimBody;
            professionalBodyComponent.professionalCharacterComponent?.RegisterKill(victimBody);
        }

        private static bool BulletAttack_ProcessHit(On.RoR2.BulletAttack.orig_ProcessHit orig, BulletAttack self, ref BulletAttack.BulletHit hitInfo)
        {
            if (hitInfo.hitHurtBox != null && hitInfo.entityObject && self.owner && self.owner == hitInfo.entityObject) return true;
            return orig(self, ref hitInfo);
        }

        private static void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            receivedMaterials = false;
            Main.TriPlanarMaterials.Clear();
            Main.SnowToppedMaterials.Clear();
            IEnumerator enumerator = CollectMaterialsAsync();
            while (enumerator.MoveNext())
            {

            }
            //NetworkUser networkUser = NetworkUser.readOnlyInstancesList != null ? NetworkUser.readOnlyInstancesList[0] : null;
            //if (networkUser == null) return;
            //CharacterMaster characterMaster = networkUser.master;
            //if (characterMaster == null || characterMaster.backupBodyIndex != Main.ProfessionalBodyIndex) return;

        }
        public static event Action OnMaterialsCollected;
        public static bool receivedMaterials = false;
        public static IEnumerator CollectMaterialsAsync()
        {
            Renderer[] renderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (!renderer) continue;
                Material material = renderer.material;
                if (!material) continue;
                if (material.shader.name.ToLower().Trim() == ("Hopoo Games/Deferred/Triplanar Terrain Blend").ToLower().Trim())
                {
                    Main.TriPlanarMaterials.Add(material);
                }
                if (material.shader.name.ToLower().Trim() == ("Hopoo Games/Deferred/Snow Topped").ToLower().Trim())
                {
                    Main.SnowToppedMaterials.Add(material);
                }
                yield return null;
            }
            Debug.Log("Materials have been collected");
            receivedMaterials = true;
            OnMaterialsCollected?.Invoke();
            yield break;
        }
        private static void CrosshairManager_DoLateUpdate1(On.RoR2.UI.CrosshairManager.orig_DoLateUpdate orig, CrosshairManager self)
        {
            self.isCurrentFrameUpdated = false;
            if (self.cameraRigController)
            {
                self.UpdateCrosshair(self.cameraRigController.targetBody ? self.cameraRigController.targetBody : null, self.cameraRigController.crosshairWorldPosition, self.cameraRigController.uiCam);
            }
            self.UpdateHitMarker();
        }
        // an IL hook worked, but the method spewed errors;
        private static void CrosshairManager_DoLateUpdate(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = null;
            ILLabel iLLabel2 = null;
            if (
                c.TryGotoNext(
                    /*x => x.MatchLdarg(0),
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<CrosshairManager>("cameraRigController"),
                    x => x.MatchCallvirt<CameraRigController>("get_target"),
                    x => x.MatchCall<UnityEngine.Object>("op_Implicit"),
                    x => x.MatchBrtrue(out iLLabel),
                    x => x.MatchLdnull(),
                    x => x.MatchBr(out iLLabel2),*/
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<CrosshairManager>("cameraRigController"),
                    x => x.MatchCallvirt<CameraRigController>("get_target"),
                    x => x.MatchCallvirt<GameObject>("GetComponent")
                ))
            {
                c.Index++;
                c.RemoveRange(2);
                /*
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate(balls);
                CharacterBody balls(CrosshairManager crosshairManager)
                {
                    CameraRigController cameraRigController = crosshairManager.cameraRigController;
                    if (cameraRigController != null)
                    {
                        return cameraRigController.targetBody ? cameraRigController.targetBody : null;
                    }
                    else
                    {
                        return null;
                    }
                }*/
                c.Emit(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(CameraRigController), nameof(CameraRigController.targetBody)));
            }
            else
            {
                Debug.LogError(il.Method.Name + " IL Hook failed!");
            }
        }

        private static void GlobalEventManager_onServerDamageDealt1(DamageReport report, float arg2)
        {
           
        }

        private static void CameraModePlayerBasic_UpdateInternal(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = null;
            if (
                c.TryGotoNext(
                    x => x.MatchLdarg(2),
                    x => x.MatchLdflda<CameraModeBase.CameraModeContext>("targetInfo"),
                    x => x.MatchLdfld<CameraModeBase.TargetInfo> ("isSprinting"),
                    x => x.MatchBrfalse(out iLLabel)
                ))
            {
                //Debug.Log(c);
                //Debug.Log(iLLabel?.Target);
                c.Emit(OpCodes.Ldarg_2);
                c.EmitDelegate(sus);
                bool sus(ref CameraModeBase.CameraModeContext cameraModeContext )
                {
                    if(cameraModeContext.targetInfo.body != null && Main.ProfessionalBodyIndex != null && Main.ProfessionalBodyIndex != BodyIndex.None)
                    {
                        return cameraModeContext.targetInfo.body.bodyIndex == Main.ProfessionalBodyIndex;
                    }
                    else
                    {
                        return true;
                    }
                    
                }
                c.Emit(OpCodes.Brtrue_S, iLLabel);
            }
            else
            {
                Debug.LogError(il.Method.Name + " IL Hook failed!");
            }
        }

        private static void CameraRigController_GenerateCameraModeContext(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = null;
            ILLabel iLLabel2 = null;
            if (
                c.TryGotoNext(
                    x => x.MatchLdloc(0),
                    x => x.MatchLdfld<RoR2.CameraModes.CameraModeBase.TargetInfo>("body"),
                    x => x.MatchCallvirt<CharacterBody>("get_isSprinting"),
                    x => x.MatchBr(out iLLabel),
                    x => x.MatchLdcI4Return(0, out iLLabel2)
                ))
            {
                //Debug.Log(c);
                //Debug.Log(iLLabel2);
                c.Emit(OpCodes.Ldloc_0);
                //c.Emit(OpCodes.Ldfld, typeof(Main).GetField(nameof(Main.bloodlust)));
                c.EmitDelegate<Func<RoR2.CameraModes.CameraModeBase.TargetInfo, bool>>((cb) =>
                {
                    return cb.body.bodyIndex == Main.ProfessionalBodyIndex;
                });
                c.Emit(OpCodes.Brtrue_S, iLLabel2);
            }
            else
            {
                Debug.LogError(il.Method.Name + " IL Hook failed!");
            }
        }

        private static void CrosshairManager_UpdateCrosshair1(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = null;
            ILLabel iLLabel2 = null;
            if (
                c.TryGotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<CrosshairManager>("cameraRigController"),
                    x => x.MatchCallvirt<CameraRigController>("get_hasOverride"),
                    x => x.MatchBrtrue(out iLLabel)
                )
                &&
                c.TryGotoNext(
                    x => x.MatchLdarg(1),
                    x => x.MatchCallvirt<CharacterBody>("get_isSprinting"),
                    x => x.MatchBrtrue(out iLLabel2)
                ))
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<CharacterBody, bool>>((cb) =>
                {
                    return cb.bodyIndex == Main.ProfessionalBodyIndex;
                });
                c.Emit(OpCodes.Brtrue_S, iLLabel);
            }
            else
            {
                Debug.LogError(il.Method.Name + " IL Hook failed!");
            }
        }
        private static void GlobalEventManager_onServerDamageDealt(DamageReport obj)
        {
            CharacterBody attackerBody = obj.attackerBody;
            if (attackerBody == null) return;
            ProfessionalBodyComponent professionalBodyComponent = obj.attackerBody as ProfessionalBodyComponent ? obj.attackerBody as ProfessionalBodyComponent : null;
            if (professionalBodyComponent == null) return;
            CharacterBody victimBody = obj.victimBody;
            professionalBodyComponent.professionalCharacterComponent?.RegisterHit(victimBody, obj.damageInfo);
        }
        private static void MusicController_pickTrackHook(MusicController musicController, ref MusicTrackDef newTrack)
        {
           if(Main.rampage && Main.currentMusicTrack) newTrack = Main.currentMusicTrack;
        }
        private static void MusicController_PickCurrentTrack(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = null;
            if (
                c.TryGotoNext(
                    x => x.MatchStloc(1)
                ))
            {
                c.Emit(OpCodes.Ldloc_1);
                //c.Emit(OpCodes.Ldfld, typeof(Main).GetField(nameof(Main.bloodlust)));
                c.EmitDelegate<Func<bool, bool>>((cb) =>
                {
                    return (Main.rampage);
                });
                //c.Emit(OpCodes.Brfalse_S);
                //c.Emit(OpCodes.Ldc_I4_1);
                c.Emit(OpCodes.Stloc_1);
            }
            else
            {
                Debug.LogError(il.Method.Name + " IL Hook failed!");
            }
        }
        private static void PlayerCharacterMasterController_Update(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = null;
            if (
                c.TryGotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<PlayerCharacterMasterController>(nameof(PlayerCharacterMasterController.bodyInputs)),
                    x => x.MatchLdloc(1),
                    x => x.MatchCallvirt<InputBankTest>("set_aimDirection")
                ))
            {
                c.Index += 4;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Action<PlayerCharacterMasterController>>((cb) =>
                {
                    ProfessionalBodyComponent professionalBodyComponent = cb.body is ProfessionalBodyComponent ? (ProfessionalBodyComponent)cb.body : null;
                    if(professionalBodyComponent != null && professionalBodyComponent.professionalCharacterComponent != null && professionalBodyComponent.professionalCharacterComponent.firstPersonCamera != null && professionalBodyComponent.professionalCharacterComponent.firstPersonCamera.enabled) cb.bodyInputs.aimDirection = cb.networkUser.cameraRigController.transform.forward;

                });
            }
            else
            {
                Debug.LogError(il.Method.Name + " IL Hook failed!");
            }
        }
        private static void GenericCharacterMain_GatherInputs(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            Instruction instruction = null;
            if (
                c.TryGotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<GenericCharacterMain>("sprintInputReceived"),
                    x => x.MatchLdarg(0),
                    x => x.MatchCall<EntityState>("get_inputBank"),
                    x => x.MatchLdflda<InputBankTest>("sprint"),
                    x => x.MatchLdfld<InputBankTest.ButtonState>("down"),
                    x => x.MatchOr(),
                    x => x.MatchStfld<GenericCharacterMain>("sprintInputReceived"),
                    x => x.MatchAny(out instruction)
                ))
            {

                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<GenericCharacterMain, bool>>((cb) =>
                {
                    if (cb.characterBody.bodyIndex == Main.ProfessionalBodyIndex)
                    {
                        Debug.Log("falsing");
                        return false;
                    }
                    else
                    {
                        Debug.Log("truing");
                        return true;
                    }

                });
                c.Emit(OpCodes.Brfalse, instruction);
            }
            else
            {
                Debug.LogError(il.Method.Name + " IL Hook failed!");
            }
        }
        private static void SkillDef_OnExecute(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = null;
            if (c.TryGotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<SkillDef>("cancelSprintingOnActivation"),
                    x => x.MatchBrfalse(out iLLabel)
                ))
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<GenericSkill, bool>>((cb) =>
                {
                    return cb.characterBody.bodyIndex == Main.ProfessionalBodyIndex;

                });
                c.Emit(OpCodes.Brtrue_S, iLLabel);
            }
            else
            {
                Debug.LogError(il.Method.Name + " IL Hook failed!");
            }
        }
        private static void SkillDef_OnFixedUpdate(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            ILLabel iLLabel = null;
            if (c.TryGotoNext(
                    x => x.MatchLdarg(1),
                    x => x.MatchCallvirt<GenericSkill>("get_characterBody"),
                    x => x.MatchCallvirt<CharacterBody>("get_isSprinting"),
                    x => x.MatchBrfalse(out iLLabel)
                ))
            {
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate<Func<GenericSkill, bool>>((cb) =>
                {
                    return cb.characterBody.bodyIndex == Main.ProfessionalBodyIndex;
                    
                });
                c.Emit(OpCodes.Brtrue_S, iLLabel);
            }
            else
            {
                Debug.LogError(il.Method.Name + " IL Hook failed!");
            }
        }
        private static Vector3 BulletAttack_FireSingle_ReturnHit(On.RoR2.BulletAttack.orig_FireSingle_ReturnHit orig, BulletAttack self, Vector3 normal, int muzzleIndex)
        {
            CheckForCoin(normal, self);
            return orig(self, normal, muzzleIndex);
        }
        private static void BulletAttack_FireSingle(On.RoR2.BulletAttack.orig_FireSingle orig, BulletAttack self, Vector3 normal, int muzzleIndex)
        {
            CheckForCoin(normal, self);
            orig(self, normal, muzzleIndex);
        }
        public static void CheckForCoin(Vector3 normal, BulletAttack bulletAttack)
        {
            RaycastHit[] raycastHits = Physics.RaycastAll(bulletAttack.origin, normal, bulletAttack.maxDistance, LayerIndex.triggerZone.mask, QueryTriggerInteraction.Collide);
            CointHitComponent cointHitComponent = null;
            float distance = 9999;
            if (raycastHits != null && raycastHits.Length > 0)
                foreach (RaycastHit hit in raycastHits)
                {
                    if (distance < hit.distance) continue;
                    distance = hit.distance;
                    CointHitComponent cointHitComponent2 = hit.collider.GetComponent<CointHitComponent>();
                    if (bulletAttack.owner && cointHitComponent2.owner == bulletAttack.owner) cointHitComponent = cointHitComponent2;
                }
            if (cointHitComponent != null)
            {
                bulletAttack.maxDistance = distance;
                cointHitComponent.Hit(bulletAttack);
            }
        }
        private static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            //if (self.bodyIndex == Main.ProfessionalBodyIndex)
            //{
            //    self.moveSpeed *= self.sprintingSpeedMultiplier;
            //}
        }
        private static void BodyCatalog_SetBodyPrefabs(On.RoR2.BodyCatalog.orig_SetBodyPrefabs orig, UnityEngine.GameObject[] newBodyPrefabs)
        {
            orig(newBodyPrefabs);
            Main.ProfessionalBodyIndex = BodyCatalog.FindBodyIndex("ProfessionalBody");
        }
        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            
        }
        private static string Glyphs_GetGlyphString_MPEventSystem_string_AxisRange_InputSource(On.RoR2.Glyphs.orig_GetGlyphString_MPEventSystem_string_AxisRange_InputSource orig, RoR2.UI.MPEventSystem eventSystem, string actionName, Rewired.AxisRange axisRange, RoR2.UI.MPEventSystem.InputSource currentInputSource)
        {
            if (eventSystem)
            {
                if (eventSystem.localUser != null)
                {
                    if (eventSystem.localUser.currentNetworkUser != null)
                    {
                        CharacterBody characterBody = eventSystem.localUser.currentNetworkUser.GetCurrentBody();
                        if (characterBody)
                        {
                            string characterName = BodyCatalog.GetBodyName(characterBody.bodyIndex);
                            if (characterName != "ProfessionalBody") return orig(eventSystem, actionName, axisRange, currentInputSource);
                        }
                    }
                }
            }
            if (actionName == "UtilitySkill")
            {
                actionName = "Sprint";
            }else
            if (actionName == "Sprint")
            {
                actionName = "UtilitySkill";
            }
           return orig(eventSystem, actionName, axisRange, currentInputSource);
        }
    }
}

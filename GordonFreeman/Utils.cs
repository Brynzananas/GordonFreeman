using EntityStates;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using static GordonFreeman.ContentPacks;

namespace GordonFreeman
{
    public class Utils
    {
        public static T CreateSkill<T>(Type state, string activationState, Sprite sprite, string name, string nameToken, string descToken, string[] keywordTokens, int maxStocks, float rechargeInterval, bool beginSkillCooldownOnSkillEnd, bool canceledFromSprinting, bool cancelSprinting, bool fullRestockOnAssign, InterruptPriority interruptPriority, bool isCombat, bool mustKeyPress, int requiredStock, int rechargeStock, int stockToConsume, SkillFamily skillFamily, bool resetCooldownTimerOnUse = false) where T : SkillDef
        {
            GameObject commandoBodyPrefab = Assets.ProfessionalBody;

            SkillDef mySkillDef = ScriptableObject.CreateInstance<T>();
            mySkillDef.SetBonusStockMultiplier(maxStocks);
            //skillsBonusStocksMultiplier.Add(mySkillDef, maxStocks);
            mySkillDef.activationState = new SerializableEntityStateType(state);
            mySkillDef.activationStateMachineName = activationState;
            mySkillDef.baseMaxStock = maxStocks;
            mySkillDef.baseRechargeInterval = rechargeInterval;
            mySkillDef.beginSkillCooldownOnSkillEnd = beginSkillCooldownOnSkillEnd;
            mySkillDef.canceledFromSprinting = canceledFromSprinting;
            mySkillDef.cancelSprintingOnActivation = cancelSprinting;
            mySkillDef.fullRestockOnAssign = fullRestockOnAssign;
            mySkillDef.interruptPriority = interruptPriority;
            mySkillDef.isCombatSkill = isCombat;
            mySkillDef.mustKeyPress = mustKeyPress;
            mySkillDef.rechargeStock = rechargeStock;
            mySkillDef.requiredStock = requiredStock;
            mySkillDef.stockToConsume = stockToConsume;
            mySkillDef.icon = sprite;
            mySkillDef.skillDescriptionToken = descToken;
            mySkillDef.skillName = nameToken;
            mySkillDef.skillNameToken = nameToken;
            mySkillDef.keywordTokens = keywordTokens;
            mySkillDef.resetCooldownTimerOnUse = resetCooldownTimerOnUse;
            (mySkillDef as ScriptableObject).name = name;
            skills.Add(mySkillDef);
            if (skillFamily)
                AddSkillToFamily(ref skillFamily, mySkillDef);
            return mySkillDef as T;
        }
        public static void AddSkillToFamily(ref SkillFamily skillFamily, SkillDef skillDef)
        {
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(skillDef.skillNameToken, false, null)
            };
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SuperRoll(float chance)
        {
            int rolls = (int)MathF.Floor(chance / 100);
            if (Util.CheckRoll(chance - (rolls * 100))) rolls++;
            return rolls;
        }

    }
    public static class Extensions
    {
        public static bool MatchAny(this Instruction instr, out Instruction instruction)
        {
            instruction = instr;
            return true;
        }
        public static bool MatchLdargReturn(this Instruction instr, int value, out ILLabel illabel)
        {
            if (instr.MatchLdarg(out var value2))
            {
                illabel = (ILLabel)instr.Operand;
                return value2 == value;
            }
            illabel = null;
            return false;
        }
        public static bool MatchLdcI4Return(this Instruction instr, int value, out ILLabel illabel)
        {
            if (instr.MatchLdcI4(out var value2))
            {
                illabel = (ILLabel)instr.Operand;
                return value2 == value;
            }
            illabel = null;
            return false;
        }
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        }
        public static T GetOrAddComponent<T>(this Transform transform) where T : Component
        {
            return transform.gameObject.GetOrAddComponent<T>();
        }
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.GetOrAddComponent<T>();
        }
        public static T GetOrAddComponent<T>(this GameObject gameObject, out bool wasAdded) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component)
            {
                wasAdded = false;
            }
            else
            {
                wasAdded = true;
            }
            return component ?? gameObject.AddComponent<T>();
        }
        public static void AddPersistentListener(this UnityEvent unityEvent, UnityAction action)
        {
            if (unityEvent.m_PersistentCalls == null) unityEvent.m_PersistentCalls = new PersistentCallGroup();
            unityEvent.m_PersistentCalls.AddListener(new PersistentCall
            {
                m_Target = action.Target as UnityEngine.Object,
                m_TargetAssemblyTypeName = UnityEventTools.TidyAssemblyTypeName(action.Method.DeclaringType.AssemblyQualifiedName),
                m_MethodName = action.Method.Name,
                m_CallState = UnityEventCallState.RuntimeOnly,
                m_Mode = PersistentListenerMode.EventDefined,
            });
        }
        public static void AddPersistentListener<T1, T2>(this UnityEvent<T1, T2> unityEvent, UnityAction<T1, T2> action)
        {
            if (unityEvent.m_PersistentCalls == null) unityEvent.m_PersistentCalls = new PersistentCallGroup();
            unityEvent.m_PersistentCalls.AddListener(new PersistentCall
            {
                m_Target = action.Target as UnityEngine.Object,
                m_TargetAssemblyTypeName = UnityEventTools.TidyAssemblyTypeName(action.Method.DeclaringType.AssemblyQualifiedName),
                m_MethodName = action.Method.Name,
                m_CallState = UnityEventCallState.RuntimeOnly,
                m_Mode = PersistentListenerMode.EventDefined,
            });
        }
    }
}

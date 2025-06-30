using BrynzaAPI;
using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using R2API;
using Rewired;
using RoR2;
using RoR2.HudOverlay;
using RoR2.Projectile;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using static BrynzaAPI.SniperHurtboxTracker;
using static GordonFreeman.Assets;
using static GordonFreeman.FireHook;
using static Rewired.ComponentControls.Effects.RotateAroundAxis;

namespace GordonFreeman
{
    public class BaseFreemanState : BaseSkillState
    {
        public ProfessionalCharacterComponent professionalCharacterComponent;
        public ProfessionalBodyComponent professionalBodyComponent;
        public ProfessionalCharacterMain professionalCharacterMain;
        public GameObject weaponObject { get { return professionalCharacterComponent && professionalCharacterComponent.weaponObject ? professionalCharacterComponent.weaponObject : gameObject; } }
        public Transform muzzleTransform { get { return professionalCharacterComponent?.modelComponent?.childLocator?.FindChild("Muzzle") ?? weaponObject.transform; } }
        public override void OnEnter()
        {
            base.OnEnter();
            professionalBodyComponent = characterBody && (characterBody is ProfessionalBodyComponent) ? (characterBody as ProfessionalBodyComponent) : null;
            professionalCharacterComponent = professionalBodyComponent ? professionalBodyComponent.professionalCharacterComponent : null;
            if (professionalBodyComponent) professionalCharacterMain = professionalBodyComponent.professionalCharacterMain;
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
    public abstract class FreemanBulletAttack : BaseFreemanState
    {
        public abstract GameObject muzzleEffectPrefab { get; }
        public abstract GameObject tracerEffectPrefab { get; }
        public abstract GameObject hitEffectPrefab { get; }
        public abstract GameObject weapon { get; }
        public abstract string fireSoundString { get; }
        public abstract float minVerticalRecoil { get; }
        public abstract float maxVerticalRecoil { get; }
        public abstract float minHorizontalRecoil { get; }
        public abstract float maxHorizontalRecoil { get; }
        public abstract float recoilAmplitude { get; }
        public abstract float trajectoryAimAssistMultiplier { get; }
        public abstract float damageCoefficient { get; }
        public abstract float procCoefficient { get; }
        public abstract DamageTypeCombo damageType { get; }
        public abstract BulletAttack.FalloffModel falloff { get; }
        public abstract LayerMask hitMask { get; }
        public abstract LayerMask stopperMask { get; }
        public abstract float force { get; }
        public abstract float spreadBloomValue { get; }
        public abstract float radius { get; }
        public abstract float range { get; }
        public abstract int bulletCount { get; }
        public abstract float minSpread { get; }
        public abstract float maxSpread { get; }
        public abstract bool crit { get; }
        public virtual void FireBullet(Ray ray, string targetMuzzle)
        {
            targetMuzzle = "Muzzle";
            FireBulletStart(ray, targetMuzzle);
            if (base.isAuthority)
            {
                BulletAttack bulletAttack = new BulletAttack
                {
                    owner = base.gameObject,
                    weapon = gameObject,
                    origin = ray.origin,
                    aimVector = ray.direction,
                    minSpread = minSpread,
                    maxSpread = maxSpread,
                    damage = damageCoefficient * this.damageStat,
                    force = force,
                    tracerEffectPrefab = tracerEffectPrefab,
                    muzzleName = targetMuzzle,
                    hitEffectPrefab = hitEffectPrefab,
                    isCrit = crit,
                    radius = radius,
                    hitMask = hitMask,
                    stopperMask = stopperMask,
                    maxDistance = range,
                    procCoefficient = procCoefficient,
                    bulletCount = (uint)bulletCount,
                    smartCollision = true,
                    falloffModel = falloff,
                    trajectoryAimAssistMultiplier = trajectoryAimAssistMultiplier,
                    damageType = damageType
                };
                //bulletAttack.SetWeaponOverride(weaponObject);
                ModifyBulletAttack(ref bulletAttack);
                bulletAttack.Fire();
            }
            base.characterBody.AddSpreadBloom(spreadBloomValue);
        }
        public virtual void FireBulletStart(Ray ray, string targetMuzzle)
        {
            if (fireSoundString != "")
                Util.PlaySound(fireSoundString, base.gameObject);
            if (muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, targetMuzzle, false);
            }
            base.AddRecoil(minVerticalRecoil * recoilAmplitude, maxVerticalRecoil * recoilAmplitude, minHorizontalRecoil * recoilAmplitude, maxHorizontalRecoil * recoilAmplitude);
        }
        
        public virtual void ModifyBulletAttack(ref BulletAttack bulletAttack)
        {

        }
    }
    public abstract class FreemanFireProjectile : BaseFreemanState
    {
        public abstract GameObject projectilePrefab { get; }
        public abstract GameObject muzzleEffectPrefab { get; }
        public abstract string fireProjectileSound { get; }
        public abstract float damageCoefficient { get; }
        public abstract DamageTypeCombo damageType { get; }
        public abstract float spreadBloomValue { get; }
        public abstract float force { get; }
        public virtual void FireProjectile(Ray ray, string targetMuzzle)
        {
            if(fireProjectileSound != "")
            Util.PlaySound(fireProjectileSound, base.gameObject);
            if (muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, targetMuzzle, false);
            }
            if (base.isAuthority)
            {
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = this.projectilePrefab,
                    position = ray.origin,
                    rotation = Util.QuaternionSafeLookRotation(ray.direction),
                    owner = base.gameObject,
                    damage = this.damageCoefficient * this.damageStat,
                    force = force,
                    crit = Util.CheckRoll(this.critStat, base.characterBody.master),
                    damageTypeOverride = new DamageTypeCombo?(damageType)
                };
                this.ModifyProjectileInfo(ref fireProjectileInfo);
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
            base.characterBody.AddSpreadBloom(spreadBloomValue);
        }

        public virtual void ModifyProjectileInfo(ref FireProjectileInfo fireProjectileInfo)
        {
            
        }
    }
    public abstract class FreemanScope : BaseFreemanState
    {
        public abstract bool toExecute { get; }
        public abstract Type stateToEnter { get; }
        public abstract GameObject crosshairOverrideObject { get; }
        public abstract GameObject scopeOverlayObject { get; }
        public abstract GameObject sniperTargetObject { get; }
        public OverlayController overlayController;
        public CrosshairUtils.OverrideRequest overrideRequest;
        public BrynzaAPI.SniperHurtboxTracker.SniperHurtboxTrackerParams sniperHurtboxTrackerParams;
        public override void OnEnter()
        {
            base.OnEnter();
            if(scopeOverlayObject)
            this.overlayController = HudOverlayManager.AddOverlay(base.gameObject, new OverlayCreationParams
            {
                prefab = this.scopeOverlayObject,
                childLocatorEntry = "ScopeContainer"
            });
            if (crosshairOverrideObject)
            {
                overrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, this.crosshairOverrideObject, CrosshairUtils.OverridePriority.Skill);
            }
            if (isAuthority)
            {
                sniperHurtboxTrackerParams = new SniperHurtboxTrackerParams
                {
                    trackerPrefab = sniperTargetObject,
                    priority = 2
                };
                BrynzaAPI.Utils.AddSniperHurtboxTracker(sniperHurtboxTrackerParams);
            }
        }
        public override void OnExit()
        {
            this.RemoveOverlay();
            if (overrideRequest != null)
                overrideRequest.Dispose();
            if (isAuthority)
            {
                BrynzaAPI.Utils.RemoveSniperHurtboxTracker(sniperHurtboxTrackerParams);
            }
            base.OnExit();
        }
        protected void SetScopeAlpha(float alpha)
        {
            if (this.overlayController != null)
            {
                this.overlayController.alpha = alpha;
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                if (toExecute)
                {
                    outer.SetState(EntityStateCatalog.InstantiateState(stateToEnter));
                }
                if (!base.IsKeyDownAuthority())
                {
                    this.outer.SetNextStateToMain();
                }
                
            }
        }
        protected void RemoveOverlay()
        {
            if (this.overlayController != null)
            {
                HudOverlayManager.RemoveOverlay(this.overlayController);
                this.overlayController = null;
            }
        }
    }
    public class FirePistols : FreemanBulletAttack, SteppedSkillDef.IStepSetter
    {
        public override GameObject muzzleEffectPrefab => FirePistol2.muzzleEffectPrefab;
        public override GameObject tracerEffectPrefab => FirePistol2.tracerEffectPrefab;
        public override GameObject hitEffectPrefab => FirePistol2.hitEffectPrefab;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => PistolFire.playSoundString;
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => FirePistol2.recoilAmplitude;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 1.25f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericPrimary;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.DefaultBullet;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => FirePistol2.spreadBloomValue;
        public override float radius => 0.1f;
        public override int bulletCount => 1;
        public override float minSpread => 0f;
        public override float maxSpread => 0f;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public override float procCoefficient => 1f;
        public override float range => 1024f;
        public override LayerMask hitMask => LayerIndex.entityPrecise.mask;
        public override LayerMask stopperMask => LayerIndex.entityPrecise.mask + LayerIndex.world.mask;

        public int step;
        public float duration;

        public void SetStep(int i)
        {
            step = i;
        }
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = FirePistol2.baseDuration / this.attackSpeedStat;
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            if (this.step % 2 == 0)
            {
                this.PlayAnimation("Gesture Additive, Left", FirePistol2.FirePistolLeftStateHash);
                this.FireBullet(ray, "MuzzleLeft");
                return;
            }
            this.PlayAnimation("Gesture Additive, Right", FirePistol2.FirePistolRightStateHash);
            this.FireBullet(ray, "MuzzleRight");
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge < this.duration || !base.isAuthority)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class FirePistolsBarrage : FreemanBulletAttack, SteppedSkillDef.IStepSetter
    {
        public override GameObject muzzleEffectPrefab => FirePistol2.muzzleEffectPrefab;
        public override GameObject tracerEffectPrefab => FirePistol2.tracerEffectPrefab;
        public override GameObject hitEffectPrefab => FirePistol2.hitEffectPrefab;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => PistolFire.playSoundString;
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => FirePistol2.recoilAmplitude;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 1.25f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericPrimary;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.DefaultBullet;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => FirePistol2.spreadBloomValue;
        public override float radius => 0.1f;
        public override int bulletCount => 1;
        public override float minSpread => 0f;
        public override float maxSpread => 0f;
        public override float procCoefficient => 1f;
        public override float range => 1024f;
        public override LayerMask hitMask => LayerIndex.entityPrecise.mask;
        public override LayerMask stopperMask => LayerIndex.entityPrecise.mask + LayerIndex.world.mask;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public int step;
        public float duration;
        public float stopwatch;
        public static float fullDuration = 1f;


        public void SetStep(int i)
        {
            step = i;
        }
        public void Fire()
        {
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            if (this.step % 2 == 0)
            {
                this.PlayAnimation("Gesture Additive, Left", FirePistol2.FirePistolLeftStateHash);
                this.FireBullet(ray, "MuzzleLeft");
                return;
            }
            this.PlayAnimation("Gesture Additive, Right", FirePistol2.FirePistolRightStateHash);
            this.FireBullet(ray, "MuzzleRight");
        }
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = FirePistol2.baseDuration / this.attackSpeedStat / 4;
            
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch -= Time.fixedDeltaTime;
            if (stopwatch <= 0f)
            {
                Fire();
                stopwatch = duration;
            }
            if (base.fixedAge < fullDuration || !base.isAuthority)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class FireRevolver : FreemanBulletAttack
    {
        public override GameObject muzzleEffectPrefab => BanditRevolverMuzzleFlash;
        public override GameObject tracerEffectPrefab => BanditRevolverTracer;
        public override GameObject hitEffectPrefab => BanditRevolverHit;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => RevolverFire.playSoundString;
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => FirePistol2.recoilAmplitude;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 2f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericPrimary;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.None;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => FirePistol2.spreadBloomValue;
        public override float radius => 0.1f;
        public override int bulletCount => 1;
        public override float minSpread => 0f;
        public override float maxSpread => 0f;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public override float procCoefficient => 1f;
        public override float range => 1024f;
        public override LayerMask hitMask => LayerIndex.entityPrecise.mask;
        public override LayerMask stopperMask => LayerIndex.entityPrecise.mask + LayerIndex.world.mask;
        public float duration;
        public static float baseDuration = 0.4f;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            this.FireBullet(ray, "");
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge < this.duration || !base.isAuthority)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class FireCoin : FreemanFireProjectile
    {
        public override GameObject projectilePrefab => CoinProjectile;
        public override GameObject muzzleEffectPrefab => null;
        public override string fireProjectileSound => "";
        public override float damageCoefficient => 0f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericSecondary;
        public override float spreadBloomValue => 0f;
        public override float force => 0f;
        public override void OnEnter()
        {
            base.OnEnter();
            Ray ray = GetAimRay();
            FireProjectile(ray, "");
            if(isAuthority) outer.SetNextStateToMain();
        }
    }
    public class FireSMG : FreemanBulletAttack
    {
        public override GameObject muzzleEffectPrefab => FirePistol2.muzzleEffectPrefab;
        public override GameObject tracerEffectPrefab => FirePistol2.tracerEffectPrefab;
        public override GameObject hitEffectPrefab => FirePistol2.hitEffectPrefab;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => SMGFire.playSoundString;
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => FirePistol2.recoilAmplitude;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 1f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericPrimary;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.DefaultBullet;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => FirePistol2.spreadBloomValue;
        public override float radius => 0.1f;
        public override int bulletCount => Utils.SuperRoll(characterBody.attackSpeed * 100);
        public override float minSpread => 0f;
        public override float maxSpread => 0f + characterBody.spreadBloomAngle;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public override float procCoefficient => 1f;
        public override float range => 1024f;
        public override LayerMask hitMask => LayerIndex.entityPrecise.mask;
        public override LayerMask stopperMask => LayerIndex.entityPrecise.mask + LayerIndex.world.mask;
        public static float baseDuration = 0.05f;
        public float stopwatch = 0f;
        public int step;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch -= Time.fixedDeltaTime;
            if (stopwatch <= 0f)
            {
                Ray ray = GetAimRay();
                FireBullet(ray, "MuzzleRight");
                if(isAuthority)
                activatorSkillSlot.stock--;
                stopwatch = duration;
            }
            if (!base.isAuthority || IsKeyDownAuthority() && activatorSkillSlot.stock > 0)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class FireSMGGrenade : FreemanFireProjectile
    {
        public override GameObject projectilePrefab => SMGGrenadeProjectile;
        public override GameObject muzzleEffectPrefab => null;
        public override string fireProjectileSound => "Play_MULT_m1_grenade_launcher_shoot";
        public override float damageCoefficient => 10f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericSecondary;
        public override float spreadBloomValue => 0f;
        public override float force => 0f;
        public override void OnEnter()
        {
            base.OnEnter();
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            FireProjectile(ray, "");
            if (isAuthority) outer.SetNextStateToMain();
        }
    }
    public class FireShotgunBase : FreemanBulletAttack
    {
        public override GameObject muzzleEffectPrefab => CaptainShotgunMuzzleFlash;
        public override GameObject tracerEffectPrefab => CaptainShotgunTracer;
        public override GameObject hitEffectPrefab => CaptainShotgunHit;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => ShotgunFire.playSoundString;
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => FirePistol2.recoilAmplitude;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 1f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericPrimary;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.DefaultBullet;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => FirePistol2.spreadBloomValue;
        public override float radius => 0.1f;
        public override int bulletCount => 8;
        public override float minSpread => 0f;
        public override float maxSpread => 3f;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public override float procCoefficient => 1f;
        public override float range => 1024f;
        public override LayerMask hitMask => LayerIndex.entityPrecise.mask + LayerIndex.world.mask;
        public override LayerMask stopperMask => LayerIndex.entityPrecise.mask + LayerIndex.world.mask;
        public float baseDuration { get { return bulletCount * 0.05f; } }
        public int step;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            this.FireBullet(ray, "MuzzleRight");
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge < this.duration || !base.isAuthority)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class FireShotgunBurst : FreemanBulletAttack
    {
        public override GameObject muzzleEffectPrefab => CaptainShotgunMuzzleFlash;
        public override GameObject tracerEffectPrefab => CaptainShotgunTracer;
        public override GameObject hitEffectPrefab => CaptainShotgunHit;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => ShotgunFire.playSoundString;
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => FirePistol2.recoilAmplitude;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 1.25f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericPrimary;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.Buckshot;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => FirePistol2.spreadBloomValue;
        public override float radius => 0.1f;
        public override int bulletCount => 14;
        public override float minSpread => 0f;
        public override float maxSpread => 3f;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public override float procCoefficient => 1f;
        public override float range => 1024f;
        public override LayerMask hitMask => LayerIndex.entityPrecise.mask;
        public override LayerMask stopperMask => LayerIndex.entityPrecise.mask + LayerIndex.world.mask;
        public float baseDuration { get { return bulletCount * 0.05f; } }
        public int step;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            this.FireBullet(ray, "MuzzleRight");
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge < this.duration || !base.isAuthority)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class FireDart : FreemanFireProjectile
    {
        public override GameObject projectilePrefab => DartProjectile;
        public override GameObject muzzleEffectPrefab => null;
        public override string fireProjectileSound => CrossbowFire.playSoundString;
        public override float damageCoefficient => 1.2f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericPrimary;
        public override float spreadBloomValue => 0f;
        public override float force => 0f;
        public static float baseDuration = 0.3f;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            FireProjectile(ray, "");
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge < this.duration || !base.isAuthority)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class ScopeRebar : FreemanScope
    {
        public override bool toExecute => inputBank && inputBank.skill1.justPressed;
        public override GameObject crosshairOverrideObject => null;
        public override GameObject scopeOverlayObject => null;
        public override GameObject sniperTargetObject => Assets.SniperTargetTest;
        public override Type stateToEnter => typeof(FireRebar);
    }
    public class FireRebar : FreemanFireProjectile
    {
        public override GameObject projectilePrefab => RebarProjectile;
        public override GameObject muzzleEffectPrefab => null;
        public override string fireProjectileSound => CrossbowFire.playSoundString;
        public override float damageCoefficient => 5f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericSecondary;
        public override float spreadBloomValue => 0f;
        public override float force => 0f;
        public static float baseDuration = 1f;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            FireProjectile(ray, "");
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge < this.duration || !base.isAuthority)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class FireTau : FreemanBulletAttack
    {
        public override GameObject muzzleEffectPrefab => CaptainShotgunMuzzleFlash;
        public override GameObject tracerEffectPrefab => MultRebarTracer;
        public override GameObject hitEffectPrefab => CaptainShotgunHit;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => TauCannonFire.playSoundString;
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => FirePistol2.recoilAmplitude;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 3f * charges;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericPrimary;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.None;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => FirePistol2.spreadBloomValue;
        public override float radius => 1f;
        public override int bulletCount => 1;
        public override float minSpread => 0f;
        public override float maxSpread => 0f;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public override float procCoefficient => 1f;
        public override float range => 1024f;
        public override LayerMask hitMask => LayerIndex.entityPrecise.mask + LayerIndex.projectile.mask;
        public override LayerMask stopperMask => LayerIndex.entityPrecise.mask + LayerIndex.world.mask;
        public float charge = 0f;
        public static float baseDuration = 0.2f;
        public float duration;
        public float damage;
        public int charges { get
            {
                SteppedSkillDef.InstanceData steppedInstance = skillLocator && skillLocator.secondary && skillLocator.secondary.skillInstanceData is SteppedSkillDef.InstanceData ? skillLocator.secondary.skillInstanceData as SteppedSkillDef.InstanceData : null;
                if (steppedInstance != null)
                {
                    return 1 + steppedInstance.step;
                }
                return 1;
            }
            set
            {
                SteppedSkillDef.InstanceData steppedInstance = skillLocator && skillLocator.secondary && skillLocator.secondary.skillInstanceData is SteppedSkillDef.InstanceData ? skillLocator.secondary.skillInstanceData as SteppedSkillDef.InstanceData : null;
                if (steppedInstance != null)
                {
                    steppedInstance.step = value;
                }
            }
        }
        public override void OnEnter()
        {
            base.OnEnter();
            damage = damageCoefficient * damageStat;
            this.duration = baseDuration / this.attackSpeedStat;
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            this.FireBullet(ray, "");
            if (characterMotor && isAuthority)
                characterMotor.velocity += ray.direction * -damageCoefficient;
            charges = 0;
        }
        public override void ModifyBulletAttack(ref BulletAttack bulletAttack)
        {
            base.ModifyBulletAttack(ref bulletAttack);
            if (!bulletAttack.hitCallback.GetInvocationList().Contains(ExplodeProjectiles))
            {
                bulletAttack.hitCallback += ExplodeProjectiles;
            }
            if (bulletAttack.hitCallback.GetInvocationList().Contains(Reflect))
            {
                bulletAttack.hitCallback -= Reflect;
            }
            else
            {
                bulletAttack.hitCallback += Reflect;
            }
        }
        private bool Reflect(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            if(hitInfo.collider.gameObject.layer == LayerIndex.world.intVal)
            {
                Vector3 vector3 = Vector3.Reflect(bulletAttack.aimVector, hitInfo.surfaceNormal);
                Ray ray = new Ray { direction = vector3, origin = hitInfo.point };
                this.FireBullet(ray, "");
            }
            return false;
        }
        private bool ExplodeProjectiles(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            ProjectileController projectileController = hitInfo.collider.GetComponent<ProjectileController>();
            if (projectileController)
            {
                ProjectileDamage projectileDamage = projectileController.GetComponent<ProjectileDamage>();
                if (isAuthority)
                {
                    BlastAttack blastAttack = new BlastAttack
                    {
                        attacker = gameObject,
                        attackerFiltering = AttackerFiltering.Default,
                        baseDamage = projectileDamage.damage + damage,
                        crit = projectileDamage ? projectileDamage.crit : false || bulletAttack.isCrit,
                        damageColorIndex = DamageColorIndex.Default,
                        damageType = projectileDamage ? projectileDamage.damageType.damageSource = DamageSource.Primary : DamageTypeCombo.GenericPrimary,
                        inflictor = hitInfo.collider.gameObject,
                        losType = BlastAttack.LoSType.None,
                        position = hitInfo.point,
                        falloffModel = BlastAttack.FalloffModel.None,
                        radius = projectileController.rigidbody ? projectileController.rigidbody.velocity.magnitude / 2f: 12f,
                        teamIndex = teamComponent ? teamComponent.teamIndex : TeamIndex.None,
                        procCoefficient = 1f,
                    };
                    blastAttack.Fire();
                }
                if (NetworkServer.active)
                {
                    HealthComponent healthComponent = projectileController.owner ? projectileController.owner.GetComponent<HealthComponent>() : null;
                    DamageInfo damageInfo = new DamageInfo
                    {
                        attacker = gameObject,
                        canRejectForce = true,
                        crit = projectileDamage ? projectileDamage.crit : false || bulletAttack.isCrit,
                        damage = projectileDamage.damage + damage,
                        damageColorIndex = projectileDamage.damageColorIndex,
                        damageType = projectileDamage.damageType,
                        inflictor = projectileController.gameObject,
                        position = projectileController.transform.position,
                        procCoefficient = 1f,
                    };
                    DamageReport damageReport = new DamageReport(damageInfo, healthComponent, damageInfo.damage, healthComponent.combinedHealth);
                    if(damageInfo.attacker != healthComponent.gameObject)
                    GlobalEventManager.instance.OnCharacterDeath(damageReport);
                }
                EffectData effectData = new EffectData
                {
                    origin = hitInfo.point,
                    scale = projectileController.rigidbody ? projectileController.rigidbody.velocity.magnitude * 2 : 12f,
                };
                EffectManager.SpawnEffect(Explosion, effectData, true);
                ProjectileExplosion projectileExplosion = projectileController.GetComponent<ProjectileExplosion>();
                if (projectileExplosion)
                {
                    Destroy(projectileExplosion);
                }
                Destroy(projectileController.gameObject);
            }
            return false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge < this.duration || !base.isAuthority)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class ChargeTau : BaseFreemanState
    {
        public float currentCharge = 0f;
        public int charge;
        public float stopwatch = 0f;
        public static float baseMaxChargeTime = 4f;
        public static float baseMaxFullChargeTime = 10f;
        public static int chargePerSecond = 5;
        public override void OnEnter()
        {
            base.OnEnter();
            Util.PlaySound(TauCannonCharge.playSoundString, gameObject);
        }
        public override void OnExit()
        {
            base.OnExit();
            Util.PlaySound(TauCannonCharge.stopSoundString, gameObject);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            currentCharge += Time.fixedDeltaTime;
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch < baseMaxChargeTime && currentCharge > 1f / (float)chargePerSecond / characterBody.attackSpeed)
            {
                if (isAuthority)
                {
                    SteppedSkillDef.InstanceData steppedInstance = activatorSkillSlot && activatorSkillSlot.skillInstanceData is SteppedSkillDef.InstanceData ? activatorSkillSlot.skillInstanceData as SteppedSkillDef.InstanceData : null;
                    if (steppedInstance != null)
                    {
                        steppedInstance.step++;
                        if (skillLocator && skillLocator.primary)
                        {
                            skillLocator.primary.stock--;
                        }
                        charge++;
                    }
                }
                currentCharge = 0f;
            }
            bool shoot = skillLocator && skillLocator.primary && skillLocator.primary.stock > 0;    
            if (!base.isAuthority || IsKeyDownAuthority() && stopwatch < baseMaxFullChargeTime && shoot)
            {
                return;
            }
            this.outer.SetNextState(new FireTau { charge = currentCharge });


        }
    }
    public class FireRocket : FreemanFireProjectile
    {
        public override GameObject projectilePrefab => isHoming ? HomingRocketProjectile : RocketProjectile;
        public override GameObject muzzleEffectPrefab => null;
        public override string fireProjectileSound => RocketFire.playSoundString;
        public override float damageCoefficient => 40f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericPrimary;
        public override float spreadBloomValue => 0f;
        public override float force => 0f;
        public static float baseDuration = 1f;
        public float duration;
        public bool isHoming;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            isHoming = true;
            GenericSkill genericSkill = skillLocator ? skillLocator.secondary : null;
            if (genericSkill != null && genericSkill.skillInstanceData != null && genericSkill.skillInstanceData is SteppedSkillDef.InstanceData)
            {
                SteppedSkillDef.InstanceData instanceData = (SteppedSkillDef.InstanceData)genericSkill.skillInstanceData;
                isHoming = instanceData.step == 0;
            }
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            FireProjectile(ray, "");
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge < this.duration || !base.isAuthority)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class SwitchRocketState : BaseFreemanState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if(isAuthority)
            outer.SetNextStateToMain();
        }
    }
    public class FreemanDash : BaseFreemanState
    {
        public static float baseDuration = 0.125f;
        public static float baseSpeed = 96f;
        public float duration;
        public float speed;
        public Vector3 direction = Vector3.zero;
        public HurtBoxGroup hurtBoxGroup;
        private bool isDashing = false;
        public override void OnEnter()
        {
            base.OnEnter();
            bool noMoveInput = false;
            direction = inputBank ? inputBank.moveVector : (characterMotor ? characterMotor.moveDirection : (rigidbody ? rigidbody.velocity.normalized : transform.eulerAngles));
            if (direction == Vector3.zero)
            {
                noMoveInput = true;
                direction = inputBank ? inputBank.aimDirection : transform.forward;
            }
            if (noMoveInput)
            {
                outer.SetStateToMain();
                return;
            }
            direction = direction.normalized;
            Util.PlaySound(EntityStates.Commando.DodgeState.dodgeSoundString, base.gameObject);
            speed = baseSpeed;
            duration = baseDuration;
            hurtBoxGroup = GetModelTransform() ? GetModelTransform().GetComponent<HurtBoxGroup>() : null;
            if (hurtBoxGroup != null)
                hurtBoxGroup.hurtBoxesDeactivatorCounter++;
            isDashing = true;
        }
        public override void OnExit()
        {
            base.OnExit();
            if (isDashing && isAuthority) 
                if (characterMotor)
                {
                    characterMotor.SetVelocityOverride(Vector3.zero);
                    characterMotor.velocity = direction * moveSpeedStat;
                    characterMotor.disableAirControlUntilCollision = false;
                }
                else if (rigidbody)
                {
                    rigidbody.velocity = direction * moveSpeedStat;
                } 
            if (hurtBoxGroup != null)
                hurtBoxGroup.hurtBoxesDeactivatorCounter--;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (isAuthority)
                if (characterMotor)
                {
                    characterMotor.SetVelocityOverride(direction * speed);
                }
                else if (rigidbody)
                {
                    rigidbody.velocity = direction * speed;
                }
            if (base.fixedAge < this.duration || !base.isAuthority)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }
    public class FreemanSlam : BaseFreemanState
    {
        public static float slamRadius = 24f;
        public static float slamSpeed = 3f;
        public static float afterSlamTime = 2f;
        public float slamTime;
        public float slamDistance = 10f;
        public float slamForce => (slamDistance / afterSlamTime) - (Physics.gravity.y * afterSlamTime / 2f);
        public bool removeBuff = false;
        public override void OnEnter()
        {
            base.OnEnter();
            if (isAuthority)
            {
                if (characterMotor)
                {
                    if (characterMotor.isGrounded)
                    {
                        outer.SetStateToMain();
                        return;
                    }
                    characterMotor.onHitGroundAuthority += CharacterMotor_onHitGroundAuthority;
                    if (Physics.Raycast(characterBody.footPosition, new Vector3(0f, Physics.gravity.y, 0f), out var hitInfo, 99999f, LayerIndex.world.mask + LayerIndex.defaultLayer.mask + LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
                    {
                        slamDistance = hitInfo.distance;
                    }

                }
                else
                {
                    outer.SetStateToMain();
                    return;
                }
                characterBody.AddBuffAuthotiry(JunkContent.Buffs.IgnoreFallDamage);
                removeBuff = true;
            }
                
        }
        private void CharacterMotor_onHitGroundAuthority(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            Collider[] colliders = Physics.OverlapSphere(characterBody.footPosition, slamRadius, LayerIndex.CommonMasks.characterBodies + LayerIndex.CommonMasks.fakeActorLayers, QueryTriggerInteraction.UseGlobal);
            List<CharacterBody> list = new List<CharacterBody>();
            foreach (Collider collider in colliders)
            {
                CharacterBody characterBody = collider.GetComponent<CharacterBody>();
                if (characterBody == null || list.Contains(characterBody)) continue;
                if (characterBody.characterMotor)
                {
                    characterBody.characterMotor.Motor?.ForceUnground(0f);
                    characterBody.characterMotor.velocity.y = Trajectory.CalculateInitialYSpeed(afterSlamTime, slamDistance);
                    //characterBody.characterMotor.ApplyForce(Physics.gravity.normalized * -1f * slamForce, true, true);
                }
                list.Add(characterBody);
            }
            outer.SetNextStateToMain();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            {
                Vector3 vector3 = Physics.gravity * 3f;
                slamTime += Time.fixedDeltaTime;
                if (isAuthority)
                {
                    if (characterMotor)
                    {
                        characterMotor.SetVelocityOverride(vector3);
                    }
                    else if (rigidbody)
                    {
                        rigidbody.velocity = vector3;
                    }
                } 
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (isAuthority)
            {
                if (characterMotor)
                {
                    characterMotor.SetVelocityOverride(Vector3.zero);
                    characterMotor.disableAirControlUntilCollision = false;
                    characterMotor.onHitGroundAuthority -= CharacterMotor_onHitGroundAuthority;
                }
                else if (rigidbody)
                {
                    rigidbody.velocity = Vector3.zero;
                }
                if(removeBuff)
                characterBody.RemoveBuffAuthotiry(JunkContent.Buffs.IgnoreFallDamage);
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }
    public class FireGluon : FreemanBulletAttack
    {
        public override GameObject muzzleEffectPrefab => null;
        public override GameObject tracerEffectPrefab => null;
        public override GameObject hitEffectPrefab => null;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => "";
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => 0f;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 0.5f * characterBody.attackSpeed;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericPrimary;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.None;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => 0f;
        public override float radius => 1f;
        public override int bulletCount => 1;
        public override float minSpread => 0f;
        public override float maxSpread => 0f;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public override float procCoefficient => 1f;
        public override LayerMask hitMask => LayerIndex.entityPrecise.mask + LayerIndex.projectile.mask;
        public override LayerMask stopperMask => LayerIndex.entityPrecise.mask + LayerIndex.world.mask;
        public override float range => 32f;
        public static int stockConsumptionPerSecond = 30;
        public static int chargeProjectilePerSecond = 5;
        public float duration;
        public float stopwatch;
        public GameObject laser;
        public ChildLocator childLocator;
        public Transform startTransform;
        public Transform endTransform;
        public List<Collider> colliders = new List<Collider>();
        public Dictionary<Collider, ProjectileController> keyValuePairs = new Dictionary<Collider, ProjectileController>();
        public override void OnEnter()
        {
            base.OnEnter();
            duration = 1f / stockConsumptionPerSecond;
            stopwatch = duration;
            laser = GameObject.Instantiate(GluonBeamEffect);
            childLocator = laser.GetComponent<ChildLocator>();
            startTransform = childLocator.FindChild("Start");
            startTransform.SetParent(muzzleTransform, false);
            endTransform = childLocator.FindChild("End");
            Util.PlaySound(GluonGunFire.playSoundString, gameObject);
        }
        public override void OnExit()
        {
            base.OnExit();
            if(laser) Destroy(laser);
            if (startTransform) Destroy(startTransform.gameObject);
            Util.PlaySound(GluonGunFire.stopSoundString, gameObject);
            Util.PlaySound(GluonGunOff.playSoundString, gameObject);
        }
        public override void Update()
        {
            base.Update();
            RaycastHit raycastHit;
            Ray ray = GetAimRay();
            endTransform.position = ray.origin + ray.direction * range;
            //if (Physics.Raycast(ray, out raycastHit, range, LayerIndex.world.mask + LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
            //{
            //    endTransform.position = raycastHit.point;
            //}
            //else
            //{
            //    endTransform.position = ray.origin + ray.direction * range;
            //}
        }
        public override void ModifyBulletAttack(ref BulletAttack bulletAttack)
        {
            base.ModifyBulletAttack(ref bulletAttack);
            bulletAttack.hitCallback = sus;
        }

        private bool sus(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            if (colliders.Contains(hitInfo.collider))
            {
                return false;
            }
            else
            {
                colliders.Add(hitInfo.collider);
            }
            if (keyValuePairs.ContainsKey(hitInfo.collider))
            {
                ProjectileController projectileController = keyValuePairs[hitInfo.collider];
                if (projectileController)
                {
                    ProjectileOverchargeComponent projectileOverchargeComponent = projectileController.gameObject.GetOrAddComponent<ProjectileOverchargeComponent>(out var wasAdded);
                    if (wasAdded)
                    {
                        AddOvercharge(projectileController, projectileOverchargeComponent, ref hitInfo);
                    }
                    projectileOverchargeComponent.charge += 1f / (float)chargeProjectilePerSecond;
                }
            }
            else
            {
                ProjectileController projectileController = hitInfo.collider.gameObject.GetComponent<ProjectileController>();
                if (projectileController != null)
                {
                    ProjectileOverchargeComponent projectileOverchargeComponent = projectileController.gameObject.GetOrAddComponent<ProjectileOverchargeComponent>();
                    AddOvercharge(projectileController, projectileOverchargeComponent, ref hitInfo);
                }
                keyValuePairs.Add(hitInfo.collider, projectileController);
            }
            void AddOvercharge(ProjectileController projectileController, ProjectileOverchargeComponent projectileOverchargeComponent, ref BulletAttack.BulletHit hitInfo)
            {
                projectileOverchargeComponent.controller = projectileController;
                projectileOverchargeComponent.entityStateMachine = outer;
                projectileOverchargeComponent.entityState = outer.state;
                ProjectileDamage projectileDamage = projectileController.GetComponent<ProjectileDamage>();
                BlastAttack blastAttack = new BlastAttack
                {
                    attacker = gameObject,
                    attackerFiltering = AttackerFiltering.Default,
                    baseDamage = damageStat,
                    crit = projectileDamage ? projectileDamage.crit : false || bulletAttack.isCrit,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = projectileDamage ? projectileDamage.damageType.damageSource = DamageSource.Primary : DamageTypeCombo.GenericPrimary,
                    inflictor = hitInfo.collider.gameObject,
                    losType = BlastAttack.LoSType.None,
                    position = hitInfo.point,
                    falloffModel = BlastAttack.FalloffModel.None,
                    radius = 12f,
                    teamIndex = teamComponent ? teamComponent.teamIndex : TeamIndex.None,
                    procCoefficient = 1f,
                };
                projectileOverchargeComponent.blastAttack = blastAttack;
            }
            return BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            this.FireBullet(ray, "");
            stopwatch -= Time.fixedDeltaTime;
            if (stopwatch <= 0f)
            {
                if (isAuthority)
                {
                    activatorSkillSlot.stock--;
                }
                colliders.Clear();
                stopwatch = duration;
            }
            if (!base.isAuthority || IsKeyDownAuthority() && activatorSkillSlot.stock > 0)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class FireParry : FreemanBulletAttack
    {
        public override GameObject muzzleEffectPrefab => null;
        public override GameObject tracerEffectPrefab => null;
        public override GameObject hitEffectPrefab => null;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => "";
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => 0f;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 1f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericSpecial;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.None;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => 0f;
        public override float radius => 1f;
        public override int bulletCount => 1;
        public override float minSpread => 0f;
        public override float maxSpread => 0f;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public override float procCoefficient => 1f;
        public override LayerMask hitMask => LayerIndex.projectile.mask;
        public override LayerMask stopperMask => LayerIndex.projectile.mask;
        public override float range => 6f;
        public static float baseDuration = 0.2f;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            this.FireBullet(ray, "");
        }
        public override void ModifyBulletAttack(ref BulletAttack bulletAttack)
        {
            base.ModifyBulletAttack(ref bulletAttack);
            bulletAttack.hitCallback = ParryProjectiles;
        }

        private bool ParryProjectiles(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            ProjectileController projectileController = hitInfo.collider.GetComponent<ProjectileController>();
            if (projectileController != null)
            {
                projectileController.IgnoreCollisionsWithOwner(false);
                projectileController.owner = gameObject;
                projectileController.IgnoreCollisionsWithOwner(true);
                if (activatorSkillSlot) activatorSkillSlot.AddOneStock();
                TeamFilter teamFilter = projectileController.teamFilter;
                bool parry = true;
                if (teamFilter != null && teamComponent && teamFilter.teamIndex == teamComponent.teamIndex) parry = false;
                if (!parry) return false;
                if(teamFilter && teamComponent)
                teamFilter.teamIndex = teamComponent.teamIndex;
                Rigidbody rigidbody = projectileController.rigidbody;
                if (rigidbody != null)
                {
                    Ray ray = GetAimRay();
                    rigidbody.position = ray.origin;
                    rigidbody.velocity = Vector3.RotateTowards(rigidbody.velocity, ray.direction, 360f, 0f);
                    if (bulletAttack.isCrit)
                    {
                        ProjectileDamage projectileDamage = projectileController.gameObject.GetComponent<ProjectileDamage>();
                        if (projectileDamage != null)
                        {
                            projectileDamage.crit = true;
                        }
                    }
                }
                else
                {
                    Destroy(projectileController.gameObject);
                }
            }
            return false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge < this.duration || !base.isAuthority)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class FireKnockback : FreemanBulletAttack
    {
        public override GameObject muzzleEffectPrefab => null;
        public override GameObject tracerEffectPrefab => null;
        public override GameObject hitEffectPrefab => null;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => "";
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => 0f;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 0f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericSpecial;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.None;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => 0f;
        public override float radius => 5f;
        public override int bulletCount => 1;
        public override float minSpread => 0f;
        public override float maxSpread => 0f;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public override float procCoefficient => 1f;
        public override LayerMask hitMask => LayerIndex.entityPrecise.mask;
        public override LayerMask stopperMask => LayerIndex.ignoreRaycast.mask;
        public override float range => 12f;
        public static float baseDuration = 0.2f;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            this.FireBullet(ray, "");
        }
        public override void ModifyBulletAttack(ref BulletAttack bulletAttack)
        {
            base.ModifyBulletAttack(ref bulletAttack);
            bulletAttack.hitCallback = LaunchUpwards;
        }

        private bool LaunchUpwards(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            HurtBox hurtBox = hitInfo.hitHurtBox;
            if (hurtBox != null && hurtBox.healthComponent && hurtBox.healthComponent.body && (teamComponent && hurtBox.healthComponent.body.teamComponent ? teamComponent.teamIndex != hurtBox.healthComponent.body.teamComponent.teamIndex : true))
            {
                if (hurtBox.healthComponent.body.characterMotor)
                {
                    hurtBox.healthComponent.body.characterMotor.Motor?.ForceUnground();
                    hurtBox.healthComponent.body.characterMotor.velocity = Physics.gravity * -1f;
                }
                else if (hurtBox.healthComponent.body.rigidbody)
                {
                    hurtBox.healthComponent.body.rigidbody.velocity = Physics.gravity * -1f;
                }
            }
            return false;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge < this.duration || !base.isAuthority)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class FireChainsaw : FreemanBulletAttack
    {
        public override GameObject muzzleEffectPrefab => null;
        public override GameObject tracerEffectPrefab => null;
        public override GameObject hitEffectPrefab => null;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => "";
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => 0f;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 1f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericSpecial;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.None;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => 0f;
        public override float radius => 1f;
        public override int bulletCount => 1;
        public override float minSpread => 0f;
        public override float maxSpread => 0f;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public override float procCoefficient => 1f;
        public override LayerMask hitMask => LayerIndex.entityPrecise.mask;
        public override LayerMask stopperMask => LayerIndex.world.mask + LayerIndex.entityPrecise.mask;
        public override float range => 6f;
        public static float baseDuration = 0.2f;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration / this.attackSpeedStat;
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            this.FireBullet(ray, "");
        }
        public override void ModifyBulletAttack(ref BulletAttack bulletAttack)
        {
            base.ModifyBulletAttack(ref bulletAttack);
            bulletAttack.hitCallback = Chainsaw;
        }

        private bool Chainsaw(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            HurtBox hurtBox = hitInfo.hitHurtBox;
            if (hurtBox != null && hurtBox.healthComponent && hurtBox.healthComponent.body && (teamComponent && hurtBox.healthComponent.body.teamComponent ? teamComponent.teamIndex != hurtBox.healthComponent.body.teamComponent.teamIndex : true))
            {
                if (hurtBox.healthComponent.combinedHealthFraction <= 0.2f)
                {
                    bulletAttack.damage = hurtBox.healthComponent.fullCombinedHealth * 0.2f;
                    if (skillLocator && skillLocator.allSkills != null)
                        foreach (var skill in skillLocator.allSkills)
                        {
                            skill.stock = skill.maxStock;
                            skill.rechargeStopwatch = 0f;
                        }
                }
            }
            return BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge < this.duration || !base.isAuthority)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class FireGravityGun : BaseFreemanState
    {
        public CharacterBody heldEnemy;
        public ProjectileController heldProjectile;
        public override void OnEnter()
        {
            base.OnEnter();
            Ray ray = GetAimRay();
            RaycastHit[] raycastHits = Physics.CapsuleCastAll(ray.origin + ray.direction, ray.origin + ray.direction * 60f, 1f, ray.direction, 60f, LayerIndex.entityPrecise.mask + LayerIndex.projectile.mask + LayerIndex.debris.mask, QueryTriggerInteraction.UseGlobal);
            float distance = 69f;
            if (raycastHits != null && raycastHits.Length > 0)
            {
                foreach (var hit in raycastHits)
                {
                    HurtBox hurtBox = hit.collider.GetComponent<HurtBox>();
                    if (hurtBox && hurtBox.healthComponent && hurtBox.healthComponent == healthComponent) continue;
                    if (hit.distance < distance)
                    {
                        distance = hit.distance;
                        if (hurtBox != null && hurtBox.healthComponent != null && hurtBox.healthComponent.body != null)
                            heldEnemy = hurtBox.healthComponent.body;
                        ProjectileController projectileController = hit.collider.GetComponent<ProjectileController>();
                        if (projectileController != null && projectileController.rigidbody != null)
                        {
                            TeamFilter teamFilter = projectileController.teamFilter;
                            if (teamFilter != null && teamComponent)
                            {
                                teamFilter.teamIndex = teamComponent.teamIndex;
                            }
                            projectileController.IgnoreCollisionsWithOwner(false);
                            projectileController.owner = gameObject;
                            projectileController.IgnoreCollisionsWithOwner(true);
                            heldProjectile = projectileController;
                        }
                    }
                }
            }
            if (heldEnemy || heldProjectile)
            {
                Util.PlaySound(GravityGunPickup.playSoundString, gameObject);
            }
            Util.PlaySound(GravityGunHold.playSoundString, gameObject);
        }
        public override void OnExit()
        {
            base.OnExit();
            Util.PlaySound(GravityGunHold.stopSoundString, gameObject);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Ray ray = GetAimRay();
            float speed = characterBody.moveSpeed * 2f;
            if (heldEnemy)
            {
                Vector3 vector3 = (ray.origin + ray.direction * 6f + ray.direction * heldEnemy.radius) - heldEnemy.corePosition;
                if (heldEnemy.characterMotor)
                {
                    heldEnemy.characterMotor.velocity = vector3 * speed;
                    if(heldEnemy.characterMotor.isGrounded) heldEnemy.characterMotor?.Motor.ForceUnground();
                }
                else if (heldEnemy.rigidbody)
                {
                    heldEnemy.rigidbody.velocity = vector3 * speed;
                }
            }
            if (heldProjectile)
            {
                Vector3 vector3 = (ray.origin + ray.direction * 6f) - heldProjectile.transform.position;
                heldProjectile.rigidbody.velocity = vector3 * speed;
            }
            if (!base.isAuthority || IsKeyDownAuthority() && (heldEnemy || heldProjectile))
            {
                return;
            }
            Util.PlaySound(GravityGunFire.playSoundString, gameObject);
            if (heldEnemy)
            {
                if (heldEnemy.characterMotor)
                {
                    heldEnemy.characterMotor.velocity += ray.direction * 90f;
                    if (heldEnemy.characterMotor.isGrounded) heldEnemy.characterMotor?.Motor.ForceUnground();
                }
                else if (heldEnemy.rigidbody)
                {
                    heldEnemy.rigidbody.velocity += ray.direction * 90f;
                }
            }
            if (heldProjectile)
            {
                heldProjectile.rigidbody.velocity = ray.direction * 90f;
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class FireHook : BaseFreemanState
    {
        public CharacterBody targetBody;
        public static float baseSpeed = 2f;
        public float speed;
        public GameObject chainObject;
        public Transform startTransform;
        public Transform endTransform;
        public LookAtTarget lookAtTarget;
        public override void OnEnter()
        {
            base.OnEnter();
            characterBody.isSprinting = true;
            speed = baseSpeed * characterBody.moveSpeed;
            HookTracker hookTracker = GetComponent<HookTracker>();
            if (hookTracker != null && hookTracker.target)
            {
                targetBody = hookTracker.target.characterBody;
                chainObject = GameObject.Instantiate(HookChainEffect);
                ChildLocator childLocator = chainObject.GetComponent<ChildLocator>();
                startTransform = childLocator.FindChild("Start");
                endTransform = childLocator.FindChild("End");
                startTransform.SetParent(muzzleTransform, false);
                endTransform.SetParent(targetBody.transform, false);
                //characterBody.rootMotionInMainState = true;
                if (CameraRigController.readOnlyInstancesList != null)
                {
                    lookAtTarget = gameObject.AddComponent<LookAtTarget>();
                    lookAtTarget.target = hookTracker.target.transform;
                    lookAtTarget.inputBankTest = inputBank;
                    foreach (CameraRigController cameraRigController in CameraRigController.readOnlyInstancesList)
                    {
                        cameraRigController.SetOverrideCam(lookAtTarget, 0.2f);
                    }
                }
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (chainObject) Destroy(chainObject);
            if (startTransform) Destroy(startTransform.gameObject);
            if (endTransform) Destroy(endTransform.gameObject);
            characterBody.rootMotionInMainState = false;
            if (characterMotor)
            {
                characterMotor.disableAirControlUntilCollision = true;
                characterMotor.SetVelocityOverride(Vector3.zero);
            }
            if (CameraRigController.readOnlyInstancesList != null && lookAtTarget)
            {
                foreach (CameraRigController cameraRigController in CameraRigController.readOnlyInstancesList)
                {
                    cameraRigController.SetOverrideCam(null, 0.2f);
                }
                Destroy(lookAtTarget);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (isAuthority && targetBody == null) outer.SetNextStateToMain();
            Vector3 vector3 = (targetBody.corePosition - characterBody.corePosition).normalized;
            if (inputBank)
            {
                vector3 = (vector3 + (Quaternion.Euler(0, 90, 0) * inputBank.aimDirection * inputBank.rawMoveData.x) + (transform.up * inputBank.rawMoveData.y)).normalized;
            }
            if (characterMotor)
            {
                characterMotor.SetVelocityOverride(vector3 * speed);
            }
            else if (rigidbody)
            {
                rigidbody.velocity = vector3 * speed;
            }
            if (!base.isAuthority || IsKeyDownAuthority() && targetBody)
            {
                return;
            }
            outer.SetNextStateToMain();
        }
        public class LookAtTarget : MonoBehaviour, ICameraStateProvider
        {
            public Transform target;
            public InputBankTest inputBankTest;
            public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
            {
                cameraState.rotation = Quaternion.LookRotation((target.position - (inputBankTest ? inputBankTest.aimOrigin : this.transform.position )).normalized);
                //cameraState.position = this.transform.position;
            }
            public bool IsHudAllowed(CameraRigController cameraRigController)
            {
                return true;
            }

            public bool IsUserControlAllowed(CameraRigController cameraRigController)
            {
                return true;
            }

            public bool IsUserLookAllowed(CameraRigController cameraRigController)
            {
                return false;
            }
        }
    }
    public class ScopeSniper : FreemanScope
    {
        public override bool toExecute => inputBank && inputBank.skill1.justReleased;
        public override Type stateToEnter => typeof(FireSniper);
        public override GameObject crosshairOverrideObject => null;
        public override GameObject scopeOverlayObject => null;
        public override GameObject sniperTargetObject => Assets.SniperTargetTest;
    }
    public class FireSniper : FreemanBulletAttack
    {
        public override GameObject muzzleEffectPrefab => FirePistol2.muzzleEffectPrefab;
        public override GameObject tracerEffectPrefab => FirePistol2.tracerEffectPrefab;
        public override GameObject hitEffectPrefab => FirePistol2.hitEffectPrefab;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => PistolFire.playSoundString;
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => FirePistol2.recoilAmplitude;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 4f;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericSecondary;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.None;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => FirePistol2.spreadBloomValue;
        public override float radius => 1f;
        public override int bulletCount => 1;
        public override float minSpread => 0f;
        public override float maxSpread => 0f;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public override float procCoefficient => 1f;
        public override float range => 1024f;
        public override LayerMask hitMask => LayerIndex.entityPrecise.mask;
        public override LayerMask stopperMask => LayerIndex.entityPrecise.mask + LayerIndex.world.mask;
        public static float baseDuration = 0.05f;
        public float stopwatch = 0f;
        public int step;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = baseDuration;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch -= Time.fixedDeltaTime;
            if (stopwatch <= 0f)
            {
                Ray ray = GetAimRay();
                FireBullet(ray, "MuzzleRight");
                if (isAuthority)
                    activatorSkillSlot.stock--;
                stopwatch = duration;
            }
            if (!base.isAuthority || IsKeyDownAuthority() && activatorSkillSlot.stock > 0)
            {
                return;
            }
            this.outer.SetNextStateToMain();
        }
        private bool Headshot(BulletAttack bulletAttack, ref BulletAttack.BulletHit hitInfo)
        {
            if(hitInfo.hitHurtBox && hitInfo.hitHurtBox.isSniperTarget)
            {
                CharacterBody characterBody = hitInfo.hitHurtBox.healthComponent && hitInfo.hitHurtBox.healthComponent .body ? hitInfo.hitHurtBox.healthComponent.body : null;
                if (characterBody != null)
                {
                    characterBody.AddTimedBuffAuthority(DLC2Content.Buffs.DisableAllSkills.buffIndex, 4f);
                }
            }
            return BulletAttack.DefaultHitCallbackImplementation(bulletAttack, ref hitInfo);
        }
    }
    public class FireJackhammer : FreemanBulletAttack
    {
        public override GameObject muzzleEffectPrefab => null;
        public override GameObject tracerEffectPrefab => null;
        public override GameObject hitEffectPrefab => null;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => "";
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => 0f;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 0.5f * characterBody.attackSpeed;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericPrimary;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.None;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => 0f;
        public override float radius => 1f;
        public override int bulletCount => 1;
        public override float minSpread => 0f;
        public override float maxSpread => 0f;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public override float procCoefficient => 1f;
        public override LayerMask hitMask => LayerIndex.entityPrecise.mask + LayerIndex.projectile.mask;
        public override LayerMask stopperMask => LayerIndex.entityPrecise.mask + LayerIndex.world.mask;
        public override float range => 6f;
        public static float baseDuration = 1f / 30f;
        public static float exitTime = 0.5f;
        public float duration;
        public float stopwatch;
        public bool end = false;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration;
            stopwatch = duration;
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            stopwatch -= Time.fixedDeltaTime;
            if (stopwatch <= 0f)
            {
                if (end)
                {
                    this.outer.SetNextStateToMain();
                }
                else
                {
                    this.FireBullet(ray, "");
                    stopwatch = duration;
                }
            }
            if (!base.isAuthority || (IsKeyDownAuthority() && activatorSkillSlot.stock > 0) || end)
            {
                return;
            }
            end = true;
            stopwatch = exitTime;
        }
    }
    public class JackhammerSlingshot : BaseFreemanState
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!base.isAuthority || IsKeyDownAuthority())
            {
                return;
            }
            Ray ray = GetAimRay();
            if (characterMotor)
            {
                characterMotor.velocity += ray.direction * MathF.Min(age, 5f);
            }
            else if (rigidbody)
            {
                rigidbody.velocity += ray.direction * MathF.Min(age, 5f);
            }
            this.outer.SetNextStateToMain();
        }
    }
    public class FireCrucible : FreemanBulletAttack
    {
        public override GameObject muzzleEffectPrefab => null;
        public override GameObject tracerEffectPrefab => null;
        public override GameObject hitEffectPrefab => null;
        public override GameObject weapon => weaponObject;
        public override string fireSoundString => "";
        public override float minVerticalRecoil => -0.4f;
        public override float maxVerticalRecoil => -0.8f;
        public override float minHorizontalRecoil => -0.3f;
        public override float maxHorizontalRecoil => 0.3f;
        public override float recoilAmplitude => 0f;
        public override float trajectoryAimAssistMultiplier => FirePistol2.trajectoryAimAssistMultiplier;
        public override float damageCoefficient => 0.5f * characterBody.attackSpeed;
        public override DamageTypeCombo damageType => DamageTypeCombo.GenericPrimary;
        public override BulletAttack.FalloffModel falloff => BulletAttack.FalloffModel.None;
        public override float force => FirePistol2.force;
        public override float spreadBloomValue => 0f;
        public override float radius => 1f;
        public override int bulletCount => 1;
        public override float minSpread => 0f;
        public override float maxSpread => 0f;
        public override bool crit => Util.CheckRoll(characterBody.crit, characterBody.master);
        public override float procCoefficient => 1f;
        public override LayerMask hitMask => LayerIndex.entityPrecise.mask + LayerIndex.projectile.mask;
        public override LayerMask stopperMask => LayerIndex.entityPrecise.mask + LayerIndex.world.mask;
        public override float range => 6f;
        public static float fireTime = 0.4f;
        public static float exitTime = 0.8f;
        public float duration;
        public float stopwatch;
        public bool fire = true;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = fireTime;
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override void ModifyBulletAttack(ref BulletAttack bulletAttack)
        {
            base.ModifyBulletAttack(ref bulletAttack);
            bulletAttack.AddModdedDamageType(BrynzaAPI.Assets.BruiseChampion);
            bulletAttack.AddModdedDamageType(BrynzaAPI.Assets.InstakillNoChampion);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            Ray ray = GetAimRay();
            base.StartAimMode(ray, 3f, false);
            stopwatch += Time.fixedDeltaTime;
            if (stopwatch >= duration)
            {
                if (fire)
                {
                    this.FireBullet(ray, "");
                    fire = false;
                }
            }
            if (!base.isAuthority || stopwatch < exitTime)
            {
                return;
            }
            outer.SetNextStateToMain();
        }
    }
    public class CrucibleSlam : BaseFreemanState
    {
        public static float slamRadius = 24f;
        public static float slamSpeed = 3f;
        public static float afterSlamTime = 2f;
        public float slamTime;
        public float slamDistance = 10f;
        public float slamForce => (slamDistance / afterSlamTime) - (Physics.gravity.y * afterSlamTime / 2f);
        public bool removeBuff = false;
        public override void OnEnter()
        {
            base.OnEnter();
            if (isAuthority)
            {
                if (characterMotor)
                {
                    if (characterMotor.isGrounded)
                    {
                        outer.SetStateToMain();
                        return;
                    }
                    characterMotor.onHitGroundAuthority += CharacterMotor_onHitGroundAuthority;
                    if (Physics.Raycast(characterBody.footPosition, new Vector3(0f, Physics.gravity.y, 0f), out var hitInfo, 99999f, LayerIndex.world.mask + LayerIndex.defaultLayer.mask + LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal))
                    {
                        slamDistance = hitInfo.distance;
                    }

                }
                else
                {
                    outer.SetStateToMain();
                    return;
                }
                characterBody.AddBuffAuthotiry(JunkContent.Buffs.IgnoreFallDamage);
                removeBuff = true;
            }

        }
        private void CharacterMotor_onHitGroundAuthority(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            Collider[] colliders = Physics.OverlapSphere(characterBody.footPosition, slamRadius, LayerIndex.CommonMasks.characterBodies + LayerIndex.CommonMasks.fakeActorLayers, QueryTriggerInteraction.UseGlobal);
            List<CharacterBody> list = new List<CharacterBody>();
            foreach (Collider collider in colliders)
            {
                CharacterBody characterBody = collider.GetComponent<CharacterBody>();
                if (characterBody == null || list.Contains(characterBody)) continue;
                if (characterBody.characterMotor)
                {
                    characterBody.characterMotor.Motor?.ForceUnground(0f);
                    characterBody.characterMotor.velocity.y = Trajectory.CalculateInitialYSpeed(afterSlamTime, slamDistance);
                    //characterBody.characterMotor.ApplyForce(Physics.gravity.normalized * -1f * slamForce, true, true);
                }
                list.Add(characterBody);
            }
            outer.SetNextStateToMain();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            {
                Vector3 vector3 = Physics.gravity * 3f;
                slamTime += Time.fixedDeltaTime;
                if (isAuthority)
                {
                    if (characterMotor)
                    {
                        characterMotor.SetVelocityOverride(vector3);
                    }
                    else if (rigidbody)
                    {
                        rigidbody.velocity = vector3;
                    }
                }
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (isAuthority)
            {
                if (characterMotor)
                {
                    characterMotor.SetVelocityOverride(Vector3.zero);
                    characterMotor.disableAirControlUntilCollision = false;
                    characterMotor.onHitGroundAuthority -= CharacterMotor_onHitGroundAuthority;
                }
                else if (rigidbody)
                {
                    rigidbody.velocity = Vector3.zero;
                }
                if (removeBuff)
                    characterBody.RemoveBuffAuthotiry(JunkContent.Buffs.IgnoreFallDamage);
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }
}

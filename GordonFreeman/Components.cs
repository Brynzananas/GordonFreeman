using EntityStates;
using RoR2.Skills;
using RoR2;
using RoR2.HudOverlay;
using RoR2.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using static GordonFreeman.Assets;
using RoR2.Projectile;
using System.Linq;
using static UnityEngine.UI.GridLayoutGroup;
using RoR2.CameraModes;
using R2API;
using static UnityEngine.SendMouseEvents;
using UnityEngine.Networking;
using BrynzaAPI;
using Newtonsoft.Json.Utilities;
using static GordonFreeman.ProfessionalCharacterComponent;
using Rewired;
using HarmonyLib;
using System.Collections;
using UnityEngine.Bindings;
using RoR2.PostProcessing;
using RoR2BepInExPack.Utilities;

namespace GordonFreeman
{
    public class ProfessionalCharacterMain : GenericCharacterMain
    {
        bool isSprintDown;
        public ProfessionalBodyComponent professionalBodyComponent;
        public ProfessionalCharacterComponent professionalCharacterComponent;
        public List<ButtonState> buttonStates = new List<ButtonState>();

        public override void OnEnter()
        {
            base.OnEnter();
            professionalBodyComponent = characterBody && (characterBody is ProfessionalBodyComponent) ? (characterBody as ProfessionalBodyComponent) : null;
            professionalCharacterComponent = professionalBodyComponent?.professionalCharacterComponent;
            if (professionalCharacterComponent != null)
                professionalBodyComponent.professionalCharacterMain = this;
            buttonStates.Add(new ButtonState() { id = 0 });
            buttonStates.Add(new ButtonState() { id = 1 });
            buttonStates.Add(new ButtonState() { id = 2 });
            buttonStates.Add(new ButtonState() { id = 3 });
        }
        public override void OnExit()
        {
            base.OnExit();
            buttonStates.Clear();
        }
        public override void Update()
        {
            base.Update();
            if (!isSprintDown)
                isSprintDown = characterBody.isPlayerControlled ? characterBody.master.playerCharacterMasterController.networkUser.inputPlayer.GetButtonDown(18) : inputBank.sprint.justPressed;
            if (inputBank)
            {
                if (inputBank.rawMoveLeft.justPressed) OperateDoubleInput(0);
                if (inputBank.rawMoveRight.justPressed) OperateDoubleInput(1);
                if (inputBank.rawMoveUp.justPressed) OperateDoubleInput(2);
                if (inputBank.rawMoveDown.justPressed) OperateDoubleInput(3);
            }
            foreach (ButtonState state in buttonStates)
            {
                if (state == null) continue;
                if (state.time > 0) state.time -= Time.deltaTime;
            }
        }
        public void OperateDoubleInput(int id)
        {
            ButtonState buttonState = buttonStates[id];
            if (buttonState == null) return;
            foreach (ButtonState state in buttonStates)
            {
                if (state == null || state == buttonState) continue;
                state.time = 0;
            }
            if (buttonState.time > 0) isSprintDown = true;
            buttonState.time = 0.15f;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
        }
        public override void HandleMovements()
        {
            if (isSprintDown && skillLocator && skillLocator.utility)
            {
                skillLocator.utility.ExecuteIfReady();
                isSprintDown = false;
            }
            //sprintInputReceived = true;
            if (characterBody != null)
                //characterBody.isSprinting = true;
                base.HandleMovements();
        }
        public override bool CanExecuteSkill(GenericSkill skillSlot)
        {
            if (skillLocator && ((skillLocator.primary && skillSlot == skillLocator.primary) || (skillLocator.secondary && skillSlot == skillLocator.secondary)) && professionalCharacterComponent && professionalCharacterComponent.weaponWheelSelectorComponent && professionalCharacterComponent.weaponWheelSelectorComponent.mainSelector && professionalCharacterComponent.weaponWheelSelectorComponent.mainSelector.activeSelf) return false;
            if (characterBody && skillLocator && skillLocator.utility && skillSlot == skillLocator.utility) return false;
            return base.CanExecuteSkill(skillSlot);
        }
        public class ButtonState
        {
            public float time;
            public int id;
        }
    }
    public class ProfessionalBodyComponent : CharacterBody
    {
        public ProfessionalCharacterComponent professionalCharacterComponent;
        public ProfessionalCharacterMain professionalCharacterMain;
    }
    public class ProfessionalCharacterComponent : MonoBehaviour
    {
        public ProfessionalSkillLocator[] weapons;
        public GenericSkill[] skills;
        public ProfessionalSkillLocator currentWeapon;
        public ProfessionalModelComponent modelComponent;
        public GenericSkill currentSpecial;
        public EntityStateMachine specialStateMachine;
        [HideInInspector] public ProfessionalSkillLocator previousWeapon;
        [HideInInspector] public GenericSkill previousSpecial;
        public CharacterBody characterBody;
        public InputBankTest inputBankTest;
        public SkillLocator skillLocator;
        [HideInInspector] public OverlayController overlayController;
        [HideInInspector] public GameObject weaponWheelSelector;
        [HideInInspector] public GraphicRaycaster raycaster;
        [HideInInspector] public MPEventSystemLocator eventSystemLocator;
        [HideInInspector] public WeaponWheelSelectorComponent weaponWheelSelectorComponent;
        [HideInInspector] public HGButton selectedButton;
        public GameObject weaponObject;
        public FirstPersonCamera firstPersonCamera;
        [HideInInspector] public UtiltiyCharges utilityCharges;
        private int _rampageCount;
        public float rampageStopwatch;
        public int rampageCount
        {
            get
            {
                return _rampageCount;
            }
            set
            {
                rampageStopwatch = 0f;
                if (value == _rampageCount) return;
                _rampageCount = value;
                Main.rampage = _rampageCount > 5;
            }
        }
        private bool built = false;
        public void OnEnable()
        {
            if (built) return; built = true;
            OverlayCreationParams overlayCreationParams = new OverlayCreationParams
            {
                prefab = WeaponWheelSelector,
                childLocatorEntry = "CrosshairExtras"
            };
            this.overlayController = HudOverlayManager.AddOverlay(base.gameObject, overlayCreationParams);
            this.overlayController.onInstanceAdded += OnOverlayInstanceAdded;
            this.overlayController.onInstanceRemove += OnOverlayInstanceRemoved;
            void OnOverlayInstanceRemoved(OverlayController controller, GameObject @object)
            {

            }
            void OnOverlayInstanceAdded(OverlayController controller, GameObject @object)
            {
                weaponWheelSelector = @object;
                weaponWheelSelectorComponent = @object.GetComponent<WeaponWheelSelectorComponent>();
                ChildLocator childLocator = weaponWheelSelectorComponent.childLocator;
                weaponWheelSelectorComponent.professionalCharacterComponent = this;
                raycaster = weaponWheelSelectorComponent.graphicRaycaster;
                eventSystemLocator = weaponWheelSelectorComponent.mPEventSystemLocator;
                weaponWheelSelectorComponent.networkUser = characterBody?.master?.playerCharacterMasterController?.networkUser;
                weaponWheelSelectorComponent.player = weaponWheelSelectorComponent?.networkUser?.inputPlayer;
                
                Transform weaponSelector = childLocator.FindChild("WeaponSelector");
                Transform specialSelector = childLocator.FindChild("SpecialsSelector");
                Transform utilities = childLocator.FindChild("UtilityCharges");
                utilityCharges = utilities.GetComponent<UtiltiyCharges>();
                utilityCharges.skillLocator = skillLocator;
                foreach (var skill in weapons)
                {
                    GameObject weaponButtonSelector = Instantiate(WeaponButtonSelector, weaponSelector);
                    WeaponButtonSelectorComponent weaponButtonSelectorComponent = weaponButtonSelector.GetComponent<WeaponButtonSelectorComponent>();
                    weaponButtonSelectorComponent.professionalSkillLocator = skill;
                    weaponButtonSelectorComponent.weaponWheelSelectorComponent = weaponWheelSelectorComponent;
                    HGButton hGButton = weaponButtonSelectorComponent.hGButton;
                    hGButton.image.sprite = weaponButtonSelectorComponent.professionalSkillLocator.primarySkill.baseSkill.icon;
                    skill.button = hGButton;
                    hGButton.onClick.AddListener(OnButtonClicked);
                    void OnButtonClicked()
                    {
                        weaponButtonSelectorComponent.SelectWeapon();
                    }
                    hGButton.onSelect.AddListener(OnButtonSelected);
                    void OnButtonSelected()
                    {
                        selectedButton = hGButton;
                    }
                    ProfessionalSkillDef primarySkillDef = skill.primarySkill && skill.primarySkill.baseSkill && skill.primarySkill.baseSkill is ProfessionalSkillDef ? skill.primarySkill.skillDef  as ProfessionalSkillDef: null;
                    if (primarySkillDef && primarySkillDef.weaponModel)
                    {
                        modelComponent.AddWeaponModel(primarySkillDef, primarySkillDef.weaponModel);
                    }
                    weaponWheelSelectorComponent.weaponButtons.Add(hGButton);
                }
                bool selectFirstSpecial = false;
                if (skillLocator)
                    foreach (var skill in skillLocator.allSkills)
                    {
                        HeroSpecialSkillDef heroSpecialSkillDef = skill.baseSkill != null && skill.baseSkill is HeroSpecialSkillDef ? skill.baseSkill as HeroSpecialSkillDef : null;
                        if (heroSpecialSkillDef == null) continue;
                        List<GenericSkill> allSkills = skillLocator.allSkills.ToList();
                        foreach (var specialSkill in heroSpecialSkillDef.skillsSelection)
                        {
                            GenericSkill genericSkill = gameObject.AddComponent<GenericSkill>();
                            genericSkill.defaultSkillDef = specialSkill;
                            genericSkill.baseSkill = specialSkill;
                            genericSkill.skillDef = specialSkill;
                            genericSkill.AssignSkill(specialSkill);
                            genericSkill.stateMachine = specialStateMachine;
                            allSkills.Add(genericSkill);
                            GameObject specialButtonSelector = Instantiate(SpecialButtonSelector, specialSelector);
                            SpeicalButtonSelectorComponent specialButtonSelectorComponent = specialButtonSelector.GetComponent<SpeicalButtonSelectorComponent>();
                            specialButtonSelectorComponent.genericSkill = genericSkill;
                            specialButtonSelectorComponent.weaponWheelSelectorComponent = weaponWheelSelectorComponent;
                            HGButton hGButton = specialButtonSelectorComponent.hGButton;
                            hGButton.image.sprite = specialButtonSelectorComponent.genericSkill.baseSkill.icon; ;
                            hGButton.onClick.AddListener(OnButtonClicked);
                            void OnButtonClicked()
                            {
                                specialButtonSelectorComponent.SelectSpecial();
                            }
                            hGButton.onSelect.AddListener(OnButtonSelected);
                            void OnButtonSelected()
                            {
                                selectedButton = hGButton;
                            }
                            weaponWheelSelectorComponent.specialButtons.Add(hGButton);
                            if(skills == null) skills = new GenericSkill[0];
                            skills.AddItem(genericSkill);
                            if (!selectFirstSpecial)
                            {
                                currentSpecial = genericSkill;
                                selectFirstSpecial = true;
                            }
                        }
                        skillLocator.allSkills = allSkills.ToArray();
                    }
                if (selectFirstSpecial)
                {
                    ApplySpecial(currentSpecial);
                }
                @object.SetActive(false);
            }

        }
        public void OnDisable()
        {
            Main.rampage = false;
        }
        public void RegisterKill(CharacterBody characterBody)
        {
            rampageCount++;
        }
        public void RegisterHit(CharacterBody characterBody, DamageInfo damageInfo)
        {
            rampageStopwatch = 0f;
        }
        public void FixedUpdate()
        {
            if (inputBankTest)
            {
                if (inputBankTest.skill3.justPressed)
                {
                    weaponWheelSelectorComponent.EnableMainSelector();
                }
                if (inputBankTest.skill3.justReleased)
                {
                    if (selectedButton)
                    {
                        selectedButton.Press();
                    }

                    weaponWheelSelectorComponent.DisableMainSelector();
                }
            }

        }

        public void Update()
        {
            rampageStopwatch += Time.deltaTime;
            if (rampageStopwatch > 7f)
            {
                rampageCount = 0;
            }
        }
        public void SwapUp()
        {
            ProfessionalSkillLocator nextSkill = weapons[0];
            Array.Copy(weapons, 1, weapons, 0, weapons.Length - 1);
            ProfessionalSkillLocator currentSkill2 = currentWeapon;
            weapons[weapons.Length - 1] = currentSkill2;
            ApplyWeapon(nextSkill);
        }
        public void SwapDown()
        {
            Array.Reverse(weapons);
            ProfessionalSkillLocator nextSkill = weapons[0];
            Array.Copy(weapons, 1, weapons, 0, weapons.Length - 1);
            ProfessionalSkillLocator currentSkill2 = currentWeapon;
            weapons[weapons.Length - 1] = currentSkill2;
            Array.Reverse(weapons);
            ApplyWeapon(nextSkill);
        }
        public void ApplyWeapon(ProfessionalSkillLocator professionalSkillLocator)
        {
            if (professionalSkillLocator == null) return;
            previousWeapon = currentWeapon;
            currentWeapon = professionalSkillLocator;
            skillLocator.primary.stateMachine.SetNextStateToMain();
            skillLocator.secondary.stateMachine.SetNextStateToMain();
            skillLocator.primary = professionalSkillLocator.primarySkill;
            skillLocator.secondary = professionalSkillLocator.secondarySkill;
            skillLocator.primary.LinkSkill(null);
            skillLocator.secondary.LinkSkill(null);
            skillLocator.primary.RecalculateValues();
            skillLocator.secondary.RecalculateValues();
            foreach (var skill in weapons)
            {
                if (skill.primarySkill != skillLocator.primary)
                {
                    skill.primarySkill.LinkSkill(skillLocator.primary);
                    skill.primarySkill.RecalculateValues();
                }
                if (skill.secondarySkill != skillLocator.secondary)
                {
                    skill.secondarySkill.LinkSkill(skillLocator.secondary);
                    skill.secondarySkill.RecalculateValues();
                }
            }
            modelComponent.PullOnWeaponModel(professionalSkillLocator.primarySkill.baseSkill);
        }
        public void ApplySpecial(GenericSkill genericSkill)
        {
            if (genericSkill == null) return;
            previousSpecial = currentSpecial;
            currentSpecial = genericSkill;
            skillLocator.special.stateMachine.SetNextStateToMain();
            skillLocator.special = genericSkill;
            skillLocator.special.LinkSkill(null);
            skillLocator.special.RecalculateValues();
            foreach (var skill in skills)
            {
                if (skill == null) continue;
                skill.LinkSkill(genericSkill);
                skill.RecalculateValues();
            }
        }
    }
    public class ProfessionalModelComponent : MonoBehaviour
    {
        [HideInInspector] public Dictionary<SkillDef, WeaponModel> keyValuePairs = new Dictionary<SkillDef, WeaponModel>();
        public Transform weaponsTransform;
        public Transform weaponTransform;
        [HideInInspector] public WeaponModel currentWeaponModel;
        public FirstPersonCamera firstPersonCamera;
        public CameraTargetParams cameraTargetParams;
        public ChildLocator childLocator;
        [HideInInspector] public Vector3 previousVector;
        public void AddWeaponModel(SkillDef skillDef, WeaponModelDef weaponModelDef)
        {
            if (skillDef == null || weaponModelDef == null || keyValuePairs.ContainsKey(skillDef)) return;
            WeaponModel weaponModel = weaponModelDef.ApplyWeaponModel(this);
            keyValuePairs.Add(skillDef, weaponModel);
        }
        public WeaponModel PullOnWeaponModel(SkillDef skillDef)
        {
            if (skillDef == null) return null;
            if (!keyValuePairs.ContainsKey(skillDef))
            {
                PullOffCurrentWeaponModel();
                return null;
            }
            WeaponModel weaponModel = keyValuePairs[skillDef];
            if(currentWeaponModel)
            {
                if (currentWeaponModel == weaponModel) return weaponModel;
                PullOffCurrentWeaponModel();
            }
            weaponModel.gameObject.SetActive(true);
            currentWeaponModel = weaponModel;
            weaponModel.transform.rotation = Quaternion.identity;
            weaponModel.transform.SetParent(weaponTransform, false);
            int i = childLocator.FindChildIndex("Muzzle");
            if(i != -1)
            childLocator.transformPairs[i].transform = weaponModel.muzzleTransform ? weaponModel.muzzleTransform : weaponTransform;
            return weaponModel;
        }
        public void PullOffCurrentWeaponModel()
        {
            if(currentWeaponModel == null) return;
            if (currentWeaponModel)
            {
                currentWeaponModel.transform.SetParent(weaponsTransform, false);
                currentWeaponModel.gameObject.SetActive(false);
                currentWeaponModel.transform.rotation = Quaternion.identity;
                currentWeaponModel = null;
                int i = childLocator.FindChildIndex("Muzzle");
                if (i != -1)
                    childLocator.transformPairs[i].transform = weaponTransform;
            }
        }
        public void Update()
        {
            if (!currentWeaponModel) return;
            currentWeaponModel.transform.localEulerAngles = new Vector3 (cameraTargetParams.recoil.y, cameraTargetParams.recoil.x, 0f);
        }
    }
    public class ProfessionalSkillLocator : MonoBehaviour
    {
        public GenericSkill primarySkill;
        public GenericSkill secondarySkill;
        [HideInInspector] public HGButton button;
        public void Start()
        {
            if (primarySkill == null) return;
            if (secondarySkill == null) return;
            SkillDef skillDef = primarySkill.baseSkill && primarySkill.baseSkill is ProfessionalSkillDef ? (primarySkill.baseSkill as ProfessionalSkillDef).secondarySkillDef : null;
            if (skillDef == null) return;
            secondarySkill.SetBaseSkill(skillDef);
            secondarySkill.AssignSkill(skillDef);
        }
    }
    public class RadialLayout : LayoutGroup
    {
        public float fDistance;
        [Range(0f, 360f)]
        public float MinAngle, MaxAngle, StartAngle;
        public bool OnlyLayoutVisible = false;
        public override void OnEnable() { base.OnEnable(); CalculateRadial(); }
        public override void SetLayoutHorizontal()
        {
        }
        public override void SetLayoutVertical()
        {
        }
        public override void CalculateLayoutInputVertical()
        {
            CalculateRadial();
        }
        public override void CalculateLayoutInputHorizontal()
        {
            CalculateRadial();
        }
        public override void OnDisable()
        {
            m_Tracker.Clear();
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
        }
        void CalculateRadial()
        {
            m_Tracker.Clear();
            if (transform.childCount == 0)
                return;

            int ChildrenToFormat = 0;
            if (OnlyLayoutVisible)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    RectTransform child = (RectTransform)transform.GetChild(i);
                    if ((child != null) && child.gameObject.activeSelf)
                        ++ChildrenToFormat;
                }
            }
            else
            {
                ChildrenToFormat = transform.childCount;
            }

            float fOffsetAngle = (MaxAngle - MinAngle) / ChildrenToFormat;

            float fAngle = StartAngle;
            for (int i = 0; i < transform.childCount; i++)
            {
                RectTransform child = (RectTransform)transform.GetChild(i);
                if ((child != null) && (!OnlyLayoutVisible || child.gameObject.activeSelf))
                {
                    m_Tracker.Add(this, child,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.AnchoredPosition |
                    DrivenTransformProperties.Pivot);
                    Vector3 vPos = new Vector3(Mathf.Cos(fAngle * Mathf.Deg2Rad), Mathf.Sin(fAngle * Mathf.Deg2Rad), 0);
                    child.localPosition = vPos * fDistance;
                    child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);
                    fAngle += fOffsetAngle;
                }

            }
        }
    }
    public class WeaponWheelSelectorComponent : MonoBehaviour
    {
        [HideInInspector] public ProfessionalCharacterComponent professionalCharacterComponent;
        public GameObject weaponSelector;
        public GameObject specialsSelector;
        public GameObject selectedSelector;
        public GameObject mainSelector;
        public bool isSpecialSelectorSelected
        {
            get { return selectedSelector == specialsSelector; }
        }
        [HideInInspector] public List<HGButton> weaponButtons = new List<HGButton>();
        [HideInInspector] public List<HGButton> specialButtons = new List<HGButton>();
        [HideInInspector] public Player player;
        [HideInInspector] public NetworkUser networkUser;
        public ChildLocator childLocator;
        public GraphicRaycaster graphicRaycaster;
        public MPEventSystemLocator mPEventSystemLocator;
        public LineRenderer lineRenderer;
        public Canvas canvas;
        [HideInInspector] public Vector2 previousMousePosition = Vector2.zero;
        [HideInInspector] public Vector2 arrowDirection = new Vector2(0f, 1f);
        public static float cursorLength = 64f;
        public void ScrollUp()
        {
            selectedSelector.SetActive(false);
            selectedSelector = isSpecialSelectorSelected ? weaponSelector : specialsSelector;
            selectedSelector.SetActive(true);
        }
        public void ScrollDown()
        {
            selectedSelector.SetActive(false);
            selectedSelector = isSpecialSelectorSelected ? weaponSelector : specialsSelector;
            selectedSelector.SetActive(true);
        }
        public void SelectWeaponSelector()
        {
            selectedSelector.SetActive(false);
            selectedSelector = weaponSelector;
            selectedSelector.SetActive(true);
        }
        public void SelectSpecialsSelector()
        {
            selectedSelector.SetActive(false);
            selectedSelector = specialsSelector;
            selectedSelector.SetActive(true);
        }
        public void Update()
        {
            if (player != null)
            {
                if (player.GetButtonDown(7))
                {
                    ScrollUp();
                }
                if (player.GetButtonDown(8))
                {
                    ScrollDown();
                }
            }
            
            if (professionalCharacterComponent == null) return;
            if (!mainSelector.activeSelf) return;
            MPInput mPInput = ((MPInput)((MPInputModule)mPEventSystemLocator?.eventSystem?.currentInputModule)?.input);
            if(mPInput == null) return;
            Vector2 vector3 = new Vector2(Screen.width / 2f, Screen.height / 2f);
            //mPInput.internalMousePosition = Vector2.ClampMagnitude(mPInput.internalMousePosition, cursorLength) + vector3;
            Vector2 direction = mPInput.internalMousePosition - vector3;
            mPInput.CenterCursor();
            previousMousePosition = mPInput.internalMousePosition;
            if (direction != Vector2.down) arrowDirection = direction.normalized;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, new Vector2(transform.position.x, transform.position.y) + (arrowDirection * cursorLength));
            float angle = 3720f;
            HGButton selectedButton = null;
            int i = 0;
            if(specialButtons != null && weaponButtons != null)
            foreach (var button in selectedSelector == weaponSelector ? weaponButtons : specialButtons)
            {
                if (button == null) continue;
                float newAngle = Vector3.Angle(direction, button.transform.localPosition);
                if (newAngle < angle)
                {
                    angle = newAngle;
                    selectedButton = button;
                }
                i++;
            }
            if (selectedButton == null) return;
            selectedButton.Select();
        }
        public void DisableMainSelector()
        {
            SelectWeaponSelector();
            mainSelector.SetActive(false);
        }
        public void EnableMainSelector()
        {
            SelectWeaponSelector();
            mainSelector.SetActive(true);
        }
    }
    public class WeaponButtonSelectorComponent : MonoBehaviour
    {
        public ProfessionalSkillLocator professionalSkillLocator;
        public WeaponWheelSelectorComponent weaponWheelSelectorComponent;
        public HGButton hGButton;
        public HGTextMeshProUGUI primaryText;
        public HGTextMeshProUGUI secondaryText;

        public void FixedUpdate()
        {
            if (professionalSkillLocator == null) return;
            GenericSkill primarySkill = professionalSkillLocator.primarySkill;
            GenericSkill secondarySkill = professionalSkillLocator.secondarySkill;
            if (primarySkill == null && secondarySkill == null) return;
            if (primarySkill && primaryText)
            {
                primaryText.text = "1S ST: " + primarySkill.stock + "\n1S CD:" + primarySkill.cooldownRemaining;
            }
            if (secondarySkill && secondaryText)
            {
                secondaryText.text = "2S ST: " + secondarySkill.stock + "\n2S CD:" + secondarySkill.cooldownRemaining;
            }
        }
        public void SelectWeapon()
        {
            if (weaponWheelSelectorComponent == null) return;
            if (weaponWheelSelectorComponent.professionalCharacterComponent == null) return;
            weaponWheelSelectorComponent.professionalCharacterComponent.ApplyWeapon(professionalSkillLocator);
            weaponWheelSelectorComponent.DisableMainSelector();
        }
    }
    public class SpeicalButtonSelectorComponent : MonoBehaviour
    {
        public GenericSkill genericSkill;
        public WeaponWheelSelectorComponent weaponWheelSelectorComponent;
        public HGButton hGButton;
        public HeroSpecialSkillDef weaponSpecialSkillDef;
        public void SelectSpecial()
        {
            if (weaponWheelSelectorComponent == null) return;
            if (weaponWheelSelectorComponent.professionalCharacterComponent == null) return;
            weaponWheelSelectorComponent.professionalCharacterComponent.ApplySpecial(genericSkill);
            weaponWheelSelectorComponent.DisableMainSelector();
        }
    }
    public class FreemanProjectileController : ProjectileController
    {
        public float contradictGravityValue;
        public new void Start()
        {
            base.Start();
            if (rigidbody.useGravity)
                rigidbody.velocity += Physics.gravity * contradictGravityValue * -1f;
        }
    }
    public class CointHitComponent : MonoBehaviour
    {
        public static List<CointHitComponent> cointHitComponents = new List<CointHitComponent>();
        public TeamFilter teamFilter;
        public bool hit = false;
        public GameObject owner { get { return freemanProjectileController?.owner; } }
        public FreemanProjectileController freemanProjectileController;
        public void OnEnable()
        {
            cointHitComponents.Add(this);
        }
        public void OnDisable()
        {
            if (cointHitComponents.Contains(this))
                cointHitComponents.Remove(this);
        }
        public void FixedUpdate()
        {
            if (!owner) return;
            Vector3 vector = owner.transform.position - transform.position;
            float distacne = vector.magnitude;
            float scale = MathF.Max(distacne / 5f, 1f);
            transform.localScale = new Vector3(scale, scale, scale);
        }
        public void Hit(BulletAttack bulletAttack)
        {
            hit = true;
            if (cointHitComponents.Contains(this))
                cointHitComponents.Remove(this);
            Vector3 direction = Vector3.zero;
            float distance = 9999f * 9999f;
            if (cointHitComponents.Count > 0)
            {
                for (int i = 0; i < cointHitComponents.Count; i++)
                {
                    CointHitComponent cointHitComponent = cointHitComponents[i];
                    if (cointHitComponent == null || cointHitComponent.hit) continue;
                    Vector3 vector3 = cointHitComponent.transform.position - transform.position;
                    float newDistance = vector3.sqrMagnitude;
                    if (distance < newDistance) continue;
                    distance = newDistance;
                    direction = vector3;
                }
            }
            else
            {
                BullseyeSearch bullseyeSearch = new BullseyeSearch();
                bullseyeSearch.teamMaskFilter = TeamMask.all;
                bullseyeSearch.teamMaskFilter.RemoveTeam(teamFilter.teamIndex);
                bullseyeSearch.filterByLoS = true;
                bullseyeSearch.searchOrigin = transform.position;
                bullseyeSearch.searchDirection = transform.eulerAngles;
                bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
                bullseyeSearch.maxDistanceFilter = 9999f;
                bullseyeSearch.maxAngleFilter = 361f;
                bullseyeSearch.RefreshCandidates();
                bullseyeSearch.FilterOutGameObject(base.gameObject);
                HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault<HurtBox>();
                if (hurtBox != null) direction = hurtBox.transform.position - transform.position;
            }
            if (direction == Vector3.zero) { Destroy(gameObject); return; };
            BulletAttack newBulletAttack = new BulletAttack
            {
                aimVector = direction,
                allowTrajectoryAimAssist = false,
                bulletCount = 1,
                cheapMultiBullet = false,
                damage = bulletAttack.damage * 2,
                damageColorIndex = bulletAttack.damageColorIndex,
                damageType = bulletAttack.damageType,
                falloffModel = bulletAttack.falloffModel,
                filterCallback = bulletAttack.filterCallback,
                force = bulletAttack.force,
                hitCallback = bulletAttack.hitCallback,
                hitEffectPrefab = bulletAttack.hitEffectPrefab,
                hitMask = bulletAttack.hitMask,
                isCrit = bulletAttack.isCrit,
                maxDistance = 9999f,
                maxSpread = 0f,
                minSpread = 0f,
                modifyOutgoingDamageCallback = bulletAttack.modifyOutgoingDamageCallback,
                owner = bulletAttack.owner,
                origin = transform.position,
                procChainMask = bulletAttack.procChainMask,
                procCoefficient = bulletAttack.procCoefficient,
                radius = bulletAttack.radius,
                sniper = bulletAttack.sniper,
                weapon = gameObject,
                smartCollision = bulletAttack.smartCollision,
                stopperMask = bulletAttack.stopperMask,
                tracerEffectPrefab = bulletAttack.tracerEffectPrefab,
                multiBulletOdds = 0f,
                queryTriggerInteraction = bulletAttack.queryTriggerInteraction,
                spreadPitchScale = bulletAttack.spreadPitchScale,
                spreadYawScale = bulletAttack.spreadYawScale,
            };

            newBulletAttack.Fire();
            Destroy(gameObject);
        }
    }
    public class FirstPersonCamera : MonoBehaviour
    {
        public CharacterBody characterBody;
        public ProfessionalModelComponent professionalModelComponent;
        public Camera weaponCamera;
        [HideInInspector] public PlayerCharacterMasterController playerCharacterMasterController;
        [HideInInspector] public CameraRigController cameraRigController;
        [HideInInspector] public Camera previousCameraObject;
        [HideInInspector] public Camera currentCameraObject;
        [HideInInspector] public CharacterModel characterModel;
        [HideInInspector] public HurtBoxGroup hurtBoxGroup;
        [HideInInspector] public float previousFov;
        [HideInInspector] public SceneCamera sceneCamera;
        [HideInInspector] public InputBankTest inputBankTest;
        [HideInInspector] public Transform weaponTransform;
        [HideInInspector] public Transform previousWeaponParentTransform;
        [HideInInspector] public Vector3 previousWeaponPosition;
        [HideInInspector] public static Vector3 cameraWeaponPosition = Vector3.zero;
        private bool enabled = false;
        public static float textureScale = 2f;
        private bool waitToIncreaseDetails = true;
        public void FixedUpdate()
        {
            if (!enabled)
            {
                if (waitToIncreaseDetails && Hooks.receivedMaterials)
                {
                    IncreaseDetails();
                }
                enabled = true;
                if (characterBody == null) return;
                if (!characterBody.isPlayerControlled || !Util.HasEffectiveAuthority(characterBody.networkIdentity)) return;
                if (characterBody.modelLocator && characterBody.modelLocator.modelTransform)
                {
                    characterModel = characterBody.modelLocator.modelTransform.gameObject.GetComponent<CharacterModel>();
                    if (characterModel != null)
                        characterModel.invisibilityCount++;
                    //hurtBoxGroup = characterBody.modelLocator.modelTransform.gameObject.GetComponent<HurtBoxGroup>();
                    //if(hurtBoxGroup != null)
                    //    foreach (var item in hurtBoxGroup.hurtBoxes)
                    //    {
                    //        item.gameObject.SetActive(false);
                    //    }
                }
                if (characterBody.inputBank) inputBankTest = characterBody.inputBank;
                if (characterBody.master == null) return;
                playerCharacterMasterController = characterBody.master.playerCharacterMasterController;
                if (playerCharacterMasterController == null) return;
                cameraRigController = playerCharacterMasterController.networkUser?.cameraRigController;
                if (cameraRigController == null) return;
                previousCameraObject = cameraRigController.sceneCam;
                if (previousCameraObject == null) return;
                currentCameraObject = Instantiate(previousCameraObject, transform);
                ScreenDamage screenDamage1 = currentCameraObject.GetComponent<ScreenDamage>();
                ScreenDamage screenDamage2 = previousCameraObject.GetComponent<ScreenDamage>();
                screenDamage1.cameraRigController = cameraRigController;
                screenDamage1.screenDamageCalculatorDefault = screenDamage2.screenDamageCalculatorDefault;
                screenDamage1.workingScreenDamageCalculator = screenDamage2.workingScreenDamageCalculator;
                previousFov = cameraRigController.baseFov;
                cameraRigController.baseFov = 90;
                previousCameraObject.gameObject.SetActive(false);
                cameraRigController.sceneCam = currentCameraObject;
                ProfessionalBodyComponent professionalBody = characterBody is ProfessionalBodyComponent ? characterBody as ProfessionalBodyComponent : null;
                weaponTransform = weaponCamera.transform;
                weaponCamera.enabled = true;
                previousWeaponParentTransform = weaponTransform.parent;
                previousWeaponPosition = weaponTransform.localPosition;
                weaponTransform.SetParent(currentCameraObject.transform);
                weaponTransform.localPosition = cameraWeaponPosition;
                weaponTransform.localEulerAngles = Vector3.zero;
                sceneCamera = currentCameraObject.GetComponent<SceneCamera>();
                if (sceneCamera != null)
                    sceneCamera.cameraRigController = cameraRigController;
            }
            
        }
        public void OnEnable()
        {
            if (!enabled && Hooks.receivedMaterials)
            IncreaseDetails();
        }
        private void IncreaseDetails()
        {
            waitToIncreaseDetails = false;
            IEnumerator enumerator = Details(textureScale);
            while (enumerator.MoveNext())
            {
            }
            
        }

        private IEnumerator Details(float value)
        {
            yield return null;
            foreach (var material in Main.TriPlanarMaterials)
            {
                material.SetTextureScale("_RedChannelTopTex", material.GetTextureScale("_RedChannelTopTex") * value);
                material.SetTextureScale("_RedChannelSideTex", material.GetTextureScale("_RedChannelSideTex") * value);
                material.SetTextureScale("_GreenChannelTex", material.GetTextureScale("_GreenChannelTex") * value);
                material.SetTextureScale("_BlueChannelTex", material.GetTextureScale("_BlueChannelTex") * value);
                material.SetTextureScale("_NormalTex", material.GetTextureScale("_NormalTex") * value);
                
            }
            foreach (var material in Main.SnowToppedMaterials)
            {
                material.SetTextureScale("_SnowTex", material.GetTextureScale("_SnowTex") * value);
                material.SetTextureScale("_SnowNormalTex", material.GetTextureScale("_SnowNormalTex") * value);
                material.SetTextureScale("_DirtTex", material.GetTextureScale("_DirtTex") * value);
                material.SetTextureScale("_DirtNormalTex", material.GetTextureScale("_DirtNormalTex") * value);
            }
            yield break;
        }
        private void DecreaseDetails()
        {
            IEnumerator enumerator = Details(1 / textureScale);
            while (enumerator.MoveNext())
            {
            }
        }

        public void LateUpdate()
        {
            if (!cameraRigController || !currentCameraObject) return;
            //if (inputBankTest) inputBankTest.aimDirection = cameraRigController.transform.rotation.eulerAngles;
            currentCameraObject.transform.rotation = cameraRigController.transform.rotation;
        }
        public void OnDisable()
        {
            if(!enabled) return;
            if (weaponTransform && previousWeaponParentTransform)
            {
                weaponTransform.SetParent(previousWeaponParentTransform);
                weaponTransform.localPosition = previousWeaponPosition;
                weaponTransform.localEulerAngles = Vector3.zero;
            }
            if (currentCameraObject) Destroy(currentCameraObject.gameObject);
            if (characterModel != null) characterModel.invisibilityCount--;
            if (cameraRigController) cameraRigController.baseFov = previousFov;
            if (previousCameraObject)
            {
                previousCameraObject.gameObject.SetActive(true);
                if (cameraRigController) cameraRigController.sceneCam = previousCameraObject;
            }
            weaponCamera.enabled = false;
            enabled = false;
            DecreaseDetails();
        }
    }
    public class ProjectileOverchargeComponent : MonoBehaviour
    {
        public static float maxCharge = 5f;
        private float _charge;
        public ProjectileController controller;
        public BlastAttack blastAttack;
        public EntityStateMachine entityStateMachine;
        public EntityState entityState;
        public Vector3 vel;
        public float charge
        {
            get
            {
                return _charge;
            }
            set
            {
                _charge = value;
                if (_charge > maxCharge)
                {
                    Explode();
                }
            }
        }
        public void FixedUpdate()
        {
            if (controller && controller.rigidbody)
            {
                controller.rigidbody.velocity = Vector3.SmoothDamp(controller.rigidbody.velocity, Vector3.zero, ref vel, 0.2f);
            }
            if (entityStateMachine && entityStateMachine.state != entityState) Explode();
        }
        public void ModifyBlastAttack()
        {
            blastAttack.baseDamage *= charge + 1;
            blastAttack.radius *= charge + 1;
            blastAttack.position = transform.position;
        }
        public void Explode()
        {
            ModifyBlastAttack();
            if (NetworkServer.active)
            {
                blastAttack.Fire();
            }
            EffectData effectData = new EffectData
            {
                origin = blastAttack.position,
                scale = blastAttack.radius,
            };
            EffectManager.SpawnEffect(Explosion, effectData, true);
            ProjectileExplosion projectileExplosion = GetComponent<ProjectileExplosion>();
            if (projectileExplosion)
            {
                Destroy(projectileExplosion);
            }
            Destroy(gameObject);
        }
    }
    public class UtiltiyCharges : MonoBehaviour
    {
        [HideInInspector] public int maxStocks;
        [HideInInspector] public SkillLocator skillLocator;
        [HideInInspector] public List<Image> bars = new List<Image>();
        public void FixedUpdate()
        {
            if (skillLocator == null) return;
            GenericSkill utilitySkill = skillLocator.utility;
            if (utilitySkill == null) return;
            if (maxStocks != utilitySkill.maxStock) Rebuild(utilitySkill);
            float fill = (utilitySkill.rechargeStopwatch + (utilitySkill.stock * utilitySkill.finalRechargeInterval)) / utilitySkill.finalRechargeInterval;
            foreach (Image bar in bars)
            {
                bar.fillAmount = fill > 1 ? 1 : fill;
                if( bar.fillAmount >= 1)
                {
                    bar.color = Color.green;
                }
                else
                {
                    bar.color = Color.red;
                }
                fill -= 1f;
                if(fill < 0) fill = 0;
            }
            maxStocks = utilitySkill.maxStock;
        }
        public void Rebuild(GenericSkill utilitySkill)
        {
            int amount = 0;
            bool trueToRemove = false;
            if (maxStocks > utilitySkill.maxStock)
            {
                amount = maxStocks - utilitySkill.maxStock;
                trueToRemove = true;
            }
            else
            {
                amount = utilitySkill.maxStock - maxStocks;
            }
            for (int i = 0; i < amount; i++)
            {
                if (trueToRemove)
                {
                    Image image = bars[bars.Count - 1];
                    bars.Remove(image);
                    Destroy(image.gameObject);
                }
                else
                {
                    GameObject utilityChargeObject = Instantiate(UtilitySkillCharge, transform);
                    Image image = utilityChargeObject.GetComponent<Image>();
                    bars.Add(image);
                }
            }
            foreach(Image bar in bars)
            {/*
                int j = bars.IndexOf(bar) + 1;
                Image nextImage = j < bars.Count ? bars[j] : null;
                if(nextImage != null)
                {
                    //Vector3 vector3 = nextImage.rectTransform.localPosition - bar.rectTransform.localPosition;
                    float angle = Vector3.Angle(bar.rectTransform.localPosition, nextImage.rectTransform.localPosition);
                    bar.rectTransform.localEulerAngles = new Vector3(0f, 0f, -angle);
                    //bar.rectTransform.rotation = Quaternion.LookRotation(vector3);
                }*/
                bar.rectTransform.sizeDelta = new Vector2(8, 16);//75 / bars.Count);
                bar.color = Color.red;
            }
        }
    }
    
    public class HookTracker : MonoBehaviour
    {
        public FixedConditionalWeakTable<Collider, CharacterBody> keyValuePairs = new FixedConditionalWeakTable<Collider, CharacterBody>();
        public CharacterBody targetBody;
        public CharacterBody ownerBody;
        public InputBankTest inputBankTest;
        public static float radius = 5f;
        public static float range = 64f;
        public bool active;
        public static GameObject indicatorPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/HuntressTrackingIndicator");
        public Indicator indicator;
        public void Awake()
        {
            indicator = new Indicator(gameObject, indicatorPrefab);
        }
        public void FixedUpdate()
        {
            if (!active) { targetBody = null; return; };
            Ray ray = GetAimRay();
            Collider[] colliders = Physics.OverlapCapsule(ray.origin + ray.direction * radius, ray.origin + ray.direction * range, radius, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);
            float newRange = range * range;
            CharacterBody characterBody1 = null;
            foreach (Collider collider in colliders)
            {
                CharacterBody characterBody = null;
                if (keyValuePairs.TryGetValue(collider, out characterBody))
                {
                }
                else
                {
                    HurtBox hurtBox = collider.GetComponent<HurtBox>();
                    characterBody = hurtBox && hurtBox.healthComponent ? hurtBox.healthComponent.body : null;
                    keyValuePairs.Add(collider, characterBody);
                }
                if (characterBody != null && ownerBody != characterBody)
                {
                    float newRange2 = (characterBody.corePosition - ray.origin).sqrMagnitude;
                    if (newRange2 < newRange)
                    {
                        newRange = newRange2;
                        characterBody1 = characterBody;
                    }
                }
            }
            targetBody = characterBody1;
        }
        public void Update()
        {
            if (active)
            {
                indicator.active = true;
            }
            else
            {
                indicator.active = false;
            }
            indicator.targetTransform = targetBody ? targetBody.transform : null;
        }
        public Ray GetAimRay()
        {
            return new Ray
            {
                origin = inputBankTest ? inputBankTest.aimOrigin : transform.position,
                direction = inputBankTest ? inputBankTest.aimDirection : transform.forward,
            };
        }
    }
    [CreateAssetMenu(menuName = "RoR2/SkillDef/Professional")]
    public class ProfessionalSkillDef : SteppedSkillDef
    {
        public SkillDef secondarySkillDef;
        public WeaponModelDef weaponModel;
    }
    [CreateAssetMenu(menuName = "RoR2/SkillDef/HeroSpecial")]
    public class HeroSpecialSkillDef : SkillDef
    {
        public SkillDef[] skillsSelection;
    }
    [CreateAssetMenu(menuName = "RoR2/SkillDef/HookTracking")]
    public class HookTarckingSkillDef : ProfessionalSkillDef
    {
        public override SkillDef.BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            HookTracker hookTracker = skillSlot.GetOrAddComponent<HookTracker>();
            hookTracker.ownerBody = skillSlot.characterBody;
            hookTracker.inputBankTest = skillSlot.characterBody.inputBank;
            return new HookTarckingSkillDef.InstanceData
            {
                hookTracker = hookTracker
            };
        }
        private static bool HasTarget([NotNull] GenericSkill skillSlot)
        {
            HookTracker hookTracker = ((HookTarckingSkillDef.InstanceData)skillSlot.skillInstanceData).hookTracker;
            if (hookTracker == null) return false;
            if (skillSlot.characterBody.skillLocator && skillSlot.characterBody.skillLocator.primary == skillSlot || skillSlot.characterBody.skillLocator.secondary == skillSlot || skillSlot.characterBody.skillLocator.utility == skillSlot || skillSlot.characterBody.skillLocator.special == skillSlot)
            {
                hookTracker.active = true;
            }
            else
            {
                hookTracker.active = false;
            }
            return hookTracker.targetBody;
        }
        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return HookTarckingSkillDef.HasTarget(skillSlot) && base.CanExecute(skillSlot);
        }
        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && HookTarckingSkillDef.HasTarget(skillSlot);
        }
        protected class InstanceData : SteppedSkillDef.InstanceData
        {
            public HookTracker hookTracker;
        }
    }
    
    public class WeaponModel : MonoBehaviour
    {
        public Transform muzzleTransform;
    }
    [CreateAssetMenu(menuName = "GordonFreeman/WeaponModelDef")]
    public class WeaponModelDef : ScriptableObject
    {
        public WeaponModel weaponModel;
        public WeaponModel ApplyWeaponModel(ProfessionalModelComponent professionalModelComponent)
        {
            WeaponModel newWeaponModel = Instantiate(weaponModel, professionalModelComponent.weaponsTransform);
            newWeaponModel.gameObject.SetActive(false);
            return newWeaponModel;
        }
    }
}


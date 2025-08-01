using System;
using FIMSpace.FProceduralAnimation;
using UnityEngine;
public class PlayerManager : MonoBehaviour
{
    [Header("Data")]
    public AnimationClip idleAnimationClip;
    public AnimationClip airFallAnimationClip;
    public AnimationClip groundLandAnimationClip;
    public AnimationClip deathAnimationClip;
    [Header("Controller")]
    public Animator characterAnimator;
    public LegsAnimator characterLegs;
    public Transform characterTransform;
    public Rigidbody characterRigid;
    public CapsuleCollider capsuleCollider;
    public LayerMask groundLayer;
    public float SpringLength;
    [Range(0.1f, 1f)] public float SpringRestRatio;
    public float SpringStrength;
    public float SpringDamping;
    public float UprightStrength;
    public float UprightDamping;
    public float JumpStrength;
    public float AdditiveInputForce;
    public float MaxSpeed;
    public float MaxAccel;
    public float MaxForce;
    public AnimationCurve ForceDotFactor;
    //
    private float springForce;
    private RaycastHit raycastHit;
    private Vector3 cameraPlanarDirection;
    private Quaternion cameraPlanarRotation;
    private Vector3 inputVector;
    private Vector3 inputVectorLerp;
    private Vector3 inputMoveVector;
    private Vector3 extraMoveVector;
    private Quaternion inputLookRotation;
    private InputCache inputCache;
    private Vector3 desiredVelocityCache;
    private string animEventParameterCache;
    private float animEventParameterValue;
    private float disableGroundSpringTimer;
    private int groundJumpCounter;
    private GameManager manager;
    //
    public void Setup(GameManager gameManager)
    {
        manager = gameManager;
    }
    //
    public void ProcessPhysics()
    {
        if (inputCache != null) { UpdateMovementForce(); }
        UpdateSpringForce();
        UpdateUprightForce();
        UpdatePhysicsState();
    }
    //
    public void UpdateSpringForce()
    {
        Physics.Raycast(characterTransform.position, Vector3.down, out raycastHit, SpringLength, groundLayer);
        //
        if (raycastHit.transform == null)
        {
            characterRigid.AddForce(Physics.gravity * 1.4f, ForceMode.Acceleration);
        }
        else
        {
            characterRigid.AddForce(Physics.gravity * 0.8f, ForceMode.Acceleration);
        }
        if (disableGroundSpringTimer > 0f)
        {
            disableGroundSpringTimer -= Time.fixedDeltaTime;
            return;
        }
        //
        if (raycastHit.transform != null)
        {
            Vector3 currentVelocity = characterRigid.linearVelocity;
            Vector3 springDirection = Vector3.down.normalized;
            Vector3 contactVelocity = Vector3.zero;
            Rigidbody contactBody = raycastHit.rigidbody;
            if (contactBody != null) { contactVelocity = contactBody.linearVelocity; }
            //
            float currentVelocityAlongSpring = Vector3.Dot(springDirection, currentVelocity);
            float contactVelocityAlongSpring = Vector3.Dot(springDirection, contactVelocity);
            float finalVelocityAlongSpring = currentVelocityAlongSpring - contactVelocityAlongSpring;
            //
            float springOffset = raycastHit.distance - (SpringRestRatio * SpringLength);
            springForce = (springOffset * SpringStrength) - (finalVelocityAlongSpring * SpringDamping);
            //
            characterRigid.AddForce(springDirection * springForce);
            //
            if (contactBody != null)
            {
                contactBody.AddForceAtPosition(springDirection * -springForce, raycastHit.point, ForceMode.Force);
            }
        }
    }
    //
    public void UpdateUprightForce()
    {
        Quaternion current = characterTransform.rotation;
        Quaternion goal = UtilityManager.ShortestRotation(cameraPlanarRotation, current);

        Vector3 rotAxis;
        float rotDegrees;

        goal.ToAngleAxis(out rotDegrees, out rotAxis);
        rotAxis.Normalize();

        characterRigid.AddTorque((rotAxis * (rotDegrees * Mathf.Deg2Rad * UprightStrength)) - (characterRigid.angularVelocity * UprightDamping), ForceMode.Force);
    }
    //
    public void UpdateMovementForce()
    {
        // Calculate camera direction and rotation on the character plane
        cameraPlanarDirection = Vector3.ProjectOnPlane(inputLookRotation * Vector3.forward, Vector3.up).normalized;
        if (cameraPlanarDirection.sqrMagnitude == 0f)
        {
            cameraPlanarDirection = Vector3.ProjectOnPlane(inputLookRotation * Vector3.up, Vector3.up).normalized;
        }
        cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Vector3.up);
        inputMoveVector = (cameraPlanarRotation * (inputCache.runActionHold ? inputVector * 2f : inputVector)).normalized;

        float velocityDot = Vector3.Dot(inputMoveVector, desiredVelocityCache.normalized);
        float accelDot = (inputCache.runActionHold ? MaxAccel * 2f : MaxAccel) * ForceDotFactor.Evaluate(velocityDot);
        Vector3 peakVelocity = inputMoveVector * (inputCache.runActionHold ? MaxSpeed * 2f : MaxSpeed);

        desiredVelocityCache = Vector3.MoveTowards(desiredVelocityCache, peakVelocity, accelDot * Time.fixedDeltaTime);
        Vector3 desiredAccelCache = (desiredVelocityCache - characterRigid.linearVelocity) / Time.fixedDeltaTime;
        float forceDot = MaxForce * ForceDotFactor.Evaluate(velocityDot);
        desiredAccelCache = Vector3.ClampMagnitude(desiredAccelCache, forceDot);
        characterRigid.AddForce(desiredAccelCache, ForceMode.Force);
        //
        if (extraMoveVector.sqrMagnitude > 0.1f)
        {
            characterRigid.AddForce(AdditiveInputForce * extraMoveVector, ForceMode.Force);
            extraMoveVector = Vector3.Lerp(extraMoveVector, Vector3.zero, Time.fixedDeltaTime * 4f);
        }
        else
        {
            extraMoveVector = Vector3.zero;
        }
    }
    //
    public void UpdatePhysicsState()
    {
        if (raycastHit.transform != null)
        {
            characterAnimator.SetBool("IsGrounded", true);
            groundJumpCounter = 2;
        }
        else
        {
            characterAnimator.SetBool("IsGrounded", false);
        }
        //
        if (characterRigid.linearVelocity.sqrMagnitude > 0.4f || inputMoveVector.sqrMagnitude > 0.4f)
        {
            characterAnimator.SetBool("IsMoving", true);
        }
        else
        {
            characterAnimator.SetBool("IsMoving", false);
        }
        //
        if (!characterAnimator.GetBool("IsGrounded"))
        {
            if (characterRigid.linearVelocity.sqrMagnitude > 0.8f && Mathf.Abs(characterRigid.linearVelocity.y) > 0.8f)
            {
                characterAnimator.SetBool("IsFalling", true);
            }
            else
            {
                characterAnimator.SetBool("IsFalling", false);
            }
        }
        else
        {
            characterAnimator.SetBool("IsFalling", false);
        }
        //
        characterAnimator.SetFloat("LocomotionX", (inputVectorLerp.x));
        characterAnimator.SetFloat("LocomotionZ", (inputVectorLerp.z));
    }
    //
    public void SetInput(InputCache cache)
    {
        inputCache = cache;

        // Move Input
        inputVector = inputCache.inputVector;
        inputCache.inputVector = Vector3.zero;
        if (inputCache.runActionHold)
        {
            inputVectorLerp = Vector3.Lerp(inputVectorLerp, inputVector * 2f, Time.deltaTime * 8f);
        }
        else
        {
            inputVectorLerp = Vector3.Lerp(inputVectorLerp, inputVector, Time.deltaTime * 8f);
        }

        // Rotation Input
        inputLookRotation = inputCache.lookRotation;
        inputCache.lookRotation = Quaternion.identity;

        // Click Input
        if (inputCache.jumpActionClick)
        {
            // Jump Input
            if (groundJumpCounter > 0)
            {
                if (characterAnimator.GetBool("IsGrounded") && groundJumpCounter == 2)
                {
                    groundJumpCounter--;
                    DetachGround();
                    extraMoveVector += Vector3.up * JumpStrength;
                    disableGroundSpringTimer = 0.2f;
                    characterAnimator.SetTrigger("JumpTrigger");
                }
                else
                {
                    if (characterRigid.linearVelocity.y < 0f)
                    {
                        groundJumpCounter--;
                        DetachGround();
                        extraMoveVector += 1.28f * JumpStrength * Vector3.up;
                        disableGroundSpringTimer = 0.2f;
                        characterAnimator.SetTrigger("JumpTrigger");
                    }
                }
            }
            inputCache.jumpActionClick = false;
        }
        else if (inputCache.primaryActionClick)
        {
            // Primary Input
            DetachGround();
            extraMoveVector += cameraPlanarDirection * JumpStrength * 0.8f;
            disableGroundSpringTimer = 0.16f;
            characterAnimator.SetTrigger("DodgeTrigger");
            inputCache.primaryActionClick = false;
        }
        else if (inputCache.secondaryActionClick)
        {
            // Secondary Input
            // DisableHitbox();
            // switch (secondaryAttribute.animIndex)
            // {
            //     case 0:
            //         characterAnimator.CrossFade("Secondary-0", 0.064f, 0, 0f);
            //         animatorSlotFocused = CombatAnimationSlot.Secondary0;
            //         break;
            //     case 1:
            //         characterAnimator.CrossFade("Secondary-1", 0.064f, 0, 0f);
            //         animatorSlotFocused = CombatAnimationSlot.Secondary1;
            //         break;
            //     case 2:
            //         characterAnimator.CrossFade("Secondary-2", 0.064f, 0, 0f);
            //         animatorSlotFocused = CombatAnimationSlot.Secondary2;
            //         break;
            //     case 3:
            //         characterAnimator.CrossFade("Secondary-3", 0.064f, 0, 0f);
            //         animatorSlotFocused = CombatAnimationSlot.Secondary3;
            //         break;
            // }
            // animatorLastSkillCache = secondaryAttribute;
            // energyAttribute.ChangeValue(-secondaryAttribute.energyRequired);
            // secondaryAttribute.TriggerCooldown();
            // GlobalCooldown();
            // CheckChargeHoldTimer();
            // inputCache.secondaryActionClick = false;
        }
        else if (inputCache.skillOneClick)
        {
            // Skill 1 Input
            // DisableHitbox();
            // characterAnimator.CrossFade("SkillOne-0", 0.064f, 0, 0f);
            // animatorSlotFocused = CombatAnimationSlot.SkillOne0;
            // animatorLastSkillCache = skillOneAttribute;
            // energyAttribute.ChangeValue(-skillOneAttribute.energyRequired);
            // skillOneAttribute.TriggerCooldown();
            // GlobalCooldown();
            // CheckChargeHoldTimer();
            // inputCache.skillOneClick = false;
        }
        else if (inputCache.skillTwoClick)
        {
            // Skill 2 Input
            // DisableHitbox();
            // characterAnimator.CrossFade("SkillTwo-0", 0.064f, 0, 0f);
            // animatorSlotFocused = CombatAnimationSlot.SkillTwo0;
            // animatorLastSkillCache = skillTwoAttribute;
            // energyAttribute.ChangeValue(-skillTwoAttribute.energyRequired);
            // skillTwoAttribute.TriggerCooldown();
            // GlobalCooldown();
            // CheckChargeHoldTimer();
            // inputCache.skillTwoClick = false;
        }
        else if (inputCache.skillThreeClick)
        {
            // Skill 3 Input
            // DisableHitbox();
            // characterAnimator.CrossFade("SkillThree-0", 0.064f, 0, 0f);
            // animatorSlotFocused = CombatAnimationSlot.SkillThree0;
            // animatorLastSkillCache = skillThreeAttribute;
            // energyAttribute.ChangeValue(-skillThreeAttribute.energyRequired);
            // skillThreeAttribute.TriggerCooldown();
            // GlobalCooldown();
            // CheckChargeHoldTimer();
            // inputCache.skillThreeClick = false;
        }

        // Hold And Release Input
        // if (animHoldChargeAttribute == animatorLastSkillCache && animHoldChargeToken)
        // {
        //     switch (animHoldChargeAttribute.attributeType)
        //     {
        //         case SkillAttributeType.Primary:
        //             animHoldChargeInputCache = inputCache.primaryActionRelease;
        //             break;
        //         case SkillAttributeType.Secondary:
        //             animHoldChargeInputCache = inputCache.secondaryActionRelease;
        //             break;
        //         case SkillAttributeType.SkillOne:
        //             animHoldChargeInputCache = inputCache.skillOneRelease;
        //             break;
        //         case SkillAttributeType.SkillTwo:
        //             animHoldChargeInputCache = inputCache.skillTwoRelease;
        //             break;
        //         case SkillAttributeType.SkillThree:
        //             animHoldChargeInputCache = inputCache.skillThreeRelease;
        //             break;
        //         case SkillAttributeType.SkillFour:
        //             animHoldChargeInputCache = inputCache.skillFourRelease;
        //             break;
        //     }

        //     if (!animHoldChargeInputCache)
        //     {
        //         if (animHoldChargeAttribute.IsCombatHoldType())
        //         {
        //             if (animHoldChargeAttribute.EnergyRateAvailable(energyAttribute.value))
        //             {
        //                 characterAnimator.SetBool("IsLooping", true);
        //                 energyAttribute.ChangeValue(-animHoldChargeAttribute.energyRequired * Time.deltaTime);
        //             }
        //             else
        //             {
        //                 EndChargeHoldTimer("Out Of Energy");
        //             }
        //         }
        //         else
        //         {
        //             characterAnimator.speed = Mathf.Lerp(characterAnimator.speed, 0.1f, 1f - (1f - Time.deltaTime) * (1f - Time.deltaTime));
        //         }
        //         //
        //         animHoldChargeTimer += Time.deltaTime * characterAnimator.speed;
        //     }
        //     else
        //     {
        //         EndChargeHoldTimer("Manual Release");
        //     }
        // }
    }
    //
    public void AdvanceTimer()
    {
        // skillOneAttribute.AdvanceTimer(Time.deltaTime);
        // skillTwoAttribute.AdvanceTimer(Time.deltaTime);
        // skillThreeAttribute.AdvanceTimer(Time.deltaTime);
        // skillFourAttribute.AdvanceTimer(Time.deltaTime);
    }
    public void GlobalCooldown()
    {
        // skillOneAttribute.GlobalCooldown(animatorLastSkillCache.cooldownActivePeriod);
        // skillTwoAttribute.GlobalCooldown(animatorLastSkillCache.cooldownActivePeriod);
        // skillThreeAttribute.GlobalCooldown(animatorLastSkillCache.cooldownActivePeriod);
        // skillFourAttribute.GlobalCooldown(animatorLastSkillCache.cooldownActivePeriod);
    }
    //
    public void DetachGround()
    {
        characterAnimator.SetBool("IsGrounded", false);
        characterLegs.User_AddImpulse(characterLegs.DebugPushHipsImpulse);
    }
    public void AddExternalForce(Vector3 externalForce)
    {
        DetachGround();
        extraMoveVector += externalForce;
        disableGroundSpringTimer = 0.64f;
        characterAnimator.SetTrigger("JumpTrigger");
    }
    //
    public void OnCharacterAnimEvent(string stringParam)
    {
        // animEventSlotCache = stringParam.Split("|")[0];
        // animEventParameterCache = stringParam.Split("|")[1];
        //
        // if (animatorSlotFocused.ToString() != animEventSlotCache)
        // {
        //     Debug.Log(stringParam + " | PREVENTED" + "-->" + animEventSlotCache + "," + animatorSlotFocused.ToString());
        //     return;
        // }
        //
        if (animEventParameterCache == "ChargeEnd")
        {
            // EndChargeHoldTimer("Full Charge");
        }
        else
        {
            // Combat Forces
            animEventParameterValue = float.Parse(animEventParameterCache.Split("-")[1]);
            // extraMoveVector = Vector3.zero;
            switch (animEventParameterCache.Split("-")[0])
            {
                case "ForceB":
                    extraMoveVector += animEventParameterValue * -cameraPlanarDirection;
                    break;
                case "ForceF":
                    extraMoveVector += animEventParameterValue * cameraPlanarDirection;
                    break;
                case "ForceR":
                    extraMoveVector += animEventParameterValue * (Quaternion.AngleAxis(90f, Vector3.up) * cameraPlanarDirection);
                    break;
                case "ForceL":
                    extraMoveVector += animEventParameterValue * (Quaternion.AngleAxis(-90f, Vector3.up) * cameraPlanarDirection);
                    break;
                case "ForceM":
                    extraMoveVector += animEventParameterValue * inputMoveVector.normalized;
                    break;
                case "ForceD":
                    extraMoveVector += animEventParameterValue * JumpStrength * Vector3.down;
                    break;
                case "ForceFL":
                    extraMoveVector += animEventParameterValue * ((Quaternion.AngleAxis(-90f, Vector3.up) * cameraPlanarDirection) + cameraPlanarDirection);
                    break;
                case "ForceDB":
                    extraMoveVector += animEventParameterValue * (-cameraPlanarDirection + 0.4f * JumpStrength * Vector3.down);
                    break;
                case "ForceDF":
                    extraMoveVector += animEventParameterValue * (cameraPlanarDirection + 0.4f * JumpStrength * Vector3.down);
                    break;
                case "ForceU":
                    DetachGround();
                    extraMoveVector += animEventParameterValue * JumpStrength * Vector3.up;
                    break;
                case "ForceUB":
                    DetachGround();
                    extraMoveVector += animEventParameterValue * (Vector3.up * JumpStrength - cameraPlanarDirection);
                    break;
                case "ForceUF":
                    DetachGround();
                    extraMoveVector += animEventParameterValue * (Vector3.up * JumpStrength + cameraPlanarDirection);
                    break;
            }
        }
    }
    public void OnHitboxDetection(PlayerManager receiver, float value)
    {
        receiver.OnGetHit(-value);
    }
    //
    public void OnGetHit(float changeValue)
    {
        // if (changeValue != 0f)
        // {
        //     healthAttribute.ChangeValue(changeValue);
        //     manager.OnTextSpawn(this, -0.1f, Color.red, changeValue.ToString());
        // }
        // //
        // if (healthAttribute.value > 0f)
        // {
        //     disableMovementTimer = 0.16f;
        //     extraMoveVector += -transform.forward * 1f;
        //     characterLegs?.User_AddImpulse(characterLegs.DebugPushHipsImpulse);
        //     if (getHitAnimToken)
        //     {
        //         getHitAnimToken = !getHitAnimToken;
        //         characterAnimator.CrossFade("Interrupted-0", 0.1f);
        //     }
        //     else
        //     {
        //         getHitAnimToken = !getHitAnimToken;
        //         characterAnimator.CrossFade("Interrupted-1", 0.1f);
        //     }
        //     stepAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        //     stepAudioSource.PlayOneShot(getHitSound);
        // }
        // else
        // {
        //     disableMovementTimer = 0.16f;
        //     extraMoveVector += -transform.forward * 1.6f;
        //     characterAnimator.CrossFade("Death-0", 0.1f);
        //     stepAudioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        //     if (characterLegs != null) { characterLegs.enabled = false; }
        //     stepAudioSource.PlayOneShot(getHitSound);
        //     capsuleCollider.enabled = false;
        //     manager.OnEntityDeath(this);
        // }
    }
}
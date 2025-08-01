using UnityEngine;
public class InputManager : MonoBehaviour
{
    public MainInputSystem mainInput;
    public InputCache InputCache { get; set; }
    //
    private Vector2 vectorCache;
    //
    public void Setup()
    {
        InputCache = new InputCache();
        mainInput = new MainInputSystem();
        mainInput.Enable();
        mainInput.Player.Enable();
    }
    //
    public void ProcessInput(PlayerManager character, Transform camera)
    {
        // Movement - Manual
        InputCache.inputVector = Vector3.zero;
        vectorCache = mainInput.Player.Move.ReadValue<Vector2>();
        InputCache.inputVector.x = vectorCache.x;
        InputCache.inputVector.z = vectorCache.y;
        InputCache.inputVector = InputCache.inputVector.normalized;

        // Look - Cinemachine
        InputCache.lookRotation = camera.rotation;

        // Zoom - Manual
        vectorCache = mainInput.Player.Zoom.ReadValue<Vector2>();
        InputCache.zoomDelta = vectorCache.y;

        // Interact
        InputCache.interactClick = mainInput.Player.Interact.WasPressedThisFrame();
        InputCache.jumpActionClick = mainInput.Player.Impulse.WasPressedThisFrame();

        // // Press Hold Action
        // InputCache.primaryActionHold = mainInput.Player.Primary.IsPressed();
        // InputCache.secondaryActionHold = mainInput.Player.Secondary.IsPressed();
        // InputCache.skillOneHold = mainInput.Player.SkillOne.IsPressed();
        // InputCache.skillTwoHold = mainInput.Player.SkillTwo.IsPressed();
        // InputCache.skillThreeHold = mainInput.Player.SkillThree.IsPressed();

        // // Press Release Action
        // InputCache.primaryActionRelease = mainInput.Player.Primary.WasReleasedThisFrame();
        // InputCache.secondaryActionRelease = mainInput.Player.Secondary.WasReleasedThisFrame();
        // InputCache.skillOneRelease = mainInput.Player.SkillOne.WasReleasedThisFrame();
        // InputCache.skillTwoRelease = mainInput.Player.SkillTwo.WasReleasedThisFrame();
        // InputCache.skillThreeRelease = mainInput.Player.SkillThree.WasReleasedThisFrame();

        // if (mainInput.Player.Impulse.WasPressedThisFrame() && character.impulseAttribute.SkillAvailable(character.energyAttribute.value))
        // {
        //     // Impulse
        //     InputCache.impulseActionClick = true;
        // }
        // else if (mainInput.Player.Primary.WasPressedThisFrame() && character.primaryAttribute.SkillAvailable(character.energyAttribute.value) && character.AvailableForSkill())
        // {
        //     // Primary Action - Mainly Click
        //     InputCache.primaryActionClick = true;
        // }
        // else if (mainInput.Player.Secondary.WasPressedThisFrame() && character.secondaryAttribute.SkillAvailable(character.energyAttribute.value) && character.AvailableForSkill())
        // {
        //     // Secondary Action - Mainly Click
        //     InputCache.secondaryActionClick = true;
        // }
        // else if (mainInput.Player.SkillOne.WasPressedThisFrame() && character.skillOneAttribute.SkillAvailable(character.energyAttribute.value) && character.AvailableForSkill())
        // {
        //     // Skill - 1
        //     InputCache.skillOneClick = true;
        // }
        // else if (mainInput.Player.SkillTwo.WasPressedThisFrame() && character.skillTwoAttribute.SkillAvailable(character.energyAttribute.value) && character.AvailableForSkill())
        // {
        //     // Skill - 2
        //     InputCache.skillTwoClick = true;
        // }
        // else if (mainInput.Player.SkillThree.WasPressedThisFrame() && character.skillThreeAttribute.SkillAvailable(character.energyAttribute.value) && character.AvailableForSkill())
        // {
        //     // Skill - 3
        //     InputCache.skillThreeClick = true;
        // }
        // else if (mainInput.Player.SkillFour.WasPressedThisFrame() && character.skillFourAttribute.SkillAvailable(character.energyAttribute.value) && character.AvailableForSkill())
        // {
        //     // Skill - 4
        //     InputCache.skillFourClick = true;
        // }
        //
        character.AdvanceTimer();
        character.SetInput(InputCache);
    }
    //
    public void ResetInput(Transform camera)
    {
        // Movement - Manual
        InputCache.inputVector = Vector3.zero;

        // Look - Cinemachine
        InputCache.lookRotation = camera.rotation;
    }
}
//
public class InputCache
{
    public Vector3 inputVector;
    public Vector3 lookVector;
    public Quaternion lookRotation;
    public float zoomDelta;
    public bool impulseActionClick;
    public bool jumpActionClick;
    public bool primaryActionClick;
    public bool primaryActionHold;
    public bool primaryActionRelease;
    public bool secondaryActionClick;
    public bool secondaryActionHold;
    public bool secondaryActionRelease;
    public bool skillOneClick;
    public bool skillOneHold;
    public bool skillOneRelease;
    public bool skillTwoClick;
    public bool skillTwoHold;
    public bool skillTwoRelease;
    public bool skillThreeClick;
    public bool skillThreeHold;
    public bool skillThreeRelease;
    public bool interactClick;
}
using Unity.Cinemachine;
using UnityEngine;
public class CameraManager : MonoBehaviour
{
    public Camera mainCamera;
    public Transform mainCameraTrans;
    public CinemachineCamera playerCamera;
    public CinemachineOrbitalFollow orbitalFollow;
    public CinemachineRotationComposer rotationComposer;
    public CinemachineCamera characterFreeCamera;
    public Transform menuCamera;
    //
    private float targetRadius;
    //
    public void Setup(PlayerManager character)
    {
        targetRadius = orbitalFollow.Radius;
        characterFreeCamera.Target.TrackingTarget = character.transform.GetChild(0);
    }
    //
    public void ProcessInput(InputCache input)
    {
        targetRadius -= input.zoomDelta * Time.deltaTime;
        //
        targetRadius = Mathf.Clamp(targetRadius, 4f, 8f);
        //
        orbitalFollow.Radius = Mathf.MoveTowards(orbitalFollow.Radius,
            targetRadius,
            4f * Time.deltaTime);
    }
}
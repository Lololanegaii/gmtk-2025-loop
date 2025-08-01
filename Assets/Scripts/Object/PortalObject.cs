using DG.Tweening;
using UnityEngine;
public class PortalObject : MonoBehaviour, ObjectInteractionInterface
{
    public GameObject portalVFX;
    public Collider portalTrigger;
    public bool portalTeleport;
    public Vector3 portalTeleportCoordinate;
    public PortalObject portalLinkedPair;
    //
    public bool AutoInteract { get; set; }
    //
    public void EnableInteraction()
    {
        AutoInteract = true;
        portalVFX.SetActive(true);
        portalTrigger.enabled = true;
    }
    public void DisableInteraction()
    {
        portalVFX.SetActive(false);
        portalTrigger.enabled = false;
    }
    public void OnInteractEnter()
    {
        float timer = 0f;
        bool jumpToken = false;
        GameManager.Instance.audioManager.PlayAudio(GameManager.Instance.audioManager.portalEnter, 0.64f);
        DOTween.To(() => timer, x => timer = x, 2f, 2f).OnUpdate(() =>
            {
                if (timer >= 1f)
                {
                    if (!jumpToken)
                    {
                        jumpToken = true;
                        DisableInteraction();
                        GameManager.Instance.audioManager.PlayAudio(GameManager.Instance.audioManager.portalExit, 0.64f);
                        if (portalTeleport)
                        {
                            GameManager.Instance.playerManager.transform.position = portalTeleportCoordinate;
                        }
                        else
                        {
                            GameManager.Instance.playerManager.AddExternalForce((portalTeleportCoordinate - transform.position) * 0.1f);
                        }
                        GameManager.Instance.SetGameState(1);
                    }
                    //
                    GameManager.Instance.cameraManager.playerCamera.Lens.FieldOfView = 60f + (2f - timer) * 60f;
                    GameManager.Instance.playerManager.characterLegs.LegsAnimatorBlend = (timer - 1f);
                }
                else
                {
                    GameManager.Instance.playerManager.characterLegs.LegsAnimatorBlend = (1f - timer);
                    GameManager.Instance.cameraManager.playerCamera.Lens.FieldOfView = 60f + timer * 60f;
                }
            }).OnComplete(() =>
            {
                GameManager.Instance.cameraManager.playerCamera.Lens.FieldOfView = 60f;
                GameManager.Instance.playerManager.characterLegs.LegsAnimatorBlend = 1f;
                portalLinkedPair.EnableInteraction();
            });
    }
    public void OnInteractExit()
    {

    }
}

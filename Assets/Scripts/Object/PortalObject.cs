using UnityEngine;
public class PortalObject : MonoBehaviour
{
    public GameObject portalVFX;
    public Collider portalTrigger;
    public Collider portalCollider;
    public LayerMask portalLayer;
    //
    public void ActivatePortal()
    {
        portalVFX.SetActive(true);
        portalTrigger.enabled = true;
        portalCollider.enabled = true;
    }
    public void DisablePortal()
    {
        portalVFX.SetActive(false);
        portalTrigger.enabled = false;
        portalCollider.enabled = false;
    }
}

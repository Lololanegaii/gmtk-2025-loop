using UnityEngine;
public class ShopObject : MonoBehaviour, ObjectInteractionInterface
{
    public GameObject shopVFX;
    //
    public bool AutoInteract { get; set; }
    //
    public void EnableInteraction()
    {
        AutoInteract = false;
        shopVFX.SetActive(true);
    }
    public void DisableInteraction()
    {
        shopVFX.SetActive(false);
    }
    public void OnInteractEnter()
    {
        Debug.Log("Interacted With Shop");
        DisableInteraction();
        GameManager.Instance.SetGameState(1);

    }
    public void OnInteractExit()
    {
    }
}

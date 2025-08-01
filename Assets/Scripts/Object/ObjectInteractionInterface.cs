using UnityEngine;

public interface ObjectInteractionInterface
{
    public bool AutoInteract { get; set; }
    void EnableInteraction();
    void DisableInteraction();
    void OnInteractEnter();
    void OnInteractExit();
}

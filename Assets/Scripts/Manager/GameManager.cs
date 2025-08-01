using DG.Tweening;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    [Header("Managers")]
    public InputManager inputManager;
    public AudioManager audioManager;
    public CameraManager cameraManager;
    public CanvasManager canvasManager;
    public PlayerManager playerManager;
    //
    [Header("Objects")]
    public LayerMask objectInteractionLayer;
    public PortalObject hubPortal;
    public ShopObject hubShop;
    //
    private Collider[] colliderCache;
    private ObjectInteractionInterface interactionInterfaceCache;
    private int gameState;
    //
    public static GameManager Instance;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        inputManager.Setup();
        cameraManager.Setup(playerManager);
        canvasManager.Setup();
        playerManager.Setup(this);
        colliderCache = new Collider[1];
        SetGameState(0);
    }
    //
    void Update()
    {
        if (gameState == 0) { return; }
        inputManager.ProcessInput(playerManager, cameraManager.mainCameraTrans);
        ProcessObjectInteraction();
    }
    void FixedUpdate()
    {
        playerManager.ProcessPhysics();
    }
    //
    void LateUpdate()
    {
        if (gameState == 0) { return; }
        if (!playerManager.IsAlive) { return; }
        cameraManager.ProcessInput(inputManager.InputCache);
    }
    //
    public void OnGameStart()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraManager.menuCamera.gameObject.SetActive(false);
        cameraManager.orbitalFollow.gameObject.SetActive(true);
        SetGameState(1);
        hubShop.EnableInteraction();
        hubPortal.EnableInteraction();
    }
    public void ProcessObjectInteraction()
    {
        if (gameState == 2) { return; }
        if (!playerManager.IsAlive) { return; }
        if (Physics.OverlapSphereNonAlloc(playerManager.transform.position + playerManager.transform.forward * 0.2f, 0.5f, colliderCache, objectInteractionLayer) > 0)
        {
            interactionInterfaceCache = colliderCache[0].GetComponent<ObjectInteractionInterface>();
            //
            if (inputManager.InputCache.interactClick || interactionInterfaceCache.AutoInteract)
            {
                SetGameState(2);
                interactionInterfaceCache.OnInteractEnter();
            }
        }
    }
    public void SetGameState(int state) { gameState = state; }
}
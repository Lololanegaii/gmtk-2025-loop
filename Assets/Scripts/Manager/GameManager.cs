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
    //
    [Header("Respawn")]
    public Transform respawnPoint; // Drag your spawn point here in inspector
    public float respawnDelay = 2f; // How long to wait before respawn
    public float lastRespawnTime; // ADD THIS LINE
    private float respawnTimer;



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

        // NEW RESPAWN CODE - Add this section:
        if (!playerManager.IsAlive)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnDelay)
            {
                RespawnPlayer();
            }
            return; // Don't process other input when dead
        }
        // END NEW CODE

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

    public void RespawnPlayer()
    {
        // Reset player position
        if (respawnPoint != null)
        {
            playerManager.transform.position = respawnPoint.position;
            playerManager.transform.rotation = respawnPoint.rotation;
        }
        else
        {
            // Fallback - respawn at current position but higher up
            Vector3 safePos = playerManager.transform.position;
            safePos.y += 5f;
            playerManager.transform.position = safePos;
        }

        // ADD THIS LINE - Reset camera to follow the respawned player
        cameraManager.orbitalFollow.ForceCameraPosition(playerManager.transform.position, Quaternion.identity);

        // Reset player state
        playerManager.IsAlive = true;
        playerManager.characterRigid.linearVelocity = Vector3.zero;
        playerManager.characterRigid.angularVelocity = Vector3.zero;
        playerManager.characterAnimator.SetTrigger("JumpTrigger"); // Reset animation

        // Reset timer
        respawnTimer = 0f;
        lastRespawnTime = Time.time; // ADD THIS LINE
    }
}
using DG.Tweening;
using UnityEngine;
public class GameManager : MonoBehaviour
{
    public InputManager inputManager;
    public AudioManager audioManager;
    public CameraManager cameraManager;
    public CanvasManager canvasManager;
    public PlayerManager playerManager;
    public PortalObject hubPortal;
    public PortalObject islandPortal;
    //
    private Collider[] colliderCache;
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
        gameState = 0;
    }
    //
    void Update()
    {
        if (gameState == 0) { return; }
        inputManager.ProcessInput(playerManager, cameraManager.mainCameraTrans);
        OnHubPortalTick();
        OnIslandPortalTick();
    }
    //
    void LateUpdate()
    {
        if (gameState == 0) { return; }
        cameraManager.ProcessInput(inputManager.InputCache);
    }
    //
    public void OnGameStart()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraManager.menuCamera.gameObject.SetActive(false);
        cameraManager.orbitalFollow.gameObject.SetActive(true);
        gameState = 1;
        hubPortal.ActivatePortal();
    }
    public void OnHubPortalTick()
    {
        if (hubPortal.portalTrigger.enabled)
        {
            if (Physics.OverlapSphereNonAlloc(playerManager.transform.position, 0.5f, colliderCache, hubPortal.portalLayer) > 0)
            {
                gameState = 2;
                hubPortal.DisablePortal();
                float timer = 0f;
                bool jumpToken = false;
                audioManager.PlayAudio(audioManager.portalEnter, 0.64f);
                playerManager.characterRigid.isKinematic = true;
                DOTween.To(() => timer, x => timer = x, 2f, 2f).OnUpdate(() =>
                    {
                        if (timer >= 1f)
                        {
                            if (!jumpToken)
                            {
                                jumpToken = true;
                                audioManager.PlayAudio(audioManager.portalExit, 0.64f);
                                playerManager.transform.position = new Vector3(0f, 8f, 120f);
                                gameState = 1;
                            }
                            //
                            cameraManager.playerCamera.Lens.FieldOfView = 60f + (2f - timer) * 40f;
                            playerManager.characterLegs.LegsAnimatorBlend = (timer - 1f);
                            playerManager.characterRigid.isKinematic = false;
                        }
                        else
                        {
                            playerManager.characterLegs.LegsAnimatorBlend = (1f - timer);
                            cameraManager.playerCamera.Lens.FieldOfView = 60f + timer * 40f;
                        }
                    }).OnComplete(() =>
                    {
                        cameraManager.playerCamera.Lens.FieldOfView = 60f;
                        playerManager.characterLegs.LegsAnimatorBlend = 1f;
                        islandPortal.ActivatePortal();
                    });
            }
        }
    }
    public void OnIslandPortalTick()
    {
        if (islandPortal.portalTrigger.enabled)
        {
            if (Physics.OverlapSphereNonAlloc(playerManager.transform.position, 0.5f, colliderCache, islandPortal.portalLayer) > 0)
            {
                gameState = 2;
                islandPortal.DisablePortal();
                float timer = 0f;
                bool jumpToken = false;
                audioManager.PlayAudio(audioManager.portalEnter, 0.64f);
                playerManager.characterRigid.isKinematic = true;
                DOTween.To(() => timer, x => timer = x, 2f, 2f).OnUpdate(() =>
                    {
                        if (timer >= 1f)
                        {
                            if (!jumpToken)
                            {
                                jumpToken = true;
                                audioManager.PlayAudio(audioManager.portalExit, 0.64f);
                                playerManager.transform.position = new Vector3(0f, 5f, -6f);
                                gameState = 1;
                            }
                            //
                            cameraManager.playerCamera.Lens.FieldOfView = 60f + (2f - timer) * 40f;
                            playerManager.characterLegs.LegsAnimatorBlend = (timer - 1f);
                            playerManager.characterRigid.isKinematic = false;

                        }
                        else
                        {
                            playerManager.characterLegs.LegsAnimatorBlend = (1f - timer);
                            cameraManager.playerCamera.Lens.FieldOfView = 60f + timer * 40f;
                        }
                    }).OnComplete(() =>
                    {
                        cameraManager.playerCamera.Lens.FieldOfView = 60f;
                        playerManager.characterLegs.LegsAnimatorBlend = 1f;
                        hubPortal.ActivatePortal();
                    });
            }
        }
    }
}
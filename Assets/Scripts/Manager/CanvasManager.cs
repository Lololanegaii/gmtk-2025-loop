using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//
public class CanvasManager : MonoBehaviour
{
    public CanvasGroup menuCanvasGroup;
    public TextMeshProUGUI pressToPlayText;
    public Button pressToPlayButton;
    //
    private Tween p2pTween;
    //
    public void Setup()
    {
        p2pTween = pressToPlayText.DOFade(0.1f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        pressToPlayButton.onClick.AddListener(OnGameStart);
    }
    public void OnGameStart()
    {
        p2pTween.Kill();
        GameManager.Instance.audioManager.PlayAudio(GameManager.Instance.audioManager.pressPlayClick, 0.64f);
        GameManager.Instance.cameraManager.menuCamera.DOPunchPosition(Vector3.right, 0.64f).SetEase(Ease.InOutSine);
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.DOFade(0f, 0.64f).SetEase(Ease.InOutSine).OnComplete(() =>
        {
            GameManager.Instance.OnGameStart();
        });
    }
}

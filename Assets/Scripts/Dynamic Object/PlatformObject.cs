using DG.Tweening;
using UnityEngine;

public class PlatformObject : MonoBehaviour
{
    [Header("Move")]
    public Vector3[] movePoints;
    public float moveSpeed;
    [Header("Rotate")]
    public Vector3 rotateFactor;
    public bool rotateYoyo;
    public float rotateSpeed;
    [Header("Disappear")]
    public bool disappearYoyo;
    [Range(1f, 10f)] public float disappearSecond;
    public Renderer disappearRenderer;
    public Collider disappearCollider;
    [Header("Death")]
    public bool deathOnTouch;
    public float deathRepelForce;
    public Vector3 deathColliderSize;
    public LayerMask deathColliderLayer;
    //
    private Vector3[] movePointCache;
    private int movePointIndex;
    private float disappear;
    //
    void Start()
    {
        movePointCache = new Vector3[movePoints.Length + 1];
        movePointCache[0] = transform.position;
        for (int i = 0; i < movePoints.Length; i++)
        {
            movePointCache[i + 1] = transform.position + movePoints[i];
        }
        movePointIndex = 1;
    }
    void Update()
    {
        if (moveSpeed > 0f)
        {
            if ((movePointCache[movePointIndex] - transform.position).sqrMagnitude > 0.1f)
            {
                transform.position += moveSpeed * Time.deltaTime * (movePointCache[movePointIndex] - transform.position).normalized;
            }
            else
            {
                movePointIndex++;
                movePointIndex %= movePointCache.Length;
            }
        }
        //
        if (rotateSpeed > 0f)
        {
            transform.Rotate(rotateSpeed * Time.deltaTime * rotateFactor);
        }
        //
        if (disappearYoyo)
        {
            disappear += Time.deltaTime;
            if (disappear >= disappearSecond)
            {
                disappear = 0f;
                if (disappearRenderer.material.color.a > 0.5f)
                {
                    disappearRenderer.material.DOFade(0f, 0.2f).SetEase(Ease.OutSine);
                    disappearCollider.enabled = false;
                }
                else
                {
                    disappearRenderer.material.DOFade(1f, 0.2f).SetEase(Ease.OutSine);
                    disappearCollider.enabled = true;
                }
            }
        }
        //
        if (deathOnTouch)
        {
            if (GameManager.Instance.playerManager.IsAlive)
            {
                if (Physics.OverlapBox(transform.position, deathColliderSize, transform.rotation, deathColliderLayer).Length > 0)
                {
                    GameManager.Instance.playerManager.OnDeath(transform.position, deathRepelForce);
                }
            }
        }
    }
    void OnDrawGizmosSelected()
    {
        if (movePointCache != null)
        {
            for (int i = 0; i < movePointCache.Length; i++)
            {
                Gizmos.DrawSphere(transform.position + movePointCache[i], 0.4f);
            }
        }
        else
        {
            for (int i = 0; i < movePoints.Length; i++)
            {
                Gizmos.DrawSphere(transform.position + movePoints[i], 0.4f);
            }
        }
    }
}

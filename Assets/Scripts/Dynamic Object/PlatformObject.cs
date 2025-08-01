using UnityEngine;

public class PlatformObject : MonoBehaviour
{
    public Vector3[] movePoints;
    public float moveSpeed;
    public Vector3 rotateFactor;
    public bool rotateYoyo;
    public float rotateSpeed;
    //
    private Vector3[] movePointCache;
    private int movePointIndex;
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

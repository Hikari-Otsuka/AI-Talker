using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform arCamera; // ARカメラのTransform

    void Start()
    {
        if (arCamera == null)
        {
            arCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        // カメラの前方1mにオブジェクトを配置
        transform.position = arCamera.position + arCamera.forward * 1.5f + Vector3.up * -1.0f;
        // Y軸で180度回転させる
        transform.rotation = arCamera.rotation * Quaternion.Euler(0f, 180f, 0f);
    }
}


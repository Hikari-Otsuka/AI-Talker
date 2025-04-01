using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform arCamera; // AR�J������Transform

    void Start()
    {
        if (arCamera == null)
        {
            arCamera = Camera.main.transform;
        }
    }

    void Update()
    {
        // �J�����̑O��1m�ɃI�u�W�F�N�g��z�u
        transform.position = arCamera.position + arCamera.forward * 1.5f + Vector3.up * -1.0f;
        // Y����180�x��]������
        transform.rotation = arCamera.rotation * Quaternion.Euler(0f, 180f, 0f);
    }
}


using UnityEngine;

public class CameraToBackground : MonoBehaviour
{
    public Camera mainCamera;
    public RenderTexture renderTexture;

    void Start()
    {
        // �J�����̃^�[�Q�b�g�e�N�X�`����ݒ�
        mainCamera.targetTexture = renderTexture;
    }
}

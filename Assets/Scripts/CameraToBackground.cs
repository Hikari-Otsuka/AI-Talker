using UnityEngine;

public class CameraToBackground : MonoBehaviour
{
    public Camera mainCamera;
    public RenderTexture renderTexture;

    void Start()
    {
        // カメラのターゲットテクスチャを設定
        mainCamera.targetTexture = renderTexture;
    }
}

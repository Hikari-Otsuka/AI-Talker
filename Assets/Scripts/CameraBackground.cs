using UnityEngine;
using UnityEngine.UI;

public class CameraBackground : MonoBehaviour
{
    public RawImage backgroundImage;  // カメラ映像を表示するRawImage

    private WebCamTexture webCamTexture;

    void Start()
    {
        // WebCamTextureのインスタンスを作成し、カメラ映像を取得
        webCamTexture = new WebCamTexture();

        // RawImageにカメラ映像を設定
        backgroundImage.texture = webCamTexture;

        // カメラを開始
        webCamTexture.Play();
    }

    void Update()
    {
        // もしカメラが動いていない場合、再生を開始
        if (!webCamTexture.isPlaying)
        {
            webCamTexture.Play();
        }
    }

    void OnDisable()
    {
        // シーンが終了する時にカメラを停止する
        if (webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }
}


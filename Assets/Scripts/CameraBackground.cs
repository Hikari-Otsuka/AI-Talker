using UnityEngine;
using UnityEngine.UI;

public class CameraBackground : MonoBehaviour
{
    public RawImage backgroundImage;  // �J�����f����\������RawImage

    private WebCamTexture webCamTexture;

    void Start()
    {
        // WebCamTexture�̃C���X�^���X���쐬���A�J�����f�����擾
        webCamTexture = new WebCamTexture();

        // RawImage�ɃJ�����f����ݒ�
        backgroundImage.texture = webCamTexture;

        // �J�������J�n
        webCamTexture.Play();
    }

    void Update()
    {
        // �����J�����������Ă��Ȃ��ꍇ�A�Đ����J�n
        if (!webCamTexture.isPlaying)
        {
            webCamTexture.Play();
        }
    }

    void OnDisable()
    {
        // �V�[�����I�����鎞�ɃJ�������~����
        if (webCamTexture.isPlaying)
        {
            webCamTexture.Stop();
        }
    }
}


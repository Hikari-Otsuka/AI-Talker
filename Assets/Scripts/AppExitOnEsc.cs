using UnityEngine;

public class AppExitOnEsc : MonoBehaviour
{
    void Update()
    {
        // ESC�L�[�������ꂽ�ꍇ
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC�L�[��������܂����B�A�v�����I�����܂��B");
            Application.Quit(); // �A�v���P�[�V�������I������

            // �G�f�B�^�Ńe�X�g����ꍇ�A�A�v���P�[�V�����I����͋[����
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}

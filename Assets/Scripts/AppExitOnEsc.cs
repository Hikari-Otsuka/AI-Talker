using UnityEngine;

public class AppExitOnEsc : MonoBehaviour
{
    void Update()
    {
        // ESCキーが押された場合
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESCキーが押されました。アプリを終了します。");
            Application.Quit(); // アプリケーションを終了する

            // エディタでテストする場合、アプリケーション終了を模擬する
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}

//OpenAITextTOSpeech
// 必要なライブラリをインポート
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using UnityEngine.Windows;

// AudioSourceコンポーネントが必要であることを指定
[RequireComponent(typeof(AudioSource))]
public class OpenAITextToSpeech : MonoBehaviour
{
    [SerializeField]
    private string apiKey; // OpenAI APIキー
    private string apiUrl; // OpenAI TextToSpeech APIのURL
    private AudioSource _audioSource; // オーディオソースコンポーネント

    [System.Serializable]
    private class SynthesisRequest
    {
        public String input; // 入力テキスト
        public string model = "tts-1"; // モデル
        public string voice = "alloy"; // 音声の種類
        public string response_format = "mp3"; // 出力フォーマット
        public float speed = 1.0f; // 話速
    }

    [System.Serializable]
    private class SynthesisResponse
    {
        public string audioContent; // Base64エンコードされた音声データ
    }

    // 開始時の初期設定
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>(); // AudioSourceコンポーネントを取得
        apiUrl = "https://api.openai.com/v1/audio/speech"; // APIのURLを構築
    }

    // テキストを音声に変換して再生するメソッド
    public void SynthesizeAndPlay(string text)
    {
        StartCoroutine(Synthesize(text)); // 同期処理を開始
    }

    // OpenAI TextToSpeech APIを呼び出して音声データを取得するコルーチン
    private IEnumerator Synthesize(string text)
    {
        // リクエストデータを作成
        SynthesisRequest requestData = new SynthesisRequest
        {
            input =  text,
            model = "tts-1",
            voice = "sage",
            response_format = "mp3",
            speed = 1.0f
        };

        // JSONデータに変換
        string jsonData = JsonUtility.ToJson(requestData);
        Debug.Log("５．音声変換に送信するJSONデータ: " + jsonData);

        // `PostWwwForm` を使い、空のフォームデータを送信（ボディなしリクエスト）
        // UnityWebRequestを作成し、POSTリクエストを送信　
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            // `UnityWebRequest.PostWwwForm` は `application/x-www-form-urlencoded` を設定するため、
            // `application/json` に上書きする必要がある
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

            // 直接リクエストボディを設定（UploadHandlerRaw を使わない方法）
            request.uploadHandler = new UploadHandlerRaw(jsonBytes); // `UploadHandlerRaw` を使わない
            request.downloadHandler = new DownloadHandlerBuffer();

            //request.uploadHandler = new UploadHandlerRaw(jsonBytes); // ここを削除すれば UploadHandlerRaw 不使用も可能

            yield return request.SendWebRequest();

            //Debug.Log("sending text: " + text + jsonData);

            // リクエストが成功した場合
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;

                //Debug.Log("APIレスポンス: " + response); // APIの返答をログに出力

                //SynthesisResponse synthesisResponse = JsonUtility.FromJson<SynthesisResponse>(response);
                //PlayAudioFromBase64(synthesisResponse.audioContent); // Base64エンコードされた音声データを再生
                byte[] audioData = request.downloadHandler.data;
                StartCoroutine(PlayMp3(audioData));
                Debug.Log("８．音声再生成功");
            }
            else // リクエストが失敗した場合
            {
                Debug.LogError("OpenAI Text-to-Speech Error: " + request.error);
                Debug.Log("レスポンス内容: " + request.downloadHandler.text); // 追加: エラー時のレスポンスを出力
            }
        }
    }

    // Base64エンコードされたオーディオデータを再生するメソッド
    private void PlayAudioFromBase64(string base64AudioData)
    {
        byte[] audioBytes = System.Convert.FromBase64String(base64AudioData);
        StartCoroutine(PlayMp3(audioBytes)); // オーディオクリップをロードして再生
        Debug.Log("MP3音声再生開始");
    }

    // オーディオデータをAudioClipに変換して再生するメソッド
    /*private void LoadAudioClipAndPlay(byte[] audioData)
    {
        int sampleRate = 16000; // OpenAI Text-to-Speechのデフォルトサンプルレートは16kHz
        int channels = 1; // モノラル

        // オーディオデータを浮動小数点配列に変換
        int samplesCount = audioData.Length / 2; // 16-bit PCM, so 2 bytes per sample
        float[] audioFloatData = new float[samplesCount];

        // PCMバイトデータをfloat配列に変換
        for (int i = 0; i < samplesCount; i++)
        {
            short sampleInt = BitConverter.ToInt16(audioData, i * 2); // 2バイトをshort intに変換
            audioFloatData[i] = sampleInt / 32768.0f; // short int範囲(-32768 to 32767)をfloat範囲(-1 to 1)に変換
        }

        // AudioClipを作成し、オーディオデータを設定
        AudioClip clip = AudioClip.Create("SynthesizedSpeech", samplesCount, channels, sampleRate, false);
        clip.SetData(audioFloatData, 0);

        // オーディオソースにクリップを設定し、再生
        _audioSource.clip = clip;
        _audioSource.Play();
        Debug.Log("音声成功3");
    }*/
    // オーディオデータをmp3に変換して再生するメソッド
    private IEnumerator PlayMp3(byte[] audioBytes)
    {
        string filePath = Path.Combine(Application.persistentDataPath, "speech.mp3");
        System.IO.File.WriteAllBytes(filePath, audioBytes);

        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                _audioSource.clip = DownloadHandlerAudioClip.GetContent(request);
                _audioSource.Play();
                //Debug.Log("MP3再生成功");
            }
            else
            {
                Debug.LogError("MP3のロードに失敗: " + request.error);
            }
        }
    }
}
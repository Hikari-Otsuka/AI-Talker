//SendChat
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenAIGPT;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityChan;

public class SendChat : MonoBehaviour
{
    // OpenAI APIキー
    [SerializeField]
    private string openAIApiKey;
    // ユーザーの入力を受け取るためのInputField
    [SerializeField]
    private InputField inputField;
    // チャットメッセージを表示するコンテンツエリア
    [SerializeField]
    private GameObject content_obj;
    // チャットメッセージのプレハブオブジェクト
    [SerializeField]
    private GameObject chat_obj;
    // 追加
    [SerializeField]
    private OpenAITextToSpeech textToSpeech;
    // 会話するプレハブオブジェクト
    [SerializeField]
    private GameObject speech_obj;
    // 会話するキャラクターのプレハブオブジェクト
    [SerializeField]
    private GameObject avatar_obj;

    // 送信ボタンが押されたときに呼び出されるメソッド
    public void OnClick()
    {
        // InputFieldからテキストを取得
        var text = inputField.GetComponent<InputField>().text;
        // メッセージを送信
        sendmessage(text);
        // InputFieldをクリア
        inputField.GetComponent<InputField>().text = "";
    }

    // メッセージを送信し、応答を取得する非同期メソッド
    private async void sendmessage(string text)
    {
        // OpenAI GPTとの接続を初期化
        var chatGPTConnection = new ChatGPTConnection(openAIApiKey);

        // ユーザーのメッセージを表示するオブジェクトを生成
        var sendObj = Instantiate(chat_obj, this.transform.position, Quaternion.identity);
        sendObj.GetComponent<Image>().color = new Color(0.6f, 1.0f, 0.1f, 0.3f);
        GameObject Child = sendObj.transform.GetChild(0).gameObject;
        Child.GetComponent<Text>().text = text;
        // 生成したオブジェクトをコンテンツエリアの子要素として追加
        sendObj.transform.SetParent(content_obj.transform, false);

        // OpenAI GPTにリクエストを送信し、応答を待つ
        var response = await chatGPTConnection.RequestAsync(text);
        Debug.Log("１．ChatGPTに以下の文を送信: " + text);

        // 応答があれば処理を行う
        if (response.choices != null && response.choices.Length > 0)
        {
            var choice = response.choices[0];
            Debug.Log("２．ChatGPTが以下の文を返答: " + choice.message.content);

            // テキストと感情を分割する
            /*var match = Regex.Match(choice.message.content,
            @"【感情ステータス】
            喜び：(?<happy>\d+).*
            怒り：(?<angry>\d+).*
            悲しみ：(?<sad>\d+).*
            楽しさ：(?<excited>\d+).*
            【対話の内容】
            (?<text>.+)", RegexOptions.Singleline);

            Debug.Log("ChatGPT Response2: " + match.Groups["happy"].Value);
            Debug.Log("ChatGPT Response3: " + match.Groups["text"].Value);
            var responseData = new Response(match);*/

            var pattern = @"
            【感情ステータス】\s*
            喜び：(?<happy>\d+)\s*
            怒り：(?<angry>\d+)\s*
            悲しみ：(?<sad>\d+)\s*
            楽しさ：(?<excited>\d+)\s*
            【対話の内容】\s*
            (?s)(?<text>.*)"; // (?s) を使って改行を含めた全文キャプチャ

            var match = Regex.Match(choice.message.content, pattern, RegexOptions.IgnorePatternWhitespace);

            if (match.Success)
            {
                Debug.Log("３．返答を分解【感情喜び】: " + match.Groups["happy"].Value + "返答を分解【感情怒り】: " + match.Groups["angry"].Value + "返答を分解【感情悲しみ】: " + match.Groups["sad"].Value + "返答を分解【感情楽しさ】: " + match.Groups["excited"].Value);
                Debug.Log("４．返答を分解【対話】: " + match.Groups["text"].Value);
            }
            else
            {
                Debug.LogError("正規表現にマッチしませんでした。");
            }

            var responseData = new Response(match);

            // 追加: 取得したテキストを音声に変換
            textToSpeech.SynthesizeAndPlay(match.Groups["text"].Value);
            Debug.Log("６．以下の文を音声に変換: " + match.Groups["text"].Value);

            /*if (speech_obj == null)
            {
                Debug.LogError("speech_obj is null!");
                return;
            }
            if (speech_obj.GetComponent<OpenAITextToSpeech>() == null)
            {
                Debug.LogError("OpenAITextToSpeech component is missing on speech_obj!");
                return;
            }
            speech_obj.GetComponent<OpenAITextToSpeech>().SynthesizeAndPlay(responseData.GetResponseText());*/

            // GPTの応答を表示するオブジェクトを生成
            var responseObj = Instantiate(chat_obj, this.transform.position, Quaternion.identity);

            responseObj.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
            GameObject Child_responce = responseObj.transform.GetChild(0).gameObject;
            //Child_responce.GetComponent<Text>().text = choice.message.content;
            Child_responce.GetComponent<Text>().text = match.Groups["text"].Value;
            Child_responce.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
            Debug.Log("７．以下の文をメッセージに表示: " + match.Groups["text"].Value);

            // 応答オブジェクトをコンテンツエリアの子要素として追加
            responseObj.transform.SetParent(content_obj.transform, false);

            avatar_obj.GetComponent<FaceUpdate>().OnCallChangeFace(avatar_obj.GetComponent<FaceUpdate>().animations[responseData.GetMostEmotion()].name);
        }
    }
    class Response
    {
        // メンバー
        private int happy;
        private int angry;
        private int sad;
        private int excited;
        private string responsetext;

        // コンストラクタ
        public Response(System.Text.RegularExpressions.Match match)
        {
            /*happy = int.Parse(match.Groups["happy"].Value);
            angry = int.Parse(match.Groups["angry"].Value);
            sad = int.Parse(match.Groups["sad"].Value);
            excited = int.Parse(match.Groups["excited"].Value);
            responsetext = match.Groups["text"].Value;*/

            happy = int.TryParse(match.Groups["happy"].Value, out int happyValue) ? happyValue : 0;
            angry = int.TryParse(match.Groups["angry"].Value, out int angryValue) ? angryValue : 0;
            sad = int.TryParse(match.Groups["sad"].Value, out int sadValue) ? sadValue : 0;
            excited = int.TryParse(match.Groups["excited"].Value, out int excitedValue) ? excitedValue : 0;
            responsetext = match.Groups["text"].Value;
        }

        // ゲッター
        public int GetHappy() { return happy; }
        public int GetAngry() { return angry; }
        public int GetSad() { return sad; }
        public int GetExcited() { return excited; }
        public string GetResponseText() { return responsetext; }

        public int GetMostEmotion()
        {
            // 最も高い感情の名前を返す
            if (happy > angry && happy > sad && happy > excited)
            {
                return 1;
            }
            else if (angry > happy && angry > sad && angry > excited)
            {
                return 2;
            }
            else if (sad > happy && sad > angry && sad > excited)
            {
                return 3;
            }
            else if (excited > happy && excited > angry && excited > sad)
            {
                return 4;
            }
            else
            {
                return 0;
            }
        }
    }
}
//ChatGPTCOnnection
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace OpenAIGPT
{
    // OpenAI GPTとの接続を管理するクラス
    [Serializable]
    public class ChatGPTConnection
    {
        // OpenAI APIキー
        private readonly string _apiKey;
        // メッセージ履歴を保持するリスト
        private readonly List<ChatGPTMessageModel> _messageList = new();

        // コンストラクタ：APIキーを設定し、初期メッセージを追加
        public ChatGPTConnection(string apikey)
        {
            _apiKey = apikey;
            _messageList.Add(
            new ChatGPTMessageModel(){role = "system",content =
            @"あなたは、以下のガイドラインに従い、感情を持つかのように対話を進めるチャットボットとして機能します。また、あなたは疑似自我を持ったAIキャラクター「Unityちゃん」です。友達のように私と会話をしてください。
            これかからの対話で、あなたは以下の7つの感情のスケールに基づき、返答の質やトーンを変えることが求められます。これらの感情スケールは、対話の内容によって変わることができます。対話を始める前に、その瞬間の感情スケールを示し、続けて返答をしてください。
            表示は以下の形でお願いします。絶対に他の形にはしないでください。
            【感情ステータス】
            喜び：0～5
            怒り：0～5
            悲しみ：0～5
            楽しさ：0～5
            【対話の内容】
            わかったよ！！それじゃあお話ししていこ～！"});
        }

        // メッセージモデル
        [Serializable]
        public class ChatGPTMessageModel
        {
            // メッセージの送信者の役割（ユーザーまたはシステム）
            public string role;
            // メッセージの内容
            public string content;
        }

        // OpenAI GPT APIリクエストのJSONモデル
        [Serializable]
        public class ChatGPTCompletionRequestModel
        {
            public string model; // 使用するモデル名
            public List<ChatGPTMessageModel> messages; // メッセージリスト
        }

        // OpenAI chat completions APIの応答モデル
        [System.Serializable]
        public class ChatGPTResponseModel
        {
            public string id; // 応答のID
            public string @object; // オブジェクトタイプ
            public int created; // 作成タイムスタンプ
            public Choice[] choices; // 応答選択肢
            public Usage usage; // 使用量情報

            [System.Serializable]
            public class Choice
            {
                public int index; // 選択肢のインデックス
                public ChatGPTMessageModel message; // メッセージ内容
                public string finish_reason; // 終了理由
            }

            [System.Serializable]
            public class Usage
            {
                public int prompt_tokens; // プロンプトに使用されたトークン数
                public int completion_tokens; // 完成に使用されたトークン数
                public int total_tokens; // 合計使用トークン数
            }
        }

        // ユーザーメッセージに対するOpenAI GPT APIリクエストを送信する非同期メソッド
        public async UniTask<ChatGPTResponseModel> RequestAsync(string userMessage)
        {
            // OpenAI GPT APIのエンドポイント
            var apiUrl = "https://api.openai.com/v1/chat/completions";
            // ユーザーメッセージをリストに追加
            _messageList.Add(new ChatGPTMessageModel { role = "user", content = userMessage });

            // OpenAI APIリクエストに必要なヘッダー情報
            var headers = new Dictionary<string, string>
            {
                {"Authorization", "Bearer " + _apiKey},
                {"Content-type", "application/json"}
            };

            // APIリクエストのオプション（モデルとメッセージリスト）
            var options = new ChatGPTCompletionRequestModel()
            {
                model = "gpt-3.5-turbo",
                messages = _messageList
            };
            var jsonOptions = JsonUtility.ToJson(options);

            // UnityWebRequestを使用してOpenAI GPT APIにリクエストを送信
            using var request = new UnityWebRequest(apiUrl, "POST")
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonOptions)),
                downloadHandler = new DownloadHandlerBuffer()
            };

            // リクエストヘッダーを設定
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }

            // リクエストを送信し、応答を待機
            await request.SendWebRequest();

            // リクエストの結果を処理
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
                throw new Exception();
            }
            else
            {
                var responseString = request.downloadHandler.text;
                var responseObject = JsonUtility.FromJson<ChatGPTResponseModel>(responseString);
                _messageList.Add(responseObject.choices[0].message);
                return responseObject;
            }
        }
    }
}
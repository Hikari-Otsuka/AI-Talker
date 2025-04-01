//OpenAITextTOSpeech
// �K�v�ȃ��C�u�������C���|�[�g
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using UnityEngine.Windows;

// AudioSource�R���|�[�l���g���K�v�ł��邱�Ƃ��w��
[RequireComponent(typeof(AudioSource))]
public class OpenAITextToSpeech : MonoBehaviour
{
    [SerializeField]
    private string apiKey; // OpenAI API�L�[
    private string apiUrl; // OpenAI TextToSpeech API��URL
    private AudioSource _audioSource; // �I�[�f�B�I�\�[�X�R���|�[�l���g

    [System.Serializable]
    private class SynthesisRequest
    {
        public String input; // ���̓e�L�X�g
        public string model = "tts-1"; // ���f��
        public string voice = "alloy"; // �����̎��
        public string response_format = "mp3"; // �o�̓t�H�[�}�b�g
        public float speed = 1.0f; // �b��
    }

    [System.Serializable]
    private class SynthesisResponse
    {
        public string audioContent; // Base64�G���R�[�h���ꂽ�����f�[�^
    }

    // �J�n���̏����ݒ�
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>(); // AudioSource�R���|�[�l���g���擾
        apiUrl = "https://api.openai.com/v1/audio/speech"; // API��URL���\�z
    }

    // �e�L�X�g�������ɕϊ����čĐ����郁�\�b�h
    public void SynthesizeAndPlay(string text)
    {
        StartCoroutine(Synthesize(text)); // �����������J�n
    }

    // OpenAI TextToSpeech API���Ăяo���ĉ����f�[�^���擾����R���[�`��
    private IEnumerator Synthesize(string text)
    {
        // ���N�G�X�g�f�[�^���쐬
        SynthesisRequest requestData = new SynthesisRequest
        {
            input =  text,
            model = "tts-1",
            voice = "sage",
            response_format = "mp3",
            speed = 1.0f
        };

        // JSON�f�[�^�ɕϊ�
        string jsonData = JsonUtility.ToJson(requestData);
        Debug.Log("�T�D�����ϊ��ɑ��M����JSON�f�[�^: " + jsonData);

        // `PostWwwForm` ���g���A��̃t�H�[���f�[�^�𑗐M�i�{�f�B�Ȃ����N�G�X�g�j
        // UnityWebRequest���쐬���APOST���N�G�X�g�𑗐M�@
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            // `UnityWebRequest.PostWwwForm` �� `application/x-www-form-urlencoded` ��ݒ肷�邽�߁A
            // `application/json` �ɏ㏑������K�v������
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonData);

            // ���ڃ��N�G�X�g�{�f�B��ݒ�iUploadHandlerRaw ���g��Ȃ����@�j
            request.uploadHandler = new UploadHandlerRaw(jsonBytes); // `UploadHandlerRaw` ���g��Ȃ�
            request.downloadHandler = new DownloadHandlerBuffer();

            //request.uploadHandler = new UploadHandlerRaw(jsonBytes); // �������폜����� UploadHandlerRaw �s�g�p���\

            yield return request.SendWebRequest();

            //Debug.Log("sending text: " + text + jsonData);

            // ���N�G�X�g�����������ꍇ
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;

                //Debug.Log("API���X�|���X: " + response); // API�̕ԓ������O�ɏo��

                //SynthesisResponse synthesisResponse = JsonUtility.FromJson<SynthesisResponse>(response);
                //PlayAudioFromBase64(synthesisResponse.audioContent); // Base64�G���R�[�h���ꂽ�����f�[�^���Đ�
                byte[] audioData = request.downloadHandler.data;
                StartCoroutine(PlayMp3(audioData));
                Debug.Log("�W�D�����Đ�����");
            }
            else // ���N�G�X�g�����s�����ꍇ
            {
                Debug.LogError("OpenAI Text-to-Speech Error: " + request.error);
                Debug.Log("���X�|���X���e: " + request.downloadHandler.text); // �ǉ�: �G���[���̃��X�|���X���o��
            }
        }
    }

    // Base64�G���R�[�h���ꂽ�I�[�f�B�I�f�[�^���Đ����郁�\�b�h
    private void PlayAudioFromBase64(string base64AudioData)
    {
        byte[] audioBytes = System.Convert.FromBase64String(base64AudioData);
        StartCoroutine(PlayMp3(audioBytes)); // �I�[�f�B�I�N���b�v�����[�h���čĐ�
        Debug.Log("MP3�����Đ��J�n");
    }

    // �I�[�f�B�I�f�[�^��AudioClip�ɕϊ����čĐ����郁�\�b�h
    /*private void LoadAudioClipAndPlay(byte[] audioData)
    {
        int sampleRate = 16000; // OpenAI Text-to-Speech�̃f�t�H���g�T���v�����[�g��16kHz
        int channels = 1; // ���m����

        // �I�[�f�B�I�f�[�^�𕂓������_�z��ɕϊ�
        int samplesCount = audioData.Length / 2; // 16-bit PCM, so 2 bytes per sample
        float[] audioFloatData = new float[samplesCount];

        // PCM�o�C�g�f�[�^��float�z��ɕϊ�
        for (int i = 0; i < samplesCount; i++)
        {
            short sampleInt = BitConverter.ToInt16(audioData, i * 2); // 2�o�C�g��short int�ɕϊ�
            audioFloatData[i] = sampleInt / 32768.0f; // short int�͈�(-32768 to 32767)��float�͈�(-1 to 1)�ɕϊ�
        }

        // AudioClip���쐬���A�I�[�f�B�I�f�[�^��ݒ�
        AudioClip clip = AudioClip.Create("SynthesizedSpeech", samplesCount, channels, sampleRate, false);
        clip.SetData(audioFloatData, 0);

        // �I�[�f�B�I�\�[�X�ɃN���b�v��ݒ肵�A�Đ�
        _audioSource.clip = clip;
        _audioSource.Play();
        Debug.Log("��������3");
    }*/
    // �I�[�f�B�I�f�[�^��mp3�ɕϊ����čĐ����郁�\�b�h
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
                //Debug.Log("MP3�Đ�����");
            }
            else
            {
                Debug.LogError("MP3�̃��[�h�Ɏ��s: " + request.error);
            }
        }
    }
}
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
    // OpenAI API�L�[
    [SerializeField]
    private string openAIApiKey;
    // ���[�U�[�̓��͂��󂯎�邽�߂�InputField
    [SerializeField]
    private InputField inputField;
    // �`���b�g���b�Z�[�W��\������R���e���c�G���A
    [SerializeField]
    private GameObject content_obj;
    // �`���b�g���b�Z�[�W�̃v���n�u�I�u�W�F�N�g
    [SerializeField]
    private GameObject chat_obj;
    // �ǉ�
    [SerializeField]
    private OpenAITextToSpeech textToSpeech;
    // ��b����v���n�u�I�u�W�F�N�g
    [SerializeField]
    private GameObject speech_obj;
    // ��b����L�����N�^�[�̃v���n�u�I�u�W�F�N�g
    [SerializeField]
    private GameObject avatar_obj;

    // ���M�{�^���������ꂽ�Ƃ��ɌĂяo����郁�\�b�h
    public void OnClick()
    {
        // InputField����e�L�X�g���擾
        var text = inputField.GetComponent<InputField>().text;
        // ���b�Z�[�W�𑗐M
        sendmessage(text);
        // InputField���N���A
        inputField.GetComponent<InputField>().text = "";
    }

    // ���b�Z�[�W�𑗐M���A�������擾����񓯊����\�b�h
    private async void sendmessage(string text)
    {
        // OpenAI GPT�Ƃ̐ڑ���������
        var chatGPTConnection = new ChatGPTConnection(openAIApiKey);

        // ���[�U�[�̃��b�Z�[�W��\������I�u�W�F�N�g�𐶐�
        var sendObj = Instantiate(chat_obj, this.transform.position, Quaternion.identity);
        sendObj.GetComponent<Image>().color = new Color(0.6f, 1.0f, 0.1f, 0.3f);
        GameObject Child = sendObj.transform.GetChild(0).gameObject;
        Child.GetComponent<Text>().text = text;
        // ���������I�u�W�F�N�g���R���e���c�G���A�̎q�v�f�Ƃ��Ēǉ�
        sendObj.transform.SetParent(content_obj.transform, false);

        // OpenAI GPT�Ƀ��N�G�X�g�𑗐M���A������҂�
        var response = await chatGPTConnection.RequestAsync(text);
        Debug.Log("�P�DChatGPT�Ɉȉ��̕��𑗐M: " + text);

        // ����������Ώ������s��
        if (response.choices != null && response.choices.Length > 0)
        {
            var choice = response.choices[0];
            Debug.Log("�Q�DChatGPT���ȉ��̕���ԓ�: " + choice.message.content);

            // �e�L�X�g�Ɗ���𕪊�����
            /*var match = Regex.Match(choice.message.content,
            @"�y����X�e�[�^�X�z
            ��сF(?<happy>\d+).*
            �{��F(?<angry>\d+).*
            �߂��݁F(?<sad>\d+).*
            �y�����F(?<excited>\d+).*
            �y�Θb�̓��e�z
            (?<text>.+)", RegexOptions.Singleline);

            Debug.Log("ChatGPT Response2: " + match.Groups["happy"].Value);
            Debug.Log("ChatGPT Response3: " + match.Groups["text"].Value);
            var responseData = new Response(match);*/

            var pattern = @"
            �y����X�e�[�^�X�z\s*
            ��сF(?<happy>\d+)\s*
            �{��F(?<angry>\d+)\s*
            �߂��݁F(?<sad>\d+)\s*
            �y�����F(?<excited>\d+)\s*
            �y�Θb�̓��e�z\s*
            (?s)(?<text>.*)"; // (?s) ���g���ĉ��s���܂߂��S���L���v�`��

            var match = Regex.Match(choice.message.content, pattern, RegexOptions.IgnorePatternWhitespace);

            if (match.Success)
            {
                Debug.Log("�R�D�ԓ��𕪉��y�����сz: " + match.Groups["happy"].Value + "�ԓ��𕪉��y����{��z: " + match.Groups["angry"].Value + "�ԓ��𕪉��y����߂��݁z: " + match.Groups["sad"].Value + "�ԓ��𕪉��y����y�����z: " + match.Groups["excited"].Value);
                Debug.Log("�S�D�ԓ��𕪉��y�Θb�z: " + match.Groups["text"].Value);
            }
            else
            {
                Debug.LogError("���K�\���Ƀ}�b�`���܂���ł����B");
            }

            var responseData = new Response(match);

            // �ǉ�: �擾�����e�L�X�g�������ɕϊ�
            textToSpeech.SynthesizeAndPlay(match.Groups["text"].Value);
            Debug.Log("�U�D�ȉ��̕��������ɕϊ�: " + match.Groups["text"].Value);

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

            // GPT�̉�����\������I�u�W�F�N�g�𐶐�
            var responseObj = Instantiate(chat_obj, this.transform.position, Quaternion.identity);

            responseObj.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.3f);
            GameObject Child_responce = responseObj.transform.GetChild(0).gameObject;
            //Child_responce.GetComponent<Text>().text = choice.message.content;
            Child_responce.GetComponent<Text>().text = match.Groups["text"].Value;
            Child_responce.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
            Debug.Log("�V�D�ȉ��̕������b�Z�[�W�ɕ\��: " + match.Groups["text"].Value);

            // �����I�u�W�F�N�g���R���e���c�G���A�̎q�v�f�Ƃ��Ēǉ�
            responseObj.transform.SetParent(content_obj.transform, false);

            avatar_obj.GetComponent<FaceUpdate>().OnCallChangeFace(avatar_obj.GetComponent<FaceUpdate>().animations[responseData.GetMostEmotion()].name);
        }
    }
    class Response
    {
        // �����o�[
        private int happy;
        private int angry;
        private int sad;
        private int excited;
        private string responsetext;

        // �R���X�g���N�^
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

        // �Q�b�^�[
        public int GetHappy() { return happy; }
        public int GetAngry() { return angry; }
        public int GetSad() { return sad; }
        public int GetExcited() { return excited; }
        public string GetResponseText() { return responsetext; }

        public int GetMostEmotion()
        {
            // �ł���������̖��O��Ԃ�
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
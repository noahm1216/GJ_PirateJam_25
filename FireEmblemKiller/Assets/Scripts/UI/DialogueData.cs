using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueData : MonoBehaviour
{
    public Color dialogueColor = Color.black; // if we eventually want to do multicolor messages <ColorUtility.ToHtmlStringRGB( myColor )>
    public TextMeshProUGUI dialogueTextBox;
    public bool neverAnimateText, autoPlayMessages;

    public string fullDialogueMessage;
    public bool skippedAnimatedText;
    public bool textIsAnimating;
    private float textTimeStamp, textWaitTime;
    public List<DialogueItem> quededDialogue = new List<DialogueItem>();

    private void Start()
    {
        ResetTextData();
    }    

    public void AddDialogueToQueue(DialogueItem _newDialogue)
    {
        print($"Adding Dialogue: {_newDialogue.words}");
        quededDialogue.Add(new DialogueItem(_newDialogue.dialogueColor, _newDialogue.words, _newDialogue.textTypeSpeedWaitTime));

        if (quededDialogue.Count == 1)
            PlayQuededDialogue(quededDialogue[0]);
    }

    private void PlayQuededDialogue(DialogueItem _nextDialogue)
    {
        print($"I want to DELETE the text {dialogueTextBox.text}\n AND replace it with {_nextDialogue.words}");
        ResetTextData();

        if (string.IsNullOrEmpty(_nextDialogue.words))
            return;       

        if (_nextDialogue.textTypeSpeedWaitTime != 0)
            textWaitTime = _nextDialogue.textTypeSpeedWaitTime;
        else
            textWaitTime = 0.15f;

        dialogueColor = _nextDialogue.dialogueColor;
        fullDialogueMessage = _nextDialogue.words;        
        dialogueTextBox.color = dialogueColor;

        textIsAnimating = true;
        textTimeStamp = Time.time;
        StartCoroutine(TypeText());
    }

    public void FinishTypingOrNextDialogue()
    {
        //print("PRESSED SKIP TEXT");
        skippedAnimatedText = true;
    }

    private void ResetSkipVars()
    {
        textIsAnimating = false;
        skippedAnimatedText = false;
    }

    private void ResetTextData()
    {
        if (dialogueTextBox)
            dialogueTextBox.text = "";
    }

    // Update is called once per frame
    private void Update()
    {
        if (autoPlayMessages || skippedAnimatedText) // if we press skip 
        {
            if (!textIsAnimating)
            {
                //print("ready to go to next message OR close dialogue box");
                if (quededDialogue.Count > 0)
                { ResetTextData(); ResetSkipVars(); quededDialogue.RemoveAt(0); }

                if (quededDialogue.Count > 0)
                { ResetTextData(); ResetSkipVars(); PlayQuededDialogue(quededDialogue[0]); }
                else
                    if (ManagerConversationHandler.Instance)
                { ResetTextData(); ResetSkipVars(); ManagerConversationHandler.Instance.TogglePortraitBox(false); ManagerConversationHandler.Instance.ToggleDialogueBox(false); }
            }           
            
        }    

    }

    IEnumerator TypeText()
    {        
        for (int i = 0; i < fullDialogueMessage.Length; i++)
        {
            if (neverAnimateText || skippedAnimatedText)
            {
                dialogueTextBox.text = fullDialogueMessage;
                ResetSkipVars();
                yield break;
            }
            else
            {
                dialogueTextBox.text += fullDialogueMessage[i];
                textIsAnimating = true;
                yield return new WaitForSeconds(textWaitTime);
            }
        }
        ResetSkipVars();
    }

   
}


// the custom data for conversations involving dialogue
[System.Serializable]
public class DialogueItem
{

    public Color dialogueColor = Color.black;
    public string words;
    public float textTypeSpeedWaitTime;


    public DialogueItem(Color _newClr, string _newWords, float _newTypeSpeed)
    {
        dialogueColor = _newClr;
        words = _newWords;
        textTypeSpeedWaitTime = _newTypeSpeed;
    }

}//end of data for dialogue
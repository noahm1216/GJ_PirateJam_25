using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueData : MonoBehaviour
{
    public Color dialogueColor = Color.black; // if we eventually want to do multicolor messages <ColorUtility.ToHtmlStringRGB( myColor )>
    public TextMeshProUGUI dialogueTextBox;
    public bool neverAnimateText;

    public string fullDialogueMessage;
    public bool skippedAnimatedText;
    public bool textIsAnimating;
    private float textTimeStamp, textWaitTime;
    public List<DialogueItem> quededDialogue = new List<DialogueItem>();

    private void Start()
    {
        dialogueTextBox.text = "...";
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
        if (string.IsNullOrEmpty(_nextDialogue.words))
            return;

        string dialogueConverter = "" + _nextDialogue.words;

        textIsAnimating = true;

        if (_nextDialogue.textTypeSpeedWaitTime != 0)
            textWaitTime = _nextDialogue.textTypeSpeedWaitTime;
        else
            textWaitTime = 0.05f;

        dialogueColor = _nextDialogue.dialogueColor;
        fullDialogueMessage = _nextDialogue.words;
        dialogueTextBox.text = "";
        dialogueTextBox.color = dialogueColor;

        textTimeStamp = Time.time;
        StartCoroutine(TypeText());
    }

    public void FinishTypingOrNextDialogue()
    {
        skippedAnimatedText = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (quededDialogue.Count > 0 && !textIsAnimating && Time.time > textTimeStamp + 2f) // if we have dialogue to play, but arent playing anything right now
        {
            if (dialogueTextBox.text == fullDialogueMessage)// if our text lines up we are done
            {
                print("DIALOGUE AND MESSAGE LINE UP!!!!!!!!");
                if (skippedAnimatedText) // if we pressed the button to skip our text
                {
                    quededDialogue.RemoveAt(0); // remove our current dialogue and reset. Then check if we have more
                    textIsAnimating = false;
                    skippedAnimatedText = false;
                    if (quededDialogue.Count > 0)
                        PlayQuededDialogue(quededDialogue[0]);
                    else // turn off the UI for dialogue and pictures
                        if (ManagerConversationHandler.Instance) { ManagerConversationHandler.Instance.ToggleDialogueBox(false); ManagerConversationHandler.Instance.TogglePortraitBox(false); }
                }
            }
            else // our message has NOT lined up yet
            {
                if (textIsAnimating) // if we ware not playing
                    PlayQuededDialogue(quededDialogue[0]);
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
                skippedAnimatedText = false;
                textIsAnimating = false;
                //yield break;
            }
            else
            {
                dialogueTextBox.text += fullDialogueMessage[i];
                textIsAnimating = true;
                yield return new WaitForSeconds(textWaitTime);
            }
        }
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
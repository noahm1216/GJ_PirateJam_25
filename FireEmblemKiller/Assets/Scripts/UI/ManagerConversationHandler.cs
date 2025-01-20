using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerConversationHandler : MonoBehaviour
{
    public static ManagerConversationHandler Instance { get; private set; }

    public PortraitData portraitTemplate;
    public List<PortraitData> portraitsCreated;

    public DialogueData dialogueManager;


    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        ToggleDialogueBox(false);
        TogglePortraitBox(false);
    }

    // SPEAKERS
    public void AddSpeakerToConversation(Color _nameColor, Sprite _profilePic, bool _isFacingLeft, string _name)
    {
        if (!portraitTemplate)
            return;
        

        if (portraitsCreated.Count > 0 && portraitTemplate.transform.parent.gameObject.activeSelf == false)
        {
            foreach (PortraitData portraits in portraitsCreated)
                Destroy(portraits.gameObject);
            portraitsCreated.Clear();
        }
        
        Transform portraitClone = Instantiate(portraitTemplate.transform, portraitTemplate.transform.parent);
        portraitClone.gameObject.SetActive(true);
        PortraitData pCloneData = null;
        portraitClone.TryGetComponent(out pCloneData);

        //figure out if we need to flip it
        //if (x % 2 == 0)
        //    Console.WriteLine(�Even�);
        //else
        //    Console.WriteLine(�Odd�);


        if (pCloneData)
        {
            pCloneData.UpdateProfile(_nameColor, _profilePic, _isFacingLeft, _name);
            portraitsCreated.Add(pCloneData);
        }

        TogglePortraitBox(true);
    }

    private void RemoveSpeakerFromConversation(Color _nameColor, Sprite _profilePic, bool _isFacingLeft, string _name)
    {
        if (portraitsCreated.Count == 0)
            return;

        foreach (PortraitData portrait in portraitsCreated)
        {
            if (portrait.name == _name && portrait.colorIndentifier == _nameColor)// found a match
            { portraitsCreated.Remove(portrait); break; }
        }

        if (portraitsCreated.Count == 0)
            TogglePortraitBox(false);
    }

    public void TogglePortraitBox(bool _showPortraitBoxes)
    {
        if (portraitTemplate && portraitTemplate.transform.parent)
            portraitTemplate.transform.parent.gameObject.SetActive(_showPortraitBoxes);
    }

    // DIALOGUE
    public void AddDialogueToList(Color _textColor, string _textWords, float _textWaitSpeed)
    {
        ToggleDialogueBox(true);
        dialogueManager.AddDialogueToQueue(new DialogueItem(_textColor, _textWords, _textWaitSpeed));        
    }

    public void ToggleDialogueBox(bool _showDialogueBox)
    {
        if (dialogueManager)
            dialogueManager.gameObject.SetActive(_showDialogueBox);
    }


    // Update is called once per frame
    void Update()
    {
        CheckInputs();
    }

    public void CheckInputs()
    {
        if (Input.GetKeyUp(KeyCode.Space))
            dialogueManager.FinishTypingOrNextDialogue();
        
    }
}

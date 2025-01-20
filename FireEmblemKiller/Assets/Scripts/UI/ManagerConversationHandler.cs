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
    public void AddSpeakerToConversation(UnitCapsule _unitPassed)
    {
        if (!portraitTemplate)
            return;        

        if (portraitsCreated.Count > 0 && portraitTemplate.transform.parent.gameObject.activeSelf == false) // clear all portraits if this was disabled at the time
        {
            foreach (PortraitData portraits in portraitsCreated)
                Destroy(portraits.gameObject);
            portraitsCreated.Clear();
        }

        // TODO
        // for each character portraits
        // if they have dont dialogue left
        // consider removing them
        if (dialogueManager && dialogueManager.quededDialogue.Count > 0) // check if there are more text in queue with this unit
            RemoveSpeakerFromConversation(_unitPassed); // check if we already have this speaker on screen || if we have more dialogue after another character, then we dont want to remove it just yet
           
        

        Transform portraitClone = Instantiate(portraitTemplate.transform, portraitTemplate.transform.parent); // create the new portrait
        portraitClone.gameObject.SetActive(true);
        PortraitData pCloneData = null;
        portraitClone.TryGetComponent(out pCloneData);               

        if (pCloneData)
        {
            pCloneData.UpdateProfile(_unitPassed, false);
            portraitsCreated.Add(pCloneData);
        }

        //if (x % 2 == 0) //figure out if we need to flip it
        //    Console.WriteLine(“Even”);
        //else
        //    Console.WriteLine(“Odd”);

        TogglePortraitBox(true); // show the portraits
    }

    private void RemoveSpeakerFromConversation(UnitCapsule _unitPassed)
    {
        if (portraitsCreated.Count == 0)
            return;

        foreach (PortraitData portrait in portraitsCreated)
        {           
            if (portrait.profileNameText.text == _unitPassed.thisUnitData.unitName && portrait.colorIndentifier == _unitPassed.thisUnitData.unitColor)// found a match
            { portraitsCreated.Remove(portrait); Destroy(portrait.gameObject); break; }
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
    public void AddDialogueToList(UnitCapsule _unitPassed, string _textWords, float _textWaitSpeed)
    {
        ToggleDialogueBox(true);
        dialogueManager.AddDialogueToQueue(new DialogueItem(_unitPassed.thisUnitData.unitName, _unitPassed.thisUnitData.unitColor, _textWords, _textWaitSpeed)); // we could make talk speed part of a unit
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

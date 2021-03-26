using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EZPZBD_ExampleUIManager : MonoBehaviour {

	[Header("Coins")]
	public Text coinText;
	public Text coinTextBackground;

    [Header("NPCs with Actions")]
    public List<EZPZBD_ExampleNPCScript> npcList;

    [Header("Dialogue")]
	public GameObject DialogueCanvas;

	public Text dialogueText;
	public Text dialogueBackgroundText;
	public Text speakerNameText;
	public Text speakerNameBackgroundText;

	public Image backgroundImage;
	public Image speakerTextImage;
	public Image speakerImage;

	public Sprite[] BackgroundImages;
	public Sprite[] SpeakerTextImages;

    [Header("Dialogue Choices")]
    public Text choice1Text;
    public Text choice1BackgroundText;
    public Text choice2Text;
    public Text choice2BackgroundText;

    //These need to be objects so I can disable them when not in use
    public GameObject choice1ImageObj;
    public GameObject choice2ImageObj;

    bool firstTimeConversation = true;

    [Header("EZPZ Branching Dialogue")]
    public EZPZ_Tree_Reader EZPZ_reader;
    public EZPZ_TreeInfoHolder EZPZ_infoHolder;
    bool waitingForPlayerChoice = false;
    bool inConversation = false;

    public void SetCoinText(int coinAmount)
	{
		coinText.text = coinAmount.ToString();
		coinTextBackground.text = coinText.text;
	}

	public void ToggleConversationUI(bool choice)
	{
		DialogueCanvas.SetActive(choice);
	}

	public void ToggleConversationUI(bool choice, int NPCIndex)
	{
        if (choice)
        {
            backgroundImage.sprite = BackgroundImages[NPCIndex];
            speakerTextImage.sprite = SpeakerTextImages[NPCIndex];
            choice1ImageObj.GetComponent<Image>().sprite = SpeakerTextImages[NPCIndex];
            choice2ImageObj.GetComponent<Image>().sprite = SpeakerTextImages[NPCIndex];

            //Set the tree
            EZPZ_reader.SetTree(EZPZ_infoHolder.GetTree(NPCIndex));

            //Make sure we have dialogue set up
            EZPZ_reader.GoToNextNode();
            NextNodeCode();

            SetSpeakerName(EZPZ_reader.GetSpeakerName());            
            SetSpeakerSprite(EZPZ_reader.GetSpeakerSprite());

            inConversation = true;
        }

		DialogueCanvas.SetActive(choice);
	}

	public void SetDialogueText(string _text)
	{
		dialogueText.text = _text;
		dialogueBackgroundText.text = _text;
	}

    public void SetChoice1Text(string _text)
    {
        choice1Text.text = _text;
        choice1BackgroundText.text = _text;
        choice1ImageObj.SetActive(true);
    }

    public void SetChoice2Text(string _text)
    {
        choice2Text.text = _text;
        choice2BackgroundText.text = _text;
        choice2ImageObj.SetActive(true);
    }

    public void SetSpeakerName(string _name)
	{
		speakerNameText.text = _name;
		speakerNameBackgroundText.text = _name;
	}

	public void SetSpeakerSprite(Sprite _icon)
	{
		speakerImage.sprite = _icon;
	}

    public bool IsInConversation()
    {
        return inConversation;
    }

   //This begins the brains of the tool. If you want to, copy and paste this and change out the variables you need
   public void ConversationUpdate()
   {
       //I'm using waitingForPlayerChoice to test if the player is at a branch
       if (!waitingForPlayerChoice)
       {
            //Go to the next stoppable node
            //Stoppable nodes are Dialogue, Branch, and End nodes
            EZPZ_reader.GoToNextNode();
   
            //This controls the logic of what happens based off the node we're on
            NextNodeCode();

            //This is not needed for basic EZPZ Branching Dialogue functionality
            //This enables the NPC actions to be checked every node
            foreach (EZPZBD_ExampleNPCScript npc in npcList)
                npc.ProcessVariableChanges();
       }
   }
   
   //This function controls how UI will react to the different nodes
   void NextNodeCode()
   {
       //This useful function gets name of the current node
       switch (EZPZ_reader.GetCurrentNodeName())
       {
           case "DIALOGUE":
                SetDialogueText(EZPZ_reader.GetNodeDialogue());
                break;
   
           case "BRANCH":
                //GetBranchDialogues gets all the text for the question and answers
                //I *know* there are only two choices for this example, but the function returns all the choices in order from top to bottom
                string[] tempString = EZPZ_reader.GetBranchDialogues();
                
                //Set up appropriate UI
                SetDialogueText(tempString[0]);
                SetChoice1Text(tempString[1]);
                SetChoice2Text(tempString[2]);
                
                //Now that we know we're on a Branch node, wait for the player's choice
                waitingForPlayerChoice = true;
                break;
   
           case "END":
           case "NULL":
                //We've reached the end of a tree
                //Get rid of UI we aren't using and reset all control varaibles
                ToggleConversationUI(false);
                inConversation = false;

                //Get rid of the instructions once the player talks for the first time
                if (firstTimeConversation)
                {
                    firstTimeConversation = false;
                    GameObject.Find("_Instructions").SetActive(false);
                }
                break;
       }
   
       //This is to account for New Speaker Nodes. If you put in new info, you want to make sure you get the change
       if (EZPZ_reader.DidSpeakerChange())
       {
            SetSpeakerName(EZPZ_reader.GetSpeakerName());
            SetSpeakerSprite(EZPZ_reader.GetSpeakerSprite());
       }
   }
   
   //This function takes in the player's choice from a Branch node
   public void PlayerChoice(int choiceIndex)
   {   
        //Pass in the index of the choice (first choice is 0), and the function will take care of the rest!
        EZPZ_reader.GoToNextNode(choiceIndex);
   
        waitingForPlayerChoice = false;

        //Remove the choice bubbles
        choice1ImageObj.SetActive(false);
        choice2ImageObj.SetActive(false);

        //Go to the next node immediatly instead of waiting a frame to update
        NextNodeCode();
   }
}

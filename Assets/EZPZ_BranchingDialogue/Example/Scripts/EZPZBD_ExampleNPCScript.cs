using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EZPZBD_ExampleNPCScript : MonoBehaviour {

	//Get the tree for this specific NPC. We'll use it to check variables
	public DialogueTreeScriptableObj NPCTree;

	//These allow us to create a good template for the actions we want the NPCs to perform.
	//Once linked with the dialogue tree variables we can do a lot with them

	[SerializeField]
	public enum VarType
	{
		BOOL,
		INT,
		FLOAT,
		STRING
	};

	//These actions have designated reactions in the ProcessVariableChanges function below
	[SerializeField]
	public enum CompletedAction
	{
		ENABLE,
		DISABLE
	};

	[System.Serializable]
	public struct ActionDictionary
	{
		public GameObject actionObject;
		public string variableName;
		public bool isLocalVaraible;
		public VarType varTypeToCheck;
		[Tooltip("For a bool check use t and f. This version only checks to see if it's equal")]
		public string triggerValue;
		public CompletedAction actionToTake;

		//If the logic is done, don't do it again
		bool completed;
		public bool GetCompleted() { return completed; }
		public void SetCompleted(bool b) { completed = b; }
	}

	public List<ActionDictionary> logicList;

	//This function searches the given dialogue tree local variables and the global variables to check if any actions are taken based on the logic list
	public void ProcessVariableChanges()
	{
		for (int i = 0; i < logicList.Count; i++)
		{
			//If it hasn't been done before
			if (!logicList[i].GetCompleted())
			{				
				//Find variable
				if (logicList[i].isLocalVaraible)
				{
					foreach(DialogueTreeScriptableObj.VariableData vd in NPCTree.variableList)
					{
						if (vd.name == logicList[i].variableName)
						{
							//Check against the trigger value
							if (CheckAgainstTriggerValueLocal(logicList[i], vd))
							{
								//It's true, so do the action
								ProcessAction(logicList[i].actionObject, logicList[i].actionToTake);
								logicList[i].SetCompleted(true);
							}

							//Leave the loop
							break;
						}
					}
				}
				else
				{
					foreach (EZPZ_TreeInfoHolder.VariableData vd in EZPZ_TreeInfoHolder.Instance.globalVariableList)
					{
						if (vd.name == logicList[i].variableName)
						{
							//Check against the trigger value
							if (CheckAgainstTriggerValueGlobal(logicList[i], vd))
							{
								//It's true, so do the action
								ProcessAction(logicList[i].actionObject, logicList[i].actionToTake);
								logicList[i].SetCompleted(true);
							}

							//Leave the loop
							break;
						}
					}
				}
			}
		}
	}

	bool CheckAgainstTriggerValueLocal(ActionDictionary ad, DialogueTreeScriptableObj.VariableData vd)
	{
		switch (ad.varTypeToCheck)
		{
			case VarType.BOOL:
				//If it's t then it's true, anything else and it's false
				bool adBool = (ad.triggerValue == "t") ? true : false;
				return (adBool == vd.boolVal);

			case VarType.INT:
				return (ad.triggerValue == vd.intVal.ToString());

			case VarType.FLOAT:
				return (ad.triggerValue == vd.floatVal.ToString());

			case VarType.STRING:
				return (ad.triggerValue == vd.stringVal);

			default:
				return false;
		}
	}

	bool CheckAgainstTriggerValueGlobal(ActionDictionary ad, EZPZ_TreeInfoHolder.VariableData vd)
	{
		switch (ad.varTypeToCheck)
		{
			case VarType.BOOL:
				//If it's t then it's true, anything else and it's false
				bool adBool = (ad.triggerValue == "t") ? true : false;
				return (adBool == vd.boolVal);

			case VarType.INT:
				return (ad.triggerValue == vd.intVal.ToString());

			case VarType.FLOAT:
				return (ad.triggerValue == vd.floatVal.ToString());

			case VarType.STRING:
				return (ad.triggerValue == vd.stringVal);

			default:
				return false;
		}
	}

	void ProcessAction(GameObject obj, CompletedAction choice)
	{
		switch (choice)
		{
			case CompletedAction.ENABLE:
				obj.SetActive(true);
				break;

			case CompletedAction.DISABLE:
				obj.SetActive(false);
				break;
		}
	}

	//Other NPC logic can go below, but for this example the NPCs never move or do anything besides talk to the player
}

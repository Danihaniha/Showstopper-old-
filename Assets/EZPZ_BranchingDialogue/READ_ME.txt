EZPZ Branching Dialogue Tool by EZPZ Tools
Copyright 2020, Zach Phillips, All rights reserved.
Questions, comments, and bugs: ezpztools@gmail.com

Hello! Thanks for downloading this tool. I hope that you can use this to easily add branching dialogue to your project! To see what this tool can do, open and play the EZPZBD_Example in the Example folder and look through the Dialogue Tree Scriptable Objects in the Dialogue Trees folder. These can be viewed and edited via the Window->EZPZ Branching Dialogue Node Editor window by dragging the trees into it or into the bar at the top of the window

Table of Contents:
- How to Use
- Branching Dialogue Scriptable Objects
- Reader Script
- Variables
  |- Local Variables
  |- Global Variables
  |- Inline Text Replacement
- Nodes
  |- Info Node
  |- Start Node
  |- End Node
  |- Dialogue Node
  |- Branch Node
  |- Local Variable Node
  |- Global Variable Node
  |- Edit Variable Node
  |- Logic Node
  |- New Speaker Node
  |- Start Change Node
  |- Teleport Flow Node
  |- Comment Node
- Reader Script Function List
- EZPZ_InfoHolder Function List

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
HOW TO USE:

I highly recommend playing the EZPZBD_Example scene in the Example folder. The dialogue trees that are used in this scene are available in the Example->Dialogue Trees folder. These can be viewed and edited via the Window->EZPZ Branching Dialogue Node Editor window by dragging the trees into it or into the bar at the top of the window. Also, take a look at the scripts in the Example->Scripts folder. These three scripts will give you an understanding of how the tool works programmatically, as well as a place to copy code. 

1) Create a Branching Dialogue Scriptable Object (Right click in Project Window, Create->Dialogue Tree)
2) Open the EZPZ Branching Dialogue Node Editor window under the Window menu
3) Drag in your Dialogue Object to the Node Editor window
4) Use left click and drag on an empty space to move around the window
5) Creating Nodes with right click
6) Select Nodes with left click, and left click and drag on a Node to move it around
7) Link nodes by left clicking on an output (right) and left clicking again on an input (left)
8) Delete connections by clicking the square on the connection line
9) Delete Nodes by right clicking on a selected Node and selecting "Remove Node"
9a) Note that some Nodes like the Branch Node have special options when you right click on them
10) Drag in the EZPZ_InfoHolder prefab into the scene. Remember to apply all changes you make to it!
11) Assign the EZPZ_Tree_Reader script to a GameObject and add the EZPZ_InfoHolder. You can optionally add your Dialogue Tree Scriptable Object in there as well for easy access.
12) In your personal UI/Dialogue controller script, implement your own versions of the ConversationUpdate(), NextNodeCode(), and PlayerChoice() functions from the EZPZBD_ExampleUIManager script
13) Play and enjoy!

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
BRANCHING DIALOGUE SCRIPTABLE OBJECTS:

Each Dialogue Object has a list of nodes and a list of variables. By default, the node list is hidden. If you want to see the data, you can remove the [HideInInspector] line above the nodeList declaration in the DialogueTreeScriptableObject script (line 59). This is not recommended since the info that this will expose might not make sense, and editing it could mess up the entire tree. However, if you know what you're looking at, and the source code makes sense, then go for it!

The list of variables is a very powerful tool that you should be aware of. To learn more about what variables can do, go to the Variables section. To add a variable, add one to the size of the variable list. This will create a variable that is a copy of the previous variable. Note that variables with duplicate names will not be accessible, so make sure your variable names are unique!

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
READER SCRIPT:

This is how you'll be reading the branching dialogue that you've created. Attach this script to a Game Object (any will do, but I recommend it be on the same object as your UI manager). The TreeInfoHolder object is the EZPZ_TreeInfoHolder prefab, and the TreeToRead is the Dialogue Object that will be currently read by the script. 

To start reading the tree, make sure to call the SetTree() function from the reader script to allow it to start reading information. In your own UI/Conversation script, impliment your own versions of the ConversationUpdate(), NextNodeCode(), and PlayerChoice() functions from the EZPZBD_ExampleUIManager script.

In general, the main functions that you'll be using are SetTree(), GetCurrentNodeName(), GoToNextNode(), GetNodeDialogue(), and GetBranchDialogues(). Make sure to look through the EZPZBD_ExampleUIManager script, read the comments, and watch the videos that help explain how to get this up and running. If you still have questions, please reach out to me at ezpztools@gmail.com.

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
VARIABLES:

  |- LOCAL:
	Local variables are variables that are stored locally to the Dialogue Object. You can see them in the inspector when you select a Dialogue Tree Scriptable Object in your Project window. Each variable has four values, Boolean, Integer, Float, and String. All of these values can be used in one variable, so make the most of the variables you create!

  |- GLOBAL:
	Global variables are variables stored in the EZPZ_TreeInfoHolder script on the EZPZ_InfoHolder prefab. These variables are accessible in any dialogue tree at any time, so it's useful for stuff like player character name, gold amount, and other key items. These can also be edited through trees and other functions. See the EZPZ_TreeInfoHolder script for a list of the functions available to you. 

  |- INLINE TEXT REPLACEMENT:
	In Dialogue Nodes, you can use curly braces to replace text with variable values. For local variables, use the format {var:VariableName:ValueType}. An example of this would be {var:Oranges:int} which replaces this line with the int value of the local variable Oranges. 

	For global variables, just replace var with varGlobal. An example would be {varGlobal:Character:string} which would return the string value of the global variable Character.

	There is one more quick replacement, {var:SpeakerName}. This returns the current speaker name of the dialogue tree, which is set in the Info, New Speaker, and Dialogue (Advanced Options) Nodes.

	Note that the curly braces are not escape characters, so you can use curly braces where ever you normally would without any concern.

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
NODES:

	If you want to remember what a node does while in the tool, hover over the ? in the top right corner of the node.

  |- INFO NODE
	The Info Node can’t be created or destroyed. It doesn’t have an in or an out connection. It holds basic info for the dialogue tree such as the name of the speaker and speaker sprite. None of these are needed for the tree to work, but they are available for easy access and planning.

  |- START NODE
	This node controls the start of the dialogue logic. It only has an out connection. A tree can have multiple start nodes, which are split by a Start ID number. The user can choose which start node to start with when calling the tree in the reader script. If not specified, the tree will start with the lowest numbered node. 
	
  |- END NODE
	This node is the marker for the end of a dialogue tree. There can be multiple of these nodes. It only has an in connection.

  |- DIALOGUE NODE
	This node contains a text field that the user can type in. It has one in connection and one out connection. It also contains an Advanced Options toggle which allows you to assign the speaker and sprite of that specific dialogue node. 

  |- BRANCH NODE
	The Branch Node is used to make branching choices in a tree. It has (at minimum) three in connections and two out connections. The first in connection will be the dialogue for the question, while all other in connections will be for the names of the options. The outputs are directly correlated to the input directly across from it.

  |- LOCAL VARIABLE NODE
	The Local Variable Node represents a variable, local to the tree, that the user has created. It only has an out connection.

  |- GLOBAL VARIABLE NODE
	The Global Variable Node is the same as a Local Variable Node, except it only gets global variables from the EZPZ_InfoHolder prefab. Note, you MUST make sure the EZPZ_InfoHolder prefab is updated ("Apply") to get the most recent global variables. It only has an out connection.

  |- EDIT VARIABLE NODE
	The Edit Variable Node allows the user to edit variable values in a Dialogue Tree at runtime. It takes in a flow connection at the top, and then two more in connections labeled A and B. The variable linked into A will be the one edited, and B will be the modifier. Select the first drop down to choose which value will be edited, and then the second drop down to choose how that value will be edited. Note that B does not need a Variable Node to be connected, as it can use the literal value built into the node. It has one out connection to continue flow.

  |- LOGIC NODE
	This node takes in Variable Nodes and makes a True/False decision based off of the equation selected. Like the Edit Variable Node, it takes in a flow connection at the top, and then two more in connections labeled A and B. Both A and B connections do not need variable nodes attached, so feel free to use the built in literal values. Select the first drop down to choose which value will be edited, and then the second drop down to choose the equation that will be tested. Depending on the outcome, the flow will continue on either the True or False out connections. 
	
  |- NEW SPEAKER NODE
	The New Speaker Node changes the Info Node information. The is useful for lines from another character without having to create and switch to a new Dialogue Tree. The New Speaker information stays throughout the rest of the flow, so if you want the original speaker information to return, you'll need to place another New Speaker Node to set it back. For quick one liners, defer to the Advanced Options on the Dialogue Node. This node has one in and one out connection. 

  |- START CHANGE NODE
	The Start Change Node changes the default start index. It's useful for skipping one time introductions or variable based interactions. It has one in and out connection.

  |- TELEPORT FLOW NODE
	This node links two nodes without using a flow line. This is useful for linking nodes that may be far apart, decreasing visual clutter. The Teleport Flow Node is split into In and Out variations. The In variation has one in connection and no out connection, and the Out variation is vice versa. Each node has a Teleport ID. In nodes go to the Out node with the matching ID.

  |- COMMENT NODE
	This node has no in or out connections, and just contains a small text field for the user to write themselves some notes. 

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
READER SCRIPT FUNCTION LIST:

public void SetTree(DialogueTreeScriptableObj tree)
  |- Set the tree to read

public void SetTree(DialogueTreeScriptableObj tree, int startIndex)
  |- Set the tree to read. Include start node to skip calling StartTree

public void SetNodeIndex(int index)
  |- Sets the current node index to a specific number. Not suggested but hey, you do you

public void StartTree(int startIndex)
  |- Set the tree to a specified beginning

public List<string> GetVariableNamesAsList()
  |- Returns the name of all the variables in the current tree as a List<string>

public string[] GetVariableNamesAsArray()
  |- Returns the name of all the variables in the current tree as a string array

public void AddVariable(string varName = "New Variable", bool bValue = false, int iValue = 0, float fValue = 0.0f, string sValue = "")
  |- Add a new variable to the variable list. *Persists after runtime*

public bool RemoveVariable(string varName)
  |- Removes a variable by name. Returns false variable not found, ignoring case

public bool SetVariableBoolValue(string variableName, bool value)
  |- Sets the bool value of the variable. Returns false if variable not found, ignoring case

public bool SetVariableIntValue(string variableName, int value)
  |- Sets the int value of the variable. Returns false if variable not found, ignoring case

public bool SetVariableFloatValue(string variableName, float value)
  |- Sets the float value of the variable. Returns false if variable not found, ignoring case

public bool SetVariableStringValue(string variableName, string value)
  |- Sets the string value of the variable. Returns false if variable not found, ignoring case

public bool GetVariableBoolValue(string variableName)
  |- Returns the bool value of the variable. Returns false if variable is not found

public int GetVariableIntValue(string variableName)
  |- Returns the bool value of the variable. Returns -1 if variable is not found

public float GetVariableFloatValue(string variableName)
  |- Returns the bool value of the variable. Returns -1f if variable is not found

public string GetVariableStringValue(string variableName)
  |- Returns the string value of the variable. Returns NULL if variable is not found

public string GetCurrentNodeName()
  |- Returns the string name of the current node. Returns NULL if no node exists

public int GetCurrentNodeEnumNum()
  |- Returns the enum index of the current node.

public string GetNextNodeName()
  |- Returns the string name of the node that is going to be called next. Returns NULL if no next node exists

public bool DidSpeakerChange()
  |- Returns true if the speaker changed

public bool ChangeSpeakerInfo(string newSpeakerName, Sprite newSpeakerSprite = null)
  |- Changes the speaker information for the tree. Returns true if successful

public string GetSpeakerName()
  |- Gets the current speaker's name. Returns NULL if Info node does not exist

public Sprite GetSpeakerSprite()
  |- Gets the current speaker's sprite. Returns NULL if Info node does not exist

public bool GoToNextNode()
  |- Tries to go to the next stopping node with no extra commands. Returns true if succeeded

public bool GoToNextNode(int choice)
  |- Tries to go to the next stopping node with an int attached for branching choices. Returns true if succeeded

public string GetNodeDialogue()
  |- Returns the dialogue on this node if it's a Dialogue Node. Returns NULL if not on a Dialogue Node

public string[] GetBranchDialogues()
  |- Returns the dialogues for the branch node. Sorted in connection order, top down. Top connection is the main question. Returns NULL if not on a Branch node

~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
EZPZ_INFOHOLDER FUNCTION LIST:

public DialogueTreeScriptableObj GetTree(string name)
  |- Returns a DialogueTreeScriptableObject located in the dialogueTreeList on the EZPZ_InfoHolder game object. Returns null if the tree does not exist

public DialogueTreeScriptableObj GetTree(int index)
  |- Returns a DialogueTreeScriptableObject located in the dialogueTreeList on the EZPZ_InfoHolder game object. Returns null if the tree does not exist

public void AddGlobalVariable(string varName = "New Variable", bool bValue = false, int iValue = 0, float fValue = 0.0f, string sValue = "")
  |- Add a new global variable to the global variable list. *Persists after runtime*

public bool RemoveGlobalVariable(string varName)
  |- Removes a global variable by name. Returns false variable not found, ignoring case

public bool SetGlobalVariableBoolValue(string variableName, bool value)
  |- Sets the bool value of the global variable. Returns false if variable not found, ignoring case

public bool SetGlobalVariableIntValue(string variableName, int value)
  |- Sets the int value of the global variable. Returns false if variable not found, ignoring case

public bool SetGlobalVariableFloatValue(string variableName, float value)
  |- Sets the float value of the global variable. Returns false if variable not found, ignoring case

public bool SetGlobalVariableStringValue(string variableName, string value)
  |- Sets the string value of the global variable. Returns false if variable not found, ignoring case

public bool GetGlobalVariableBoolValue(string variableName)
  |- Returns the bool value of the global variable. Returns false if variable is not found

public int GetGlobalVariableIntValue(string variableName)
  |- Returns the bool value of the global variable. Returns -1 if variable is not found

public float GetGlobalVariableFloatValue(string variableName)
  |- Returns the bool value of the global variable. Returns -1f if variable is not found

public string GetGlobalVariableStringValue(string variableName)
  |- Returns the string value of the global variable. Returns NULL if variable is not found

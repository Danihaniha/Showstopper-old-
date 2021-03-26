//Copyright 2020, Zach Phillips, All rights reserved.
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
[CreateAssetMenu(menuName = "EZPZ Dialogue Tree")]
public class DialogueTreeScriptableObj : ScriptableObject {

    public string treeName;

    public int startIndex = 0;

    [System.Serializable]
    public struct InOutConnections
    {
        public int nodeIndex;
        public int connectionIndex;
    }

    [System.Serializable]
    public struct NodeData
    {
        public int nodeIndex;
        public int enumType;
        public int inPoints;
        public int outPoints;
        public Vector2 position;
        public List<InOutConnections> inNodeIndexes;
        public List<InOutConnections> outNodeIndexes;

        //Specific node jazz depending on enum type
        //ADD NEW NODES HERE
        public DialogueNode dN;
        public StartNode sN;
        public InfoNode iN;
        public BranchNode bN;
        public VariableNode vN;
        public LogicNode lN;
        public EditVariableNode eN;
        public CommentNode cN;
        public NewSpeakerNode nN;
        public StartChangeNode scN;
        public TeleportFlowNode tfN;
    };

    [System.Serializable]
    public struct DefaultValues
    {
        public bool defaultBValue;
        public int defaultIValue;
        public float defaultFValue;
        public string defaultSValue;
    }

    [SerializeField]
    [HideInInspector] //Comment this line out if you're interested in debugging
    public List<NodeData> nodeList = new List<NodeData>();

    [System.Serializable]
    public class VariableData
    {
        public string name = "VariableName";

        public bool boolVal = false;
        public int intVal = 0;
        public float floatVal = 0.0f;
        public string stringVal = "";
    }

    [SerializeField]
    public List<VariableData> variableList = new List<VariableData>();

    public void Awake()
    {
        //Default to include a info node
        if (nodeList.Count == 0)
        {
            NodeData nd = new NodeData();

            nd.nodeIndex = 0;
            nd.enumType = 0;
            nd.position = Vector2.zero;

            nd.inNodeIndexes = null;
            nd.outNodeIndexes = null;

            //ADD NEW NODES HERE
            nd.sN = new StartNode();
            nd.dN = new DialogueNode();
            nd.iN = new InfoNode();
            nd.bN = new BranchNode();
            nd.vN = new VariableNode();
            nd.lN = new LogicNode();
            nd.eN = new EditVariableNode();
            nd.cN = new CommentNode();
            nd.nN = new NewSpeakerNode();
            nd.scN = new StartChangeNode();

            nodeList.Add(nd);
        }

        if (variableList.Count == 0)
        {
            VariableData vd = new VariableData();

            vd.name = "Temp Variable";

            variableList.Add(vd);
        }
    }

    public void SaveNodeData(List<int> index, List<int> type, List<Vector2> inOutPoints, List<Vector2> pos, List<List<InOutConnections>> inNodes, List<List<InOutConnections>> outNodes, 
        List<InfoNode> iNodesAndID, List<StartNode> sNodesAndID, List<DialogueNode> dNodesAndID, List<BranchNode> bNodesAndID, List<VariableNode> vNodesAndID, List<LogicNode> lNodesAndID,
        List<EditVariableNode> eNodesAndID, List<CommentNode> cNodesAndID, List<NewSpeakerNode> nNodesAndID, List<StartChangeNode> scNodesAndID, List<TeleportFlowNode> tfNodesAndID)
    {
        nodeList.Clear();

        for (int i = 0; i < index.Count; i++)
        {
            NodeData nd = new NodeData();

            nd.nodeIndex = index[i];
            nd.enumType = type[i];
            nd.position = pos[i];
            nd.inPoints = (int)inOutPoints[i].x;
            nd.outPoints = (int)inOutPoints[i].y;

            nd.inNodeIndexes = inNodes[i];
            nd.outNodeIndexes = outNodes[i];

            //ADD NEW NODES HERE
            nd.sN = new StartNode();
            nd.dN = new DialogueNode();
            nd.iN = new InfoNode();
            nd.bN = new BranchNode();
            nd.vN = new VariableNode();
            nd.lN = new LogicNode();
            nd.eN = new EditVariableNode();
            nd.cN = new CommentNode();
            nd.nN = new NewSpeakerNode();
            nd.scN = new StartChangeNode();
            nd.tfN = new TeleportFlowNode();

            //ADD NEW NODES HERE
            switch (nd.enumType)
            {
                case 0: //Start
                    nd.iN = iNodesAndID[i];
                    break;

                case 1: //Start
                    nd.sN = sNodesAndID[i];
                    break;

                case 3: //Dialogue
                    nd.dN = dNodesAndID[i];
                    break;

                case 4: //Branch
                    nd.bN = bNodesAndID[i];
                    break;

                case 5: //Variable
                    nd.vN = vNodesAndID[i];
                    break;

                case 6: //Logic
                    nd.lN = lNodesAndID[i];
                    break;

                case 7: //Edit Varaible
                    nd.eN = eNodesAndID[i];
                    break;

                case 8: //Comment
                    nd.cN = cNodesAndID[i];
                    break;

                case 9: //New Speaker
                    nd.nN = nNodesAndID[i];
                    break;

                case 10: //Start Change
                    nd.scN = scNodesAndID[i];
                    break;

                case 11: //Teleport Flow Change
                    nd.tfN = tfNodesAndID[i];
                    break;
            }

            nodeList.Add(nd);
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }

    public List<NodeData> GetNodesFromTree()
    {
        return nodeList;
    }

    public string GetTreeName()
    {
        return treeName;
    }

#region Variable Getters and Setters

    /// <summary>
    /// Add a new variable to the variable list. *Persists after runtime*
    /// </summary>
    /// <param name="varName"></param>
    /// <param name="bValue"></param>
    /// <param name="iValue"></param>
    /// <param name="fValue"></param>
    /// <param name="sValue"></param>
    public void AddVariable(string varName = "New Variable", bool bValue = false, int iValue = 0, float fValue = 0.0f, string sValue = "")
    {
        VariableData vd = new VariableData();

        vd.name = varName;
        vd.boolVal = bValue;
        vd.intVal = iValue;
        vd.floatVal = fValue;
        vd.stringVal = sValue;

        variableList.Add(vd);
    }

    /// <summary>
    /// Removes a variable by name. Returns false variable not found, ignoring case
    /// </summary>
    /// <param name="varName"></param>
    /// <returns></returns>
    public bool RemoveVariable(string varName)
    {
        for (int i = 0; i < variableList.Count; i++)
        {
            if (variableList[i].name.ToLower() == varName.ToLower())
            {
                variableList.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sets the bool value of the variable. Returns false if variable not found, ignoring case
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetVariableBoolValue(string variableName, bool value)
    {
        for (int i = 0; i < variableList.Count; i++)
        {
            if (variableList[i].name.ToLower() == variableName.ToLower())
            {
                variableList[i].boolVal = value;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sets the int value of the variable. Returns false if variable not found, ignoring case
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetVariableIntValue(string variableName, int value)
    {
        for (int i = 0; i < variableList.Count; i++)
        {
            if (variableList[i].name.ToLower() == variableName.ToLower())
            {
                variableList[i].intVal = value;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sets the float value of the variable. Returns false if variable not found, ignoring case
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetVariableFloatValue(string variableName, float value)
    {
        for (int i = 0; i < variableList.Count; i++)
        {
            if (variableList[i].name.ToLower() == variableName.ToLower())
            {
                variableList[i].floatVal = value;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sets the string value of the variable. Returns false if variable not found, ignoring case
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetVariableStringValue(string variableName, string value)
    {
        for (int i = 0; i < variableList.Count; i++)
        {
            if (variableList[i].name.ToLower() == variableName.ToLower())
            {
                variableList[i].stringVal = value;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the bool value of the variable. Returns false if variable is not found
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public bool GetVariableBoolValue(string variableName)
    {
        for (int i = 0; i < variableList.Count; i++)
        {
            if (variableList[i].name.ToLower() == variableName.ToLower())
            {
                return variableList[i].boolVal;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the bool value of the variable. Returns -1 if variable is not found
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public int GetVariableIntValue(string variableName)
    {
        for (int i = 0; i < variableList.Count; i++)
        {
            if (variableList[i].name.ToLower() == variableName.ToLower())
            {
                return variableList[i].intVal;
            }
        }

        return -1;
    }

    /// <summary>
    /// Returns the bool value of the variable. Returns -1f if variable is not found
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public float GetVariableFloatValue(string variableName)
    {
        for (int i = 0; i < variableList.Count; i++)
        {
            if (variableList[i].name.ToLower() == variableName.ToLower())
            {
                return variableList[i].floatVal;
            }
        }

        return -1f;
    }

    /// <summary>
    /// Returns the string value of the variable. Returns NULL if variable is not found
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public string GetVariableStringValue(string variableName)
    {
        for (int i = 0; i < variableList.Count; i++)
        {
            if (variableList[i].name.ToLower() == variableName.ToLower())
            {
                return variableList[i].stringVal;
            }
        }

        return "NULL";
    }
#endregion
}

#region Node Definitions
//We need the definitions here instead of in the Node.cs file because of Editor Script runtime permissions
//ADD NEW NODES HERE
[System.Serializable]
public class InfoNode
{
    //Constructors
    public InfoNode()
    {
    }

    public InfoNode(InfoNode IN)
    {
        speakerName = IN.speakerName;
        speakerSprite = IN.speakerSprite;
        //speakerObj = IN.speakerObj;
    }

    //Vars
    const string infoText = "The Info Node can’t be created or destroyed. It holds all " +
                       "the basic info for the dialogue tree such as the name of the " +
                       "speaker and sprite. All of these are optional. To change info, " +
                       "use a Change Speaker Node or use the Advanced Options on a Dialogue Node.";

    public string GetInfoString() { return infoText; }

    public string speakerName = "";

    public Sprite speakerSprite = null;

    //Since Unity can't use Scene GameObjects when outside of the scene there isn't a
    //good way to allow the player to assign a GameObject inside the tree. Will revisit this in the future
    //***
    //public GameObject speakerObj = null;
    //public int speakerObjectID;

    public string GetSpeakerName()
    {
        return speakerName;
    }

    public void SetSpeakerName(string str)
    {
        speakerName = str;
    }

    public Sprite GetSpeakerSprite()
    {
        return speakerSprite;
    }

    public void SetSpeakerSprite(Sprite spr)
    {
        speakerSprite = spr;
    }

    //Since Unity can't use Scene GameObjects when outside of the scene there isn't a
    //good way to allow the player to assign a GameObject inside the tree. Will revisit this in the future
    //***
    //public GameObject GetSpeakerObject()
    //{
    //    return speakerObj;
    //}
    //
    //public void SetSpeakerObject(GameObject go)
    //{
    //    speakerObj = go;
    //}
}
[System.Serializable]
public class StartNode
{
    //Constructors
    public StartNode()
    {
    }

    public StartNode(StartNode SN)
    {
        startID = SN.startID;
    }

    //Vars
    const string infoText = "The Start Node controls the start of the dialogue logic." +
                      " A tree can have multiple start nodes, which are split by a " +
                      "Start ID number. You can choose which start node to start " +
                      "with when calling the tree in the reader script. If not " +
                      "specified, the tree will start with the lowest numbered node.";

    public string GetInfoString() { return infoText; }

    public int startID = 0;

    public int GetID()
    {
        return startID;
    }

    public void SetID(int id)
    {
        startID = id;
    }
}
[System.Serializable]
public class DialogueNode
{
    //Constructors
    public DialogueNode()
    {
    }

    public DialogueNode(DialogueNode DN)
    {
        dialogue = DN.dialogue;
        advancedOptions = DN.advancedOptions;
        speakerName = DN.speakerName;
        speakerSprite = DN.speakerSprite;
        //speakerObj = DN.speakerObj;
    }

    //Vars
    const string infoText = "The Dialogue Node is where dialogue will be read from at " +
                      "run time. Use {var:VarName:VarType} or {varGlobal:VarName:VarType} " +
                      "to insert variables into the text (Eg: {var:Oranges:int}). There is " +
                      "also {var:SpeakerName} which gets the current speaker name. Also, " +
                      "use the Advanced Options bool to set the speaker for this dialogue " +
                      "individually, as opposed to using a New Speaker Node.";

    public string GetInfoString() { return infoText; }

    public string dialogue = "";

    public string GetDialogue()
    {
        return dialogue;
    }

    public void SetDialogue(string str)
    {
        dialogue = str;
    }

    //Advanced options
    public bool advancedOptions = false;

    //Allow for speaker switching per individual dialogue box
    public string speakerName = "";
    public Sprite speakerSprite = null;
    //public GameObject speakerObj = null;
}
[System.Serializable]
public class BranchNode
{
    //Constructors
    public BranchNode()
    {
    }

    public BranchNode(BranchNode BN)
    {
        inPins = BN.inPins;
        outPins = BN.outPins;
    }

    //Vars
    const string infoText = "The Branch Node is used to make branching choices in a tree. " +
                      "It defaults to three in and two out connections. The first in " +
                      "connection will be the dialogue for the question, while the other " +
                      "two connections will be for the names of the options. The options " +
                      "are directly across from their output. This can be expanded to " +
                      "add/remove pins for more options by right clicking on the node.";

    public string GetInfoString() { return infoText; }

    public int inPins = 3;

    public int outPins = 2;

    public int GetInPins()
    {
        return inPins;
    }

    public int GetOutPins()
    {
        return outPins;
    }
}
[System.Serializable]
public class VariableNode
{
    //Constructors
    public VariableNode()
    {
    }

    public VariableNode(VariableNode VN)
    {
        selectedVarIndex = VN.selectedVarIndex;
        selectedVarTypeIndex = VN.selectedVarTypeIndex;

        //bValue = VN.bValue;
        //iValue = VN.iValue;
        //fValue = VN.fValue;
        //sValue = VN.sValue;

        isGlobal = VN.isGlobal;
    }

    //Vars
    const string localInfoText = "The Local Variable Node is a node representation of " +
                          "the local tree variables. They are used with the " +
                          "Logic and Edit Variable Nodes.";

    const string globalInfoText = "The Global Variable Node is a node representation of " +
                            "the global variables. They are used with the Logic " +
                            "and Edit Variable Nodes. If you do not see your global " +
                            "variable, make sure the EZPZ_InfoHolder prefab is up to date.";

    public string GetInfoString(bool isLocal) { if (isLocal) return localInfoText; else return globalInfoText; }

    public int selectedVarIndex = 0;
    
    public int selectedVarTypeIndex = 0;

    public bool locked = false;

    //public bool bValue = false;
    //public int iValue = 0;
    //public float fValue = 0.0f;
    //public string sValue = "";

    public bool isGlobal = false;
}
[System.Serializable]
public class LogicNode
{
    //Constructors
    public LogicNode()
    {
    }

    public LogicNode(LogicNode LN)
    {
        selectedVarTypeIndex = LN.selectedVarTypeIndex;
        selectedEquationTypeIndex = LN.selectedEquationTypeIndex;
        aValue = LN.aValue;
        bValue = LN.bValue;
    }

    //Vars
    const string infoText = "The Logic Node allows for runtime path changing based off of " +
                      "variable values. The top connection takes in the flow, while " +
                      "the bottom two take in the A and B variable respectively. " +
                      "Leaving the inputs open is fine as well, you can use the default values. " +
                      "Depending on the outcome of the question, the flow " +
                      "will continue out the True or False out connections.";

    public string GetInfoString() { return infoText; }

    public enum VarType
    {
        BOOL = 0,
        INT = 1,
        FLOAT = 2,
        STRING = 3
    }

    public enum EquationType
    {
        EQUALS = 0,
        NOT_EQUAL = 1, 
        GREATER_THAN = 2,
        GREATER_OR_EQUAL = 3,
        LESS_THAN = 4,
        LESS_OR_EQUAL = 5,
        AND = 6,
        OR = 7
    }

    [Tooltip("0-3: Bool, Int, Float, String")]
    public int selectedVarTypeIndex;
    [Tooltip("0-7: Equals, Not Equal, Greater Than, Greater or Equal, Less Than, Less or Equal, And (Bool only), Or (Bool only)")]
    public int selectedEquationTypeIndex;

    public DialogueTreeScriptableObj.DefaultValues aValue;
    public DialogueTreeScriptableObj.DefaultValues bValue;

    public bool hasA;
    public bool hasB;

    public int GetVarCompareTypeAsInt()
    {
        return selectedVarTypeIndex;
    }

    public string GetVarCompareTypeAsString()
    {
        return System.Enum.GetName(typeof(VarType), selectedVarTypeIndex);
    }

    public int GetEquationTypeAsInt()
    {
        return selectedEquationTypeIndex;
    }

    public string GetEquationTypeAsString()
    {
        return System.Enum.GetName(typeof(EquationType), selectedEquationTypeIndex);
    }

    public string GetEquationTypeAsFormattedString(int index)
    {
        switch (index)
        {
            case 0:
                return "A == B";

            case 1:
                return "A != B";

            case 2:
                return "A > B";

            case 3:
                return "A >= B";

            case 4:
                return "A < B";

            case 5:
                return "A <= B";

            case 6:
                return "A && B";

            case 7:
                return "A || B";

            default:
                return "NULL";
        }
    }
}
[System.Serializable]
public class EditVariableNode
{
    //Constructors
    public EditVariableNode()
    {
    }

    public EditVariableNode(EditVariableNode EN)
    {
        selectedVarTypeIndex = EN.selectedVarTypeIndex;
        selectedEquationTypeIndex = EN.selectedEquationTypeIndex;
        aValue = EN.aValue;
        bValue = EN.bValue;
    }

    //Vars
    const string infoText = "The Edit Variable Node allows for runtime changing of local " +
                      "and global variable values. The top connection takes in the " +
                      "flow, while the bottom two take in the A and B variable " +
                      "respectively. The A variable will be the one edited by the " +
                      "selected equation.";

    public string GetInfoString() { return infoText; }

    public enum VarType
    {
        BOOL = 0,
        INT = 1,
        FLOAT = 2,
        STRING = 3
    }

    public enum EquationType
    {
        SET = 0,
        ADD = 1,
        SUBTRACT = 2,
        MULTIPLY = 3,
        DIVIDE = 4
    }

    [Tooltip("0-3: Bool, Int, Float, String")]
    public int selectedVarTypeIndex;
    [Tooltip("0-5: Set, Add, Subtract, Multiply, Divide")]
    public int selectedEquationTypeIndex;

    public DialogueTreeScriptableObj.DefaultValues aValue;
    public DialogueTreeScriptableObj.DefaultValues bValue;

    public bool hasA;
    public bool hasB;

    public int GetVarCompareTypeAsInt()
    {
        return selectedVarTypeIndex;
    }

    public string GetVarCompareTypeAsString()
    {
        return System.Enum.GetName(typeof(VarType), selectedVarTypeIndex);
    }

    public int GetEquationTypeAsInt()
    {
        return selectedEquationTypeIndex;
    }

    public string GetEquationTypeAsString()
    {
        return System.Enum.GetName(typeof(EquationType), selectedEquationTypeIndex);
    }
}
[System.Serializable]
public class CommentNode
{
    //Constructors
    public CommentNode()
    {
    }

    public CommentNode(CommentNode CN)
    {
        comment = CN.comment;
    }

    //Vars
    const string infoText = "The Comment Node allows you to write comments " +
                      "to yourself about certain areas in the tree.";

    public string GetInfoString() { return infoText; }

    public string comment = "";
}
[System.Serializable]
public class NewSpeakerNode
{
    //Constructors
    public NewSpeakerNode()
    {
    }

    public NewSpeakerNode(NewSpeakerNode NN)
    {
        speakerName = NN.speakerName;
        speakerSprite = NN.speakerSprite;
        //speakerObj = NN.speakerObj;
    }

    //Vars
    const string infoText = "The New Speaker Node allows for runtime changing the tree's " +
                      "info. This is good for quick quips from other characters " +
                      "without the need for another dialogue tree.";

    public string GetInfoString() { return infoText; }

    public string speakerName = "";
    public Sprite speakerSprite = null;
    //public GameObject speakerObj = null;
}
[System.Serializable]
public class StartChangeNode
{
    //Constructors
    public StartChangeNode()
    {
    }

    public StartChangeNode(StartChangeNode SCN)
    {
        startID = SCN.startID;
    }

    //Vars
    const string infoText = "The Start Change Node changes the default start index. " +
                            "Useful for skipping one time introductions or " +
                            "variable based interactions.";

    public string GetInfoString() { return infoText; }

    public int startID = 0;

    public int GetID()
    {
        return startID;
    }

    public void SetID(int id)
    {
        startID = id;
    }
}
[System.Serializable]
public class TeleportFlowNode
{
    //Constructors
    public TeleportFlowNode()
    {
    }

    public TeleportFlowNode(TeleportFlowNode TFN)
    {
        isIn = TFN.isIn;
        teleportID = TFN.teleportID;
    }

    //Vars
    const string infoText = "The Teleport Flow Node links two nodes without using a " +
                            "flow line. This is useful for linking nodes that may be far apart.";

    public string GetInfoString() { return infoText; }

    public bool isIn = true;
    public int teleportID = 0;

    public int GetID()
    {
        return teleportID;
    }

    public void SetID(int id)
    {
        teleportID = id;
    }
}
#endregion
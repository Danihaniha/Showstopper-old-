//Code by Zach Phillips for the EZPZ Branching Dialogue toolset
//Used tutorial for basic window set up: https://gram.gs/gramlog/creating-node-based-editor-unity/
//All other code is copyright 2020, Zach Phillips, All rights reserved
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using Microsoft.Win32.SafeHandles;

public struct NodeConnectionCouple
{
    public Node n;
    public int index;
}

//Basic container for the node class
public class Node{

    DialogueTreeScriptableObj branchingTreeObj;

    //Popup content
    string[] treeVarList;
    string[] conditionalVariableList;
    string[] conditionalEquationList;
    string[] editEquationList;

    //Temp/Tester values
    int oldConditionalVariableValue = -1;
    int oldEditVariableValue = -1;

    //Help values
    public Rect helpRect;
    public string myHelpInfo;
    public bool helpHoveredOver = false;

    public enum NodeType
    {
        //ADD NEW NODES HERE
        INFO = 0,
        START = 1,
        END = 2,
        DIALOGUE = 3,
        BRANCH = 4,
        VARIABLE = 5,
        LOGIC = 6,
        EDIT_VARIABLE = 7,
        COMMENT = 8,
        NEW_SPEAKER = 9,
        START_CHANGE = 10,
        TELEPORT_FLOW = 11
    }

    //Node inits
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

    public int ID = -1;

    //Used for node visuals
    public Rect rect;
    Vector2 labelPosition;
    public string title;
    public bool beingDragged;
    public bool isSelected;

    NodeType type;
    Vector2 position;
    int dialogueNodeHeightAdjustment = 0;
    int commentNodeHeightAdjustment = 0;
    int branchNodeHeightAdjustment = 0;

    public List<ConnectionPoint> inPoint = new List<ConnectionPoint>();
    public List<ConnectionPoint> outPoint = new List<ConnectionPoint>();

    //Styles
    public GUIStyle style = new GUIStyle();
    public GUIStyle defaultNodeStyle = new GUIStyle();
    public GUIStyle selectedNodeStyle = new GUIStyle();

    public Action<Node> OnRemoveNode;
    public Action<Node> OnDuplicateNode;

    //Change this to include a connection index...maybe another public enum
    public List<NodeConnectionCouple> inNodes = new List<NodeConnectionCouple>();
    public List<NodeConnectionCouple> outNodes = new List<NodeConnectionCouple>();

    GUIStyle textStyle = new GUIStyle();

    Font boldFont;
    Font regularFont;

    GUIStyle inStyle = new GUIStyle();
    GUIStyle outStyle = new GUIStyle();
    Action<ConnectionPoint> inClickAction;
    Action<ConnectionPoint> outClickAction;

    //Constructor
    public Node(DialogueTreeScriptableObj treeObj, int nodeType, Vector2 pos, GUIStyle inPointStyle, GUIStyle outPointStyle, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<Node> OnClickRemoveNode, Action<Node> OnClickDuplicateNode)
    {
        branchingTreeObj = treeObj;

        //ADD NEW NODES HERE
        dN = new DialogueNode();
        sN = new StartNode();
        iN = new InfoNode();
        bN = new BranchNode();
        vN = new VariableNode();
        lN = new LogicNode();
        eN = new EditVariableNode();
        cN = new CommentNode();
        nN = new NewSpeakerNode();
        scN = new StartChangeNode();
        tfN = new TeleportFlowNode();

        type = (NodeType)nodeType;

        Vector2 widthHeight = Vector2.zero;
        widthHeight = DecideNodeSizeAndHelpInfo();

        position = pos;
        rect = new Rect(position.x, position.y, widthHeight.x, widthHeight.y);
        helpRect = new Rect(rect.x + rect.width - 20, rect.y + 7, 20, 20);
        inStyle = inPointStyle;
        outStyle = outPointStyle;
        inClickAction = OnClickInPoint;
        outClickAction = OnClickOutPoint;
        SetInOutButtons();
        OnRemoveNode = OnClickRemoveNode;
        OnDuplicateNode = OnClickDuplicateNode;

        CreateNodeStyle();
        boldFont = (Font)Resources.Load("Roboto-Bold");
        regularFont = (Font)Resources.Load("Roboto-Regular");

        textStyle.normal.textColor = Color.white;
        textStyle.fontSize = 14;
        textStyle.font = boldFont;

        SetUpPopupContentLists();
    }

    void SetUpPopupContentLists()
    {
        treeVarList = new string[branchingTreeObj.variableList.Count];
        for (int i = 0; i < branchingTreeObj.variableList.Count; i++)
        {
            treeVarList[i] = branchingTreeObj.variableList[i].name;
        }

        conditionalVariableList = new string[Enum.GetNames(typeof(LogicNode.VarType)).Length];
        for (int i = 0; i < Enum.GetNames(typeof(LogicNode.VarType)).Length; i++)
        {
            conditionalVariableList[i] = Enum.GetNames(typeof(LogicNode.VarType))[i];
        }

        SetUpVariableEquationList();

        SetUpEditVariableEquationList();
    }

    void SetUpVariableEquationList()
    {
        switch (lN.selectedVarTypeIndex)
        {
            case 0: //Bool
                conditionalEquationList = new string[] { "A == B", "A != B", "A \uff06\uff06 B", "A || B" };
                //So apparently when creating a custom popup menu, the menu uses the & escape character but not other standard characters like \n. 
                //Meanwhile, the value box which shows the user's choice does not use the & character but s affected by the other standard escape 
                //characters. So that means the menu displays as A & B, while the value box displays A && B, which is the literal string. So to get 
                //&& to display in both, I need to use \uff06 twice, the unicode character for a fullwidth ampersand. \u0026, a normal ampersand 
                //doesn't work, only the fullwidth ones. Who knows at this point lol
                if (lN.selectedEquationTypeIndex > 3)
                    lN.selectedEquationTypeIndex = 0;
                break;
            case 3: //String
                conditionalEquationList = new string[] { "A == B", "A != B"};
                if (lN.selectedEquationTypeIndex > 1)
                    lN.selectedEquationTypeIndex = 0;
                break;

            case 1: //Int
            case 2: //Float
                conditionalEquationList = new string[] { "A == B", "A != B", "A > B", "A >= B", "A < B", "A <= B"};
                break;
        }        
    }

    void SetUpEditVariableEquationList()
    {
        switch (eN.selectedVarTypeIndex)
        {
            case 0: //Bool
                editEquationList = new string[] { "A = B" };
                eN.selectedEquationTypeIndex = 0;
                break;
            case 3: //String
                editEquationList = new string[] { "A = B", "A + B", "A - B" };
                if (eN.selectedEquationTypeIndex > 2)
                    eN.selectedEquationTypeIndex = 0;
                break;

            case 1: //Int
            case 2: //Float
                editEquationList = new string[] { "A = B", "A + B", "A - B", "A * B", "A \u005c B" };
                break;
        }
    }

    public void SetInOutPointsExplicit(Vector2 inOut)
    {
        for (int i = 0; i < inOut.x; i++)
        {
            if (i > inPoint.Count)
                inPoint.Add(new ConnectionPoint(this, ConnectionPointType.In, inStyle, inClickAction));
        }
        for (int i = 0; i < inOut.y; i++)
        {
            if (i > outPoint.Count)
                outPoint.Add(new ConnectionPoint(this, ConnectionPointType.Out, outStyle, outClickAction));
        }
    }

    void CreateNodeStyle()
    {
        //https://gist.github.com/eppz/bb9d77b6444524857391a0e4822ca6c0
        //has node styles 0-6
        //grey, blue, teal, green, yellow, orange, red
        //ADD NEW NODES HERE
        switch (type)
        {
            case NodeType.INFO:
            case NodeType.COMMENT:
                defaultNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node0.png") as Texture2D;
                selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node0 on.png") as Texture2D;
                break;

            case NodeType.START:
            case NodeType.START_CHANGE:
                defaultNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3.png") as Texture2D;
                selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node3 on.png") as Texture2D;
                break;

            case NodeType.END:
                defaultNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node6.png") as Texture2D;
                selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node6 on.png") as Texture2D;
                break;

            case NodeType.DIALOGUE:
            case NodeType.NEW_SPEAKER:
                defaultNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
                selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
                break;

            case NodeType.BRANCH:
            case NodeType.TELEPORT_FLOW:
                defaultNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2.png") as Texture2D;
                selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node2 on.png") as Texture2D;
                break;

            case NodeType.VARIABLE:
            case NodeType.EDIT_VARIABLE:
                defaultNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5.png") as Texture2D;
                selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node5 on.png") as Texture2D;
                break;

            case NodeType.LOGIC:
                defaultNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node4.png") as Texture2D;
                selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node4 on.png") as Texture2D;
                break;
        }

        defaultNodeStyle.border = new RectOffset(12, 12, 12, 12);
        selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);
        style = defaultNodeStyle;

        EditorStyles.textField.wordWrap = true;
        EditorStyles.textField.stretchHeight = true;
    }

    Vector2 DecideNodeSizeAndHelpInfo()
    {
        Vector2 temp = Vector2.zero;

        //ADD NEW NODES HERE
        switch (type)
        {
            case NodeType.INFO:
                temp.x = 250;
                temp.y = 135;
                labelPosition = new Vector2(112, 15);
                myHelpInfo = iN.GetInfoString();
                break;

            case NodeType.START:
                temp.x = 100;
                temp.y = 60;
                labelPosition = new Vector2(30, 15);
                myHelpInfo = sN.GetInfoString();
                break;

            case NodeType.DIALOGUE:
                temp.x = 210;
                temp.y = 100;
                labelPosition = new Vector2(85, 15);
                myHelpInfo = dN.GetInfoString();
                break;

            case NodeType.END:
                temp.x = 100;
                temp.y = 50;
                labelPosition = new Vector2(38, 15);
                myHelpInfo = "The End Node ends the dialogue tree. There can be " +
                              "multiple End Nodes in a tree.";
                break;

            case NodeType.BRANCH:
                temp.x = 210;
                temp.y = 160;
                labelPosition = new Vector2(100, 15);
                myHelpInfo = bN.GetInfoString();
                break;

            case NodeType.VARIABLE:
                temp.x = 150;
                temp.y = 60;
                labelPosition = new Vector2(20, 15);
                break;

            case NodeType.LOGIC:
                temp.x = 150;
                temp.y = 140;
                labelPosition = new Vector2(55, 15);
                myHelpInfo = lN.GetInfoString();
                break;

            case NodeType.EDIT_VARIABLE:
                temp.x = 150;
                temp.y = 140;
                labelPosition = new Vector2(30, 15);
                myHelpInfo = eN.GetInfoString();
                break;

            case NodeType.COMMENT:
                temp.x = 210;
                temp.y = 100;
                labelPosition = new Vector2(75, 15);
                myHelpInfo = cN.GetInfoString();
                break;

            case NodeType.NEW_SPEAKER:
                temp.x = 240;
                temp.y = 135;
                labelPosition = new Vector2(85, 15);
                myHelpInfo = nN.GetInfoString();
                break;

            case NodeType.START_CHANGE:
                temp.x = 140;
                temp.y = 60;
                labelPosition = new Vector2(25, 15);
                myHelpInfo = scN.GetInfoString();
                break;

            case NodeType.TELEPORT_FLOW:
                temp.x = 140;
                temp.y = 60;
                labelPosition = new Vector2(25, 15);
                myHelpInfo = tfN.GetInfoString();
                break;
        }

        return temp;
    }

    //For nodes that have multiple in/out connections
    //ADD NEW NODES HERE
    void SetInOutButtons()
    {
        switch (type)
        {
            case NodeType.BRANCH:
                for (int i = 0; i < bN.inPins; i++)
                {
                    inPoint.Add(new ConnectionPoint(this, ConnectionPointType.In, inStyle, inClickAction));
                }
                for (int i = 0; i < bN.outPins; i++)
                {
                    outPoint.Add(new ConnectionPoint(this, ConnectionPointType.Out, outStyle, outClickAction));
                }
                break;

            case NodeType.LOGIC: //Needs three in two out
                inPoint.Add(new ConnectionPoint(this, ConnectionPointType.In, inStyle, inClickAction));
                inPoint.Add(new ConnectionPoint(this, ConnectionPointType.In, inStyle, inClickAction));
                inPoint.Add(new ConnectionPoint(this, ConnectionPointType.In, inStyle, inClickAction));
                outPoint.Add(new ConnectionPoint(this, ConnectionPointType.Out, outStyle, outClickAction));
                outPoint.Add(new ConnectionPoint(this, ConnectionPointType.Out, outStyle, outClickAction));
                break;

            case NodeType.EDIT_VARIABLE: //Needs three in one out
                inPoint.Add(new ConnectionPoint(this, ConnectionPointType.In, inStyle, inClickAction));
                inPoint.Add(new ConnectionPoint(this, ConnectionPointType.In, inStyle, inClickAction));
                inPoint.Add(new ConnectionPoint(this, ConnectionPointType.In, inStyle, inClickAction));
                outPoint.Add(new ConnectionPoint(this, ConnectionPointType.Out, outStyle, outClickAction));
                break;

            case NodeType.START:
            case NodeType.VARIABLE:
                outPoint.Add(new ConnectionPoint(this, ConnectionPointType.Out, outStyle, outClickAction));
                break;

            case NodeType.COMMENT:
            case NodeType.INFO:
                break;

            default:
                inPoint.Add(new ConnectionPoint(this, ConnectionPointType.In, inStyle, inClickAction));
                outPoint.Add(new ConnectionPoint(this, ConnectionPointType.Out, outStyle, outClickAction));
                break;
        }
    }

    public void AddInNode(Node addedNode, int index)
    {
        NodeConnectionCouple tempNCC;
        tempNCC.n = addedNode;
        tempNCC.index = index;
        inNodes.Add(tempNCC);

        //Check if logic node and if so remove HasA/HasB
        if (type == NodeType.LOGIC)
        {
            if (index == 1)
                lN.hasA = true;
            else if (index == 2)
                lN.hasB = true;
        }
        //Check if edit node and if so remove HasA/HasB
        else if (type == NodeType.EDIT_VARIABLE)
        {
            if (index == 1)
                eN.hasA = true;
            else if (index == 2)
                eN.hasB = true;
        }
    }

    public void AddOutNode(Node addedNode, int index)
    {
        NodeConnectionCouple tempNCC;
        tempNCC.n = addedNode;
        tempNCC.index = index;
        outNodes.Add(tempNCC);

        //Check if variable node and if so remove lock
        if (type == NodeType.VARIABLE)
        {
            if (addedNode.type == NodeType.LOGIC)
            {
                vN.selectedVarTypeIndex = addedNode.lN.selectedVarTypeIndex;
                vN.locked = true;
            }

            if (addedNode.type == NodeType.EDIT_VARIABLE)
            {
                vN.selectedVarTypeIndex = addedNode.eN.selectedVarTypeIndex;
                vN.locked = true;
            }
        }
    }

    public void RemoveInNode(NodeConnectionCouple ncc)
    {
        inNodes.Remove(ncc);

        //Check if logic node and if so remove HasA/HasB
        if (type == NodeType.LOGIC)
        {
            if (ncc.index == 1)
                lN.hasA = false;
            else if (ncc.index == 2)
                lN.hasB = false;
        }
        //Check if edit node and if so remove HasA/HasB
        else if (type == NodeType.EDIT_VARIABLE)
        {
            if (ncc.index == 1)
                eN.hasA = false;
            else if (ncc.index == 2)
                eN.hasB = false;
        }
    }

    public void RemoveOutNode(NodeConnectionCouple ncc)
    {
        outNodes.Remove(ncc);

        //Check if variable node and if so remove lock
        if (type == NodeType.VARIABLE)
        {
            vN.locked = false;
        }
    }

    //Update rect position on drag
    public void Drag(Vector2 delta)
    {
        delta /= NodeEditor.zoomScale;
        rect.position += delta;
        helpRect.position += delta;
        position += delta;
    }

    //Display on the window
    public void Draw(int i, float zoomScale)
    {
        DrawInOutConnections(zoomScale);

        Rect scaledRect;
        GUIStyle fontStyle;

        //ADD NEW NODES HERE
        switch (type)
        {
            //If the node gets dynamically longer, do it here. Else, draw normal node
            case NodeType.DIALOGUE:
                if (dN != null)
                    dialogueNodeHeightAdjustment = (int)GUI.skin.box.CalcHeight(new GUIContent(dN.dialogue), rect.width - 20) - 10;

                int advancedOptionsOffset = dN.advancedOptions ? 115 : 15;

                if (dialogueNodeHeightAdjustment > rect.height - (labelPosition.y * 3))
                {
                    scaledRect = new Rect(rect.x, rect.y, rect.width, rect.height + dialogueNodeHeightAdjustment - 40 + advancedOptionsOffset);
                    scaledRect.size *= zoomScale;
                    scaledRect.position *= zoomScale;
                    GUI.Box(scaledRect, title, style);
                }
                else
                {
                    scaledRect = new Rect(rect.x, rect.y, rect.width, rect.height + advancedOptionsOffset);
                    scaledRect.size *= zoomScale;
                    scaledRect.position *= zoomScale;
                    GUI.Box(scaledRect, title, style);
                }
                break;

            case NodeType.COMMENT:
                if (cN != null)
                    commentNodeHeightAdjustment = (int)GUI.skin.box.CalcHeight(new GUIContent(cN.comment), rect.width - 20) - 10;

                if (commentNodeHeightAdjustment > rect.height - (labelPosition.y * 3))
                {
                    scaledRect = new Rect(rect.x, rect.y, rect.width, rect.height + commentNodeHeightAdjustment - 40);
                    scaledRect.size *= zoomScale;
                    scaledRect.position *= zoomScale;
                    GUI.Box(scaledRect, title, style);
                }
                else
                {
                    scaledRect = rect;
                    scaledRect.size *= zoomScale;
                    scaledRect.position *= zoomScale;
                    GUI.Box(scaledRect, title, style);
                }
                break;

            case NodeType.BRANCH:
                //Lengthen and shorten based on how many pins
                if (bN.inPins >= bN.outPins)
                    branchNodeHeightAdjustment = (bN.inPins - 3) * 30;
                else
                    branchNodeHeightAdjustment = (bN.outPins - 3) * 30;

                scaledRect = new Rect(rect.x, rect.y, rect.width, rect.height + branchNodeHeightAdjustment);
                scaledRect.size *= zoomScale;
                scaledRect.position *= zoomScale;
                GUI.Box(scaledRect, title, style);
                break;

            case NodeType.TELEPORT_FLOW:
                //Special color
                GUI.color = new Color(1f, .6f, 1f);
                scaledRect = rect;
                scaledRect.size *= zoomScale;
                scaledRect.position *= zoomScale;
                GUI.Box(scaledRect, title, style);
                //GUI.Box(new Rect(rect.x, rect.y, rect.width, rect.height + branchNodeHeightAdjustment), title, style);
                GUI.color = Color.white;
                break;

            default:
                scaledRect = rect;
                scaledRect.size *= zoomScale;
                scaledRect.position *= zoomScale;
                GUI.Box(scaledRect, title, style);
                break;
        }

        ID = i;

        fontStyle = new GUIStyle(textStyle);
        fontStyle.fontSize = (int)(textStyle.fontSize * zoomScale);

        scaledRect = new Rect(rect.x + (labelPosition.x / 1.25f) * zoomScale, rect.y + labelPosition.y * zoomScale - 5, rect.width, rect.height);
        scaledRect.size *= zoomScale;

        //Draw name of node
        if (type == NodeType.VARIABLE)
        {
            if (!vN.isGlobal)
                GUI.Label(scaledRect, "LOCAL VARIABLE", fontStyle);
            else
                GUI.Label(scaledRect, "GLOBAL VARIABLE", fontStyle);
        }
        else
        {
            string name = type.ToString();
            if (name.Contains("_"))
                name = name.Replace("_", " ");

            GUI.Label(scaledRect, name, fontStyle);
        }

        //Draw help box
        Rect adjustedHelpRect = new Rect(rect.x + scaledRect.width - (25 * zoomScale), scaledRect.y, helpRect.width, helpRect.height);

        fontStyle.normal.textColor = Color.black;
        GUI.Label(adjustedHelpRect, "?", fontStyle);
        //fontStyle.normal.textColor = Color.white;
        helpHoveredOver = (adjustedHelpRect.Contains(Event.current.mousePosition)) ? true : false;

        DrawSpecifcProperties();
    }

    void DrawInOutConnections(float zoomScale)
    {
        //ADD NEW NODES HERE
        switch (type)
        {
            case NodeType.INFO:
            case NodeType.COMMENT:
                break;

            case NodeType.START:
                outPoint[0].Draw(zoomScale);
                break;

            case NodeType.END:
                inPoint[0].Draw(zoomScale);
                break;

            case NodeType.BRANCH:
                for (int i = 0; i < bN.inPins; i++)
                {
                    if (i > inPoint.Count - 1)
                        inPoint.Add(new ConnectionPoint(this, ConnectionPointType.In, inStyle, inClickAction));
                    inPoint[i].Draw(20 - (30 * i), zoomScale);

                }
                for (int i = 0; i < bN.outPins; i++)
                {
                    if (i > outPoint.Count - 1)
                        outPoint.Add(new ConnectionPoint(this, ConnectionPointType.Out, outStyle, outClickAction));
                    outPoint[i].Draw(-10 - (30 * i), zoomScale);
                }
                break;

            case NodeType.VARIABLE:
                outPoint[0].Draw(zoomScale);
                break;

            case NodeType.LOGIC:
                inPoint[0].Draw(20, zoomScale);
                inPoint[1].Draw(-15, zoomScale);
                inPoint[2].Draw(-45, zoomScale);
                outPoint[0].Draw(-15, zoomScale);
                outPoint[1].Draw(-45, zoomScale);
                break;

            case NodeType.EDIT_VARIABLE:
                inPoint[0].Draw(20, zoomScale);
                inPoint[1].Draw(-15, zoomScale);
                inPoint[2].Draw(-45, zoomScale);
                outPoint[0].Draw(-15, zoomScale);
                break;

            case NodeType.TELEPORT_FLOW:
                if (tfN.isIn)
                    inPoint[0].Draw(zoomScale);
                else
                    outPoint[0].Draw(zoomScale);
                break;

            case NodeType.NEW_SPEAKER:
            case NodeType.START_CHANGE:
            case NodeType.DIALOGUE:
                inPoint[0].Draw(zoomScale);
                outPoint[0].Draw(zoomScale);
                break;
        }
    }

    void DrawSpecifcProperties()
    {
        //ADD NEW NODES HERE
        switch (type)
        {
            case NodeType.INFO:
                EditorGUI.BeginChangeCheck();
                string tempInfoSpeakerName;

                EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2), rect.width - 20, 20), "Speaker Name:");
                tempInfoSpeakerName = EditorGUI.TextField(new Rect(rect.x + 110, rect.y + (labelPosition.y * 2), rect.width - 130, 20), iN.speakerName);

                //EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2) + 25, rect.width - 20, 20), "Speaker Obj:");
                //iN.speakerObj = (GameObject)EditorGUI.ObjectField(new Rect(rect.x + 110, rect.y + (labelPosition.y * 2) + 25, rect.width - 120, 20), "", iN.speakerObj, typeof(GameObject), true);

                EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2) + 25, rect.width - 20, 20), "Speaker Sprite:");
                iN.speakerSprite = (Sprite)EditorGUI.ObjectField(new Rect(rect.x + 110, rect.y + 60, 64, 64), "", iN.speakerSprite, typeof(Sprite), true);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(branchingTreeObj, "Info Speaker Name change");
                    iN.speakerName = tempInfoSpeakerName;
                }
                break;

            case NodeType.START:
                EditorGUI.BeginChangeCheck();
                int tempStartIndex;

                EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2), rect.width - 20, 20), "ID:");
                tempStartIndex = EditorGUI.IntField(new Rect(rect.x + 35, rect.y + (labelPosition.y * 2), rect.width - 50, 20), sN.startID);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(branchingTreeObj, "Start ID changed");
                    sN.startID = tempStartIndex;
                }
                break;

            case NodeType.DIALOGUE:
                EditorGUI.BeginChangeCheck();
                string tempDialogue;
                string tempDialogueSpeakerName;

                if (dialogueNodeHeightAdjustment > rect.height - (labelPosition.y * 3))
                    tempDialogue = EditorGUI.TextField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2), rect.width - 20, rect.height + dialogueNodeHeightAdjustment - (labelPosition.y * 3) - 40), dN.dialogue);
                else
                    tempDialogue = EditorGUI.TextField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2), rect.width - 20, rect.height - (labelPosition.y * 3)), dN.dialogue);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(branchingTreeObj, "Dialogue node edits");
                    dN.dialogue = tempDialogue;
                }

                //Draw advanced options
                if (dialogueNodeHeightAdjustment > rect.height - (labelPosition.y * 3))
                {
                    dN.advancedOptions = EditorGUI.Toggle(new Rect(rect.x + 10, rect.y + rect.height - 12 + (dialogueNodeHeightAdjustment - 40), rect.width - 50, 20), dN.advancedOptions);
                    EditorGUI.LabelField(new Rect(rect.x + 25, rect.y + rect.height - 12 + (dialogueNodeHeightAdjustment - 40), rect.width - 50, 20), "Advanced Options");

                    if (dN.advancedOptions)
                    {
                        EditorGUI.BeginChangeCheck();

                        EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + rect.height - 12 + 20 + (dialogueNodeHeightAdjustment - 40), rect.width - 20, 20), "Speaker Name:");
                        tempDialogueSpeakerName = EditorGUI.TextField(new Rect(rect.x + 110, rect.y + rect.height - 12 + 20 + (dialogueNodeHeightAdjustment - 40), rect.width - 120, 16), dN.speakerName);

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(branchingTreeObj, "Dialogue Speaker Name change");
                            dN.speakerName = tempDialogueSpeakerName;
                        }

                        //EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + rect.height - 12 + 45 + (dialogueNodeHeightAdjustment - 40), rect.width - 20, 20), "Speaker Obj:");
                        //dN.speakerObj = (GameObject)EditorGUI.ObjectField(new Rect(rect.x + 110, rect.y + rect.height - 12 + 45 + (dialogueNodeHeightAdjustment - 40), rect.width - 120, 20), "", dN.speakerObj, typeof(GameObject), true);

                        EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + rect.height - 12 + 45 + (dialogueNodeHeightAdjustment - 40), rect.width - 20, 20), "Speaker Sprite:");
                        dN.speakerSprite = (Sprite)EditorGUI.ObjectField(new Rect(rect.x + 110, rect.y + rect.height - 12 + 45 + (dialogueNodeHeightAdjustment - 40), 64, 64), "", dN.speakerSprite, typeof(Sprite), true);
                    }
                }
                else
                {
                    dN.advancedOptions = EditorGUI.Toggle(new Rect(rect.x + 10, rect.y + rect.height - 12, rect.width - 50, 20), dN.advancedOptions);
                    EditorGUI.LabelField(new Rect(rect.x + 25, rect.y + rect.height - 12, rect.width - 50, 20), "Advanced Options");

                    if (dN.advancedOptions)
                    {
                        EditorGUI.BeginChangeCheck();

                        EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + rect.height - 12 + 20, rect.width - 20, 20), "Speaker Name:");
                        tempDialogueSpeakerName = EditorGUI.TextField(new Rect(rect.x + 110, rect.y + rect.height - 12 + 20, rect.width - 120, 16), dN.speakerName);

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(branchingTreeObj, "Dialogue Speaker Name change");
                            dN.speakerName = tempDialogueSpeakerName;
                        }

                        //EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + rect.height - 12 + 25 + 20, rect.width - 20, 20), "Speaker Obj:");
                        //dN.speakerObj = (GameObject)EditorGUI.ObjectField(new Rect(rect.x + 110, rect.y + rect.height - 12 + 25 + 20, rect.width - 120, 20), "", dN.speakerObj, typeof(GameObject), true);

                        EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + rect.height - 12 + 25 + 20, rect.width - 20, 20), "Speaker Sprite:");
                        dN.speakerSprite = (Sprite)EditorGUI.ObjectField(new Rect(rect.x + 110, rect.y + rect.height - 12 + 25 + 20, 64, 64), "", dN.speakerSprite, typeof(Sprite), true);
                    }
                }
                break;

            case NodeType.BRANCH:
                EditorGUI.LabelField(new Rect(rect.x + 10, inPoint[0].rect.y, rect.width - 20, 20), "Branch Question");
                for (int i = 1; i < inPoint.Count; i++)
                {
                    EditorGUI.LabelField(new Rect(rect.x + 10, inPoint[i].rect.y, rect.width - 20, 20), "Answer " + (i));
                }
                for (int i = 0; i < outPoint.Count; i++)
                {
                    EditorGUI.LabelField(new Rect(rect.x + (rect.width - 70), outPoint[i].rect.y, rect.width - 20, 20), "Choice " + (i + 1));
                }
                break;

            case NodeType.VARIABLE:              
                EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2), rect.width - 20, 20), "Var:");

                if (!vN.isGlobal)
                {
                    treeVarList = new string[branchingTreeObj.variableList.Count];
                    for (int i = 0; i < branchingTreeObj.variableList.Count; i++)
                        treeVarList[i] = branchingTreeObj.variableList[i].name;

                    //treeVarList = new string[branchingTreeObj.variableList.Count + 1];
                    //treeVarList[0] = "Literal Value";
                    //for (int i = 0; i < branchingTreeObj.variableList.Count; i++)
                    //    treeVarList[i + 1] = branchingTreeObj.variableList[i].name;
                }
                else
                {
                    treeVarList = new string[EZPZ_TreeInfoHolder.Instance.globalVariableList.Count];
                    for (int i = 0; i < EZPZ_TreeInfoHolder.Instance.globalVariableList.Count; i++)
                        treeVarList[i] = EZPZ_TreeInfoHolder.Instance.globalVariableList[i].name;
                }

                vN.selectedVarIndex = EditorGUI.Popup(new Rect(rect.x + 40, rect.y + (labelPosition.y * 2), rect.width - 50, 20), vN.selectedVarIndex, treeVarList);

                //if (vN.selectedVarIndex == 0 && !vN.getIsGlobalVar())
                //{
                //    //Change the node size if it's a litteral value
                //    if (rect.height != 105)
                //        rect.height = 105;
                //
                //    if (!vN.locked)
                //        vN.selectedVarTypeIndex = EditorGUI.Popup(new Rect(rect.x + 40, rect.y + (labelPosition.y * 2) + 20, rect.width - 50, 20), vN.selectedVarTypeIndex, conditionalVariableList);
                //    else
                //    {
                //        EditorGUI.LabelField(new Rect(rect.x + 40, rect.y + (labelPosition.y * 2) + 20, rect.width - 50, 20), "Type Locked");
                //    }
                //
                //    switch (vN.selectedVarTypeIndex)
                //    {
                //        case 0: //Bool
                //            vN.bValue = EditorGUI.Toggle(new Rect(rect.x + 40, rect.y + (labelPosition.y * 2) + 40, rect.width - 50, 20), vN.bValue);
                //            break;
                //
                //        case 1: //Int
                //            vN.iValue = EditorGUI.IntField(new Rect(rect.x + 40, rect.y + (labelPosition.y * 2) + 40, rect.width - 50, 20), vN.iValue);
                //            break;
                //
                //        case 2: //Float
                //            vN.fValue = EditorGUI.FloatField(new Rect(rect.x + 40, rect.y + (labelPosition.y * 2) + 40, rect.width - 50, 20), vN.fValue);
                //            break;
                //
                //        case 3: //String
                //            vN.sValue = EditorGUI.TextField(new Rect(rect.x + 40, rect.y + (labelPosition.y * 2) + 40, rect.width - 50, 20), vN.sValue);
                //            break;
                //    }
                //}
                //else if (rect.height != 60)
                //    rect.height = 60;

                break;

            case NodeType.LOGIC:
                EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2), rect.width - 20, 20), "Var Type:");
                EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2) + 20, rect.width - 20, 20), "Equation:");

                lN.selectedVarTypeIndex = EditorGUI.Popup(new Rect(rect.x + 75, rect.y + (labelPosition.y * 2), rect.width - 90, 20), lN.selectedVarTypeIndex, conditionalVariableList);
                if (oldConditionalVariableValue != lN.selectedVarTypeIndex)
                {
                    SetUpVariableEquationList();
                    
                    ////Check if any literal value variable nodes are around and change their values
                    //foreach (NodeConnectionCouple ncc in inNodes)
                    //{
                    //    if (ncc.n.type == NodeType.VARIABLE)
                    //        ncc.n.vN.selectedVarTypeIndex = lN.selectedVarTypeIndex;
                    //}

                    oldConditionalVariableValue = lN.selectedVarTypeIndex;
                }
                lN.selectedEquationTypeIndex = EditorGUI.Popup(new Rect(rect.x + 75, rect.y + (labelPosition.y * 2) + 20, rect.width - 90, 20), lN.selectedEquationTypeIndex, conditionalEquationList);

                EditorGUI.LabelField(new Rect(rect.x + (rect.width - 50), outPoint[0].rect.y, rect.width - 20, 20), "True:");
                EditorGUI.LabelField(new Rect(rect.x + (rect.width - 50), outPoint[1].rect.y, rect.width - 20, 20), "False:");

                EditorGUI.LabelField(new Rect(rect.x + 10, inPoint[1].rect.y, rect.width - 20, 20), "A");
                EditorGUI.LabelField(new Rect(rect.x + 10, inPoint[2].rect.y, rect.width - 20, 20), "B");
                
                if (!lN.hasA)
                {
                    switch (lN.selectedVarTypeIndex)
                    {
                        case 0: //Bool
                            lN.aValue.defaultBValue = EditorGUI.Toggle(new Rect(rect.x + 25, inPoint[1].rect.y, 20, 20), lN.aValue.defaultBValue);
                            break;

                        case 1: //Int
                            lN.aValue.defaultIValue = EditorGUI.IntField(new Rect(rect.x + 25, inPoint[1].rect.y, rect.width - 85, 20), lN.aValue.defaultIValue);
                            break;

                        case 2: //Float
                            lN.aValue.defaultFValue = EditorGUI.FloatField(new Rect(rect.x + 25, inPoint[1].rect.y, rect.width - 85, 20), lN.aValue.defaultFValue);
                            break;

                        case 3: //String
                            lN.aValue.defaultSValue = EditorGUI.TextField(new Rect(rect.x + 25, inPoint[1].rect.y, rect.width - 85, 20), lN.aValue.defaultSValue);
                            break;
                    }
                }

                if (!lN.hasB)
                {
                    switch (lN.selectedVarTypeIndex)
                    {
                        case 0: //Bool
                            lN.bValue.defaultBValue = EditorGUI.Toggle(new Rect(rect.x + 25, inPoint[2].rect.y, 20, 20), lN.bValue.defaultBValue);
                            break;

                        case 1: //Int
                            lN.bValue.defaultIValue = EditorGUI.IntField(new Rect(rect.x + 25, inPoint[2].rect.y, rect.width - 85, 20), lN.bValue.defaultIValue);
                            break;

                        case 2: //Float
                            lN.bValue.defaultFValue = EditorGUI.FloatField(new Rect(rect.x + 25, inPoint[2].rect.y, rect.width - 85, 20), lN.bValue.defaultFValue);
                            break;

                        case 3: //String
                            lN.bValue.defaultSValue = EditorGUI.TextField(new Rect(rect.x + 25, inPoint[2].rect.y, rect.width - 85, 20), lN.bValue.defaultSValue);
                            break;
                    }
                }

                break;

            case NodeType.EDIT_VARIABLE:
                EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2), rect.width - 20, 20), "Var Type:");
                EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2) + 20, rect.width - 20, 20), "Set A Using:");

                eN.selectedVarTypeIndex = EditorGUI.Popup(new Rect(rect.x + 75, rect.y + (labelPosition.y * 2), rect.width - 90, 20), eN.selectedVarTypeIndex, conditionalVariableList);
                if (oldEditVariableValue != eN.selectedVarTypeIndex)
                {
                    SetUpEditVariableEquationList();

                    ////Check if any literal value variable nodes are around and change their values
                    //foreach (NodeConnectionCouple ncc in inNodes)
                    //{
                    //    if (ncc.n.type == NodeType.VARIABLE)
                    //        ncc.n.vN.selectedVarTypeIndex = eN.selectedVarTypeIndex;
                    //}

                    oldEditVariableValue = eN.selectedVarTypeIndex;
                }
                eN.selectedEquationTypeIndex = EditorGUI.Popup(new Rect(rect.x + 85, rect.y + (labelPosition.y * 2) + 20, rect.width - 100, 20), eN.selectedEquationTypeIndex, editEquationList);

                EditorGUI.LabelField(new Rect(rect.x + 10, inPoint[1].rect.y, rect.width - 20, 20), "A");
                EditorGUI.LabelField(new Rect(rect.x + 10, inPoint[2].rect.y, rect.width - 20, 20), "B");

                if (!eN.hasA)
                {
                    switch (eN.selectedVarTypeIndex)
                    {
                        case 0: //Bool
                            eN.aValue.defaultBValue = EditorGUI.Toggle(new Rect(rect.x + 25, inPoint[1].rect.y, 20, 20), eN.aValue.defaultBValue);
                            break;

                        case 1: //Int
                            eN.aValue.defaultIValue = EditorGUI.IntField(new Rect(rect.x + 25, inPoint[1].rect.y, rect.width - 85, 20), eN.aValue.defaultIValue);
                            break;

                        case 2: //Float
                            eN.aValue.defaultFValue = EditorGUI.FloatField(new Rect(rect.x + 25, inPoint[1].rect.y, rect.width - 85, 20), eN.aValue.defaultFValue);
                            break;

                        case 3: //String
                            eN.aValue.defaultSValue = EditorGUI.TextField(new Rect(rect.x + 25, inPoint[1].rect.y, rect.width - 85, 20), eN.aValue.defaultSValue);
                            break;
                    }
                }

                if (!eN.hasB)
                {
                    switch (eN.selectedVarTypeIndex)
                    {
                        case 0: //Bool
                            eN.bValue.defaultBValue = EditorGUI.Toggle(new Rect(rect.x + 25, inPoint[2].rect.y, 20, 20), eN.bValue.defaultBValue);
                            break;

                        case 1: //Int
                            eN.bValue.defaultIValue = EditorGUI.IntField(new Rect(rect.x + 25, inPoint[2].rect.y, rect.width - 85, 20), eN.bValue.defaultIValue);
                            break;

                        case 2: //Float
                            eN.bValue.defaultFValue = EditorGUI.FloatField(new Rect(rect.x + 25, inPoint[2].rect.y, rect.width - 85, 20), eN.bValue.defaultFValue);
                            break;

                        case 3: //String
                            eN.bValue.defaultSValue = EditorGUI.TextField(new Rect(rect.x + 25, inPoint[2].rect.y, rect.width - 85, 20), eN.bValue.defaultSValue);
                            break;
                    }
                }

                break;

            case NodeType.COMMENT:
                if (commentNodeHeightAdjustment > rect.height - (labelPosition.y * 3))
                    cN.comment = EditorGUI.TextField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2), rect.width - 20, rect.height + commentNodeHeightAdjustment - (labelPosition.y * 3) - 40), cN.comment);
                else
                    cN.comment = EditorGUI.TextField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2), rect.width - 20, rect.height - (labelPosition.y * 3)), cN.comment);
                break;

            case NodeType.NEW_SPEAKER:
                EditorGUI.BeginChangeCheck();
                string tempNewSpeakerName;

                EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2), rect.width - 20, 20), "Speaker Name:");
                tempNewSpeakerName = EditorGUI.TextField(new Rect(rect.x + 110, rect.y + (labelPosition.y * 2), rect.width - 130, 20), nN.speakerName);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(branchingTreeObj, "New Speaker name change");
                    nN.speakerName = tempNewSpeakerName;
                }

                //EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2) + 25, rect.width - 20, 20), "Speaker Obj:");
                //nN.speakerObj = (GameObject)EditorGUI.ObjectField(new Rect(rect.x + 110, rect.y + (labelPosition.y * 2) + 25, rect.width - 120, 20), "", nN.speakerObj, typeof(GameObject), true);

                EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2) + 25, rect.width - 20, 20), "Speaker Sprite:");
                nN.speakerSprite = (Sprite)EditorGUI.ObjectField(new Rect(rect.x + 110, rect.y + 55, 64, 64), "", nN.speakerSprite, typeof(Sprite), true);
                
                break;

            case NodeType.START_CHANGE:
                EditorGUI.BeginChangeCheck();
                int tempStartChangeID;

                EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2), rect.width - 20, 20), "New Start ID:");
                tempStartChangeID = EditorGUI.IntField(new Rect(rect.x + 95, rect.y + (labelPosition.y * 2), rect.width - 105, rect.height - (labelPosition.y * 3)), scN.startID);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(branchingTreeObj, "Start Change ID change");
                    scN.startID = tempStartChangeID;
                }
                break;

            case NodeType.TELEPORT_FLOW:
                EditorGUI.BeginChangeCheck();
                int tempTeleportID;

                EditorGUI.LabelField(new Rect(rect.x + 10, rect.y + (labelPosition.y * 2), rect.width - 20, 20), "Teleport ID:");
                tempTeleportID = EditorGUI.IntField(new Rect(rect.x + 95, rect.y + (labelPosition.y * 2), rect.width - 105, rect.height - (labelPosition.y * 3)), tfN.teleportID);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(branchingTreeObj, "Teleport ID change");
                    tfN.teleportID = tempTeleportID;
                }
                break;
        }
    }

    //Process any events that are node specific
    //Returns a bool to check for Repaint in NodeEditor
    public bool ProcessEvents(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0)
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        beingDragged = true;
                        GUI.changed = true;
                        isSelected = true;
                        style = selectedNodeStyle;
                    }
                    else
                    {
                        //Check to make sure expandable nodes are also checked since they aren't in the full rect calculation
                        if (type == NodeType.DIALOGUE)
                        {
                            int advancedOptionsOffset = dN.advancedOptions ? 140 : 15;
                            Rect tempRect;
                            if (dialogueNodeHeightAdjustment > rect.height - (labelPosition.y * 3))
                                tempRect = new Rect(rect.x, rect.y, rect.width, rect.height + dialogueNodeHeightAdjustment - 40 + advancedOptionsOffset);
                            else
                                tempRect = new Rect(rect.x, rect.y, rect.width, rect.height + advancedOptionsOffset);

                            if (tempRect.Contains(e.mousePosition))
                            {
                                beingDragged = true;
                                GUI.changed = true;
                                isSelected = true;
                                style = selectedNodeStyle;
                            }
                            else
                            {
                                GUI.changed = true;
                                isSelected = false;
                                style = defaultNodeStyle;
                            }
                        }
                        else if (type == NodeType.COMMENT)
                        {
                            Rect tempRect;
                            if (commentNodeHeightAdjustment > rect.height - (labelPosition.y * 3))
                                tempRect = new Rect(rect.x, rect.y, rect.width, rect.height + commentNodeHeightAdjustment - 40);
                            else
                                tempRect = new Rect(rect.x, rect.y, rect.width, rect.height);

                            if (tempRect.Contains(e.mousePosition))
                            {
                                beingDragged = true;
                                GUI.changed = true;
                                isSelected = true;
                                style = selectedNodeStyle;
                            }
                            else
                            {
                                GUI.changed = true;
                                isSelected = false;
                                style = defaultNodeStyle;
                            }
                        }
                        else if (type == NodeType.BRANCH)
                        {
                            Rect tempRect;
                            if (bN.inPins >= bN.outPins)
                                branchNodeHeightAdjustment = (bN.inPins - 3) * 30;
                            else
                                branchNodeHeightAdjustment = (bN.outPins - 3) * 30;

                            tempRect = new Rect(rect.x, rect.y, rect.width, rect.height + branchNodeHeightAdjustment);

                            if (tempRect.Contains(e.mousePosition))
                            {
                                beingDragged = true;
                                GUI.changed = true;
                                isSelected = true;
                                style = selectedNodeStyle;
                            }
                            else
                            {
                                GUI.changed = true;
                                isSelected = false;
                                style = defaultNodeStyle;
                            }
                        }
                        else
                        {
                            GUI.changed = true;
                            isSelected = false;
                            style = defaultNodeStyle;
                        }
                    }
                }

                if (e.button == 1 && isSelected)
                {
                    if (rect.Contains(e.mousePosition))
                    {
                        ProcessContextMenu();
                        e.Use();
                    }

                    //Check to make sure expandable nodes are also checked since they aren't in the full rect calculation
                    else if (type == NodeType.DIALOGUE)
                    {
                        int advancedOptionsOffset = dN.advancedOptions ? 140 : 15;
                        Rect tempRect;
                        if (dialogueNodeHeightAdjustment > rect.height - (labelPosition.y * 3))
                            tempRect = new Rect(rect.x, rect.y, rect.width, rect.height + dialogueNodeHeightAdjustment - 40 + advancedOptionsOffset);
                        else
                            tempRect = new Rect(rect.x, rect.y, rect.width, rect.height + advancedOptionsOffset);

                        if (tempRect.Contains(e.mousePosition))
                        {
                            ProcessContextMenu();
                            e.Use();
                        }
                    }
                    else if (type == NodeType.COMMENT)
                    {
                        Rect tempRect;
                        if (commentNodeHeightAdjustment > rect.height - (labelPosition.y * 3))
                            tempRect = new Rect(rect.x, rect.y, rect.width, rect.height + commentNodeHeightAdjustment - 40);
                        else
                            tempRect = new Rect(rect.x, rect.y, rect.width, rect.height);

                        if (tempRect.Contains(e.mousePosition))
                        {
                            ProcessContextMenu();
                            e.Use();
                        }
                    }
                    else if (type == NodeType.BRANCH)
                    {
                        Rect tempRect;
                        if (bN.inPins >= bN.outPins)
                            branchNodeHeightAdjustment = (bN.inPins - 3) * 30;
                        else
                            branchNodeHeightAdjustment = (bN.outPins - 3) * 30;

                        tempRect = new Rect(rect.x, rect.y, rect.width, rect.height + branchNodeHeightAdjustment);

                        if (tempRect.Contains(e.mousePosition))
                        {
                            ProcessContextMenu();
                            e.Use();
                        }
                    }
                }
                break;

            case EventType.MouseUp:
                beingDragged = false;
                break;

            case EventType.MouseDrag:
                if (e.button == 0 && beingDragged)
                {
                    Drag(e.delta);
                    e.Use();
                    return true;
                }
                break;
        }

        return false;
    }

    //Create a context menu on the node
    private void ProcessContextMenu()
    {
        if (type == NodeType.INFO)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddDisabledItem(new GUIContent("Cannot remove or duplicate info node"));
            genericMenu.ShowAsContext();
        }
        else if (type == NodeType.BRANCH)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Add another choice"), false, () => BranchChoice(true));
            if (bN.inPins > 3)
                genericMenu.AddItem(new GUIContent("Remove choice"), false, () => BranchChoice(false));
            else
                genericMenu.AddDisabledItem(new GUIContent("Cannot remove choice - 2 choices required"));

            //Old Branch set up, keeping in case of Branch workflow revert
            //genericMenu.AddItem(new GUIContent("Add in pin"), false, () => BranchInPin(true));
            ////~~~
            //if (bN.outPins < bN.inPins - 1)
            //    genericMenu.AddItem(new GUIContent("Add out pin"), false, () => BranchOutPin(true));            
            //else
            //    genericMenu.AddDisabledItem(new GUIContent("Cannot add out pin - add another in pin"));
            ////~~~
            //if (bN.inPins > 3)
            //    genericMenu.AddItem(new GUIContent("Remove in pin"), false, () => BranchInPin(false));
            //else
            //    genericMenu.AddDisabledItem(new GUIContent("Cannot remove in pin - 3 in pins required"));
            ////~~~
            //if (bN.outPins > 2)
            //    genericMenu.AddItem(new GUIContent("Remove out pin"), false, () => BranchOutPin(false));
            //else
            //    genericMenu.AddDisabledItem(new GUIContent("Cannot remove out pin - 2 out pins required"));
            ////~~~

            genericMenu.AddItem(new GUIContent("Duplicate node"), false, OnClickDuplicateNode);
            genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
            genericMenu.ShowAsContext();
        }
        else
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Duplicate node"), false, OnClickDuplicateNode);
            genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);

            genericMenu.ShowAsContext();
        }
    }

    //Remove node
    private void OnClickRemoveNode()
    {
        if (OnRemoveNode != null)
        {
            //Check if logic node and if so remove HasA/HasB
            foreach (NodeConnectionCouple ncc in outNodes)
            {
                if (ncc.n.type == NodeType.LOGIC)
                {
                    foreach (NodeConnectionCouple ncc2 in ncc.n.inNodes)
                    {
                        if (ncc2.n.ID == ID) //If we're looking back at ourseleves
                        {
                            if (ncc2.index == 1)
                                ncc.n.lN.hasA = false;
                            else if (ncc2.index == 2)
                                ncc.n.lN.hasB = false;

                            break;
                        }
                    }
                }
            }

            OnRemoveNode(this);
        }
    }

    private void OnClickDuplicateNode()
    {
        if (OnRemoveNode != null)        
            OnDuplicateNode(this);
    }

    //Keep if want to give more control later on...might break reader though
    private void BranchInPin(bool isAdd)
    {
        if (isAdd)
        {
            bN.inPins++;
            inPoint.Add(new ConnectionPoint(this, ConnectionPointType.In, inStyle, inClickAction));
        }
        else
        {
            if (bN.inPins > 3)
            {
                bN.inPins--;
                inPoint.RemoveAt(inPoint.Count - 1);
            }
        }
    }

    //Keep if want to give more control later on...might break reader though
    private void BranchOutPin(bool isAdd)
    {
        if (isAdd)
        {
            bN.outPins++;
            outPoint.Add(new ConnectionPoint(this, ConnectionPointType.Out, outStyle, outClickAction));
        }
        else
        {
            if (bN.outPins > 2)
            {
                bN.outPins--;
                outPoint.RemoveAt(outPoint.Count - 1);
            }
        }
    }

    private void BranchChoice(bool isAdd)
    {
        if (isAdd)
        {
            bN.inPins++;
            bN.outPins++;
            inPoint.Add(new ConnectionPoint(this, ConnectionPointType.In, inStyle, inClickAction));
            outPoint.Add(new ConnectionPoint(this, ConnectionPointType.Out, outStyle, outClickAction));
        }
        else
        {
            bN.inPins--;
            bN.outPins--;
            outPoint.RemoveAt(outPoint.Count - 1);
            inPoint.RemoveAt(inPoint.Count - 1);
        }
    }

    public int GetNodeType()
    {
        return (int) type;
    }

    public Vector2 GetPosition()
    {
        return position;
    }

    public List<NodeConnectionCouple> GetInNodes()
    {
        return inNodes;
    }

    public List<NodeConnectionCouple> GetOutNodes()
    {
        return outNodes;
    }
}
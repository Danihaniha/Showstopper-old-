//Code by Zach Phillips for the EZPZ Branching Dialogue toolset
//Used tutorial for basic window set up: https://gram.gs/gramlog/creating-node-based-editor-unity/
//All other code is copyright 2020, Zach Phillips, All rights reserved
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.MemoryProfiler;
using System.Runtime.InteropServices;
using UnityEngine.Analytics;

[InitializeOnLoad]
public class NodeEditor : EditorWindow {

    List<Node> nodes;

    List<ConnectionLine> connections;

    GUIStyle inPointStyle;
    GUIStyle outPointStyle;

    GUIStyle noObjSelectedStyle;

    ConnectionPoint selectedInPoint;
    ConnectionPoint selectedOutPoint;

    string hoverOverHelpText = "";
    bool hoverOverHelpNeeded = false;
    Rect hoverOverHelpRect;

    Vector2 offset;
    Vector2 drag;
    Vector2 totalDragOffset = Vector2.zero;

    //Used for a Undo/Redo test, keeping in case I need to reset
    //int nodeCount = 1;
    //int connectionCount = 0;

    Vector2 currMousePos = Vector2.zero;
    public DialogueTreeScriptableObj branchingTreeObj;
    public DialogueTreeScriptableObj backUpObjCheck;

    bool fromNullToFound = false;
    bool justInitWindow = false;

    bool draggingLine = false;

    public static float zoomScale = 1f;

    static bool loaded = false;

    //Constructor for use on editor load
    static NodeEditor()
    {
        loaded = true;
    }

    //Open window or select current open window
    [MenuItem("Window/EZPZ Branching Dialogue Node Editor")]
    static void Init()
    {
        //Warning if I don't use this to instantiate 
        NodeEditor _nodeEdit = ScriptableObject.CreateInstance<NodeEditor>();

        //Old method, got warning
        //NodeEditor _nodeEdit = new NodeEditor();
        //instance = _nodeEdit;
        _nodeEdit.CreateWindowAndLoad();
    }   

    void CreateWindowAndLoad()
    {
        NodeEditor window = (NodeEditor)EditorWindow.GetWindow(typeof(NodeEditor));
        window.titleContent.text = "Node Editor";
        branchingTreeObj = null;
        justInitWindow = true;
        window.Show();
    }

    void OnLostFocus()
    {
        if (branchingTreeObj != null)
            SaveTree();
    }

    void OnDestroy()
    {
        if (branchingTreeObj != null)
            SaveTree();
    }

    //Create the styles
    private void OnEnable()
    {
        //Set the style for the "in" connection point
        inPointStyle = new GUIStyle();
        inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
        inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
        inPointStyle.border = new RectOffset(4, 4, 12, 12);

        //Set the style for the "out" connection point
        outPointStyle = new GUIStyle();
        outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
        outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
        outPointStyle.border = new RectOffset(4, 4, 12, 12);

        //Set style for when nothing is selected
        noObjSelectedStyle = new GUIStyle();
        noObjSelectedStyle.fontSize = 50;
        noObjSelectedStyle.alignment = TextAnchor.MiddleCenter;

        Undo.undoRedoPerformed += UndoCallback;
        totalDragOffset = Vector2.zero;
    }

    void UndoCallback()
    {
        //Check to make sure it's a node/connections change. This way we don't do any unnecessary loading
        //if (nodes.Count != nodeCount || connections.Count != connectionCount)
        //{
        if (branchingTreeObj != null)
        {
            LoadTreeData();

            OnDrag(totalDragOffset);
            offset -= totalDragOffset;

            Repaint();
        }
        //}
    }

    //Displays all the information to the window
    void OnGUI()
    {
        if (justInitWindow || loaded)
        {
            branchingTreeObj = null;
            justInitWindow = false;
            loaded = false;
        }

        if (branchingTreeObj != null && fromNullToFound)
            LoadTreeData();

        if (branchingTreeObj != null)
        {
            fromNullToFound = false;

            //Draw background
            EditorGUI.DrawRect(new Rect(new Vector2(0, 0), new Vector2(position.width, position.height)), new Color(.2f, .2f, .2f));

            DrawGrid(20 * zoomScale, 0.2f, Color.gray);
            DrawGrid(100 * zoomScale, 0.4f, Color.gray);

            DrawNodes();
            DrawConnections();

            DrawConnectionLine(Event.current);

            ProcessNodeEvents(Event.current);
            ProcessEvents(Event.current);

            if (hoverOverHelpNeeded)
            {
                GUI.Box(new Rect(hoverOverHelpRect.x, hoverOverHelpRect.y, 200, GUI.skin.box.CalcHeight(new GUIContent(hoverOverHelpText), 200)), hoverOverHelpText);
            }
        }
        else
        {
            fromNullToFound = true;

            DropAreaGUI();
        }

        if (!fromNullToFound)
        {
            EditorGUI.BeginChangeCheck();
            branchingTreeObj = (DialogueTreeScriptableObj)EditorGUILayout.ObjectField(branchingTreeObj, typeof(DialogueTreeScriptableObj), true);
            if (EditorGUI.EndChangeCheck())
            {
                if (branchingTreeObj != null)
                {
                    LoadTreeData();
                    totalDragOffset = Vector2.zero;
                }
            }
        }

        if (GUI.changed)
            Repaint();
    }

    //Thanks to bzgeb for the DropArea code
    //https://gist.github.com/bzgeb/3800350
    public void DropAreaGUI()
    {
        Event evt = Event.current;
        Rect drop_area = GUILayoutUtility.GetRect(0.0f, 0.0f, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        GUI.Box(drop_area, "");
        GUI.Label(new Rect(new Vector2(0, 0), new Vector2(position.width, position.height)), "Please Drag in an \nEZPZ Dialogue Tree To Edit", noObjSelectedStyle);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!drop_area.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object dragged_object in DragAndDrop.objectReferences)
                    {
                        if (dragged_object is DialogueTreeScriptableObj)
                        {
                            branchingTreeObj = (DialogueTreeScriptableObj)dragged_object;
                            break;
                        }
                    }
                }
                break;
        }
    }

    //Draw the background grid
    private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
    {
        int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
        int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

        Handles.BeginGUI();
        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        offset += drag * .5f;
        Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

        for (int i = 0; i <= widthDivs; i++)
        {
            Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height  + gridSpacing, 0f) + newOffset);
        }

        for (int j = 0; j <= heightDivs; j++)
        {
            Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width + gridSpacing, gridSpacing * j, 0f) + newOffset);
        }

        Handles.color = Color.white;
        Handles.EndGUI();
    }

    //Draws all nodes in the nodes list
    void DrawNodes()
    {
        bool isHoverReady = false; 

        if (nodes != null)
        {
            int i = 0;
            foreach (Node n in nodes)
            {
                n.Draw(i, zoomScale);
                if (n.helpHoveredOver)
                {
                    isHoverReady = true;
                    hoverOverHelpNeeded = true;
                    hoverOverHelpRect = n.helpRect;
                    hoverOverHelpText = n.myHelpInfo;
                }
                i++;
            }
        }

        if (!isHoverReady)
            hoverOverHelpNeeded = false;
    }

    //Draw the connection lines between nodes
    private void DrawConnections()
    {
        if (connections != null)
        {
            for (int i = 0; i < connections.Count; i++)
            {
                connections[i].Draw();
            }
        }
    }

    //Draw connection line from point to mouse
    private void DrawConnectionLine(Event e)
    {
        if (selectedInPoint != null && selectedOutPoint == null)
        {
            Handles.DrawBezier(
                selectedInPoint.rect.center,
                e.mousePosition,
                selectedInPoint.rect.center + Vector2.left * 50f,
                e.mousePosition - Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;

            draggingLine = true;
        }

        if (selectedOutPoint != null && selectedInPoint == null)
        {
            Handles.DrawBezier(
                selectedOutPoint.rect.center,
                e.mousePosition,
                selectedOutPoint.rect.center - Vector2.left * 50f,
                e.mousePosition + Vector2.left * 50f,
                Color.white,
                null,
                2f
            );

            GUI.changed = true;

            draggingLine = true;
        }
    }

    //Save the data to a Scriptable Object
    //ADD NEW NODES HERE
    void SaveTree()
    {
        if (nodes != null && branchingTreeObj != null)
        {

            int i = 0;
            int nodeAmount = nodes.Count;

            List<int> index = new List<int>();
            while (i < nodeAmount && nodeAmount > 0)
            {
                index.Add(i);
                i++;
            }

            i = 0;

            List<int> type = new List<int>();
            List<InfoNode> iNodesAndID = new List<InfoNode>();
            List<StartNode> sNodesAndID = new List<StartNode>();
            List<DialogueNode> dNodesAndID = new List<DialogueNode>();
            List<BranchNode> bNodesAndID = new List<BranchNode>();
            List<VariableNode> vNodesAndID = new List<VariableNode>();
            List<LogicNode> lNodesAndID = new List<LogicNode>();
            List<EditVariableNode> eNodesAndID = new List<EditVariableNode>();
            List<CommentNode> cNodesAndID = new List<CommentNode>();
            List<NewSpeakerNode> nNodesAndID = new List<NewSpeakerNode>();
            List<StartChangeNode> scNodesAndID = new List<StartChangeNode>();
            List<TeleportFlowNode> tfNodesAndID = new List<TeleportFlowNode>();

            while (i < nodeAmount && nodeAmount > 0)
            {
                int t = nodes[i].GetNodeType();

                type.Add(t);

                iNodesAndID.Add(nodes[i].iN);
                sNodesAndID.Add(nodes[i].sN);
                dNodesAndID.Add(nodes[i].dN);
                bNodesAndID.Add(nodes[i].bN);
                vNodesAndID.Add(nodes[i].vN);
                lNodesAndID.Add(nodes[i].lN);
                eNodesAndID.Add(nodes[i].eN);
                cNodesAndID.Add(nodes[i].cN);
                nNodesAndID.Add(nodes[i].nN);
                scNodesAndID.Add(nodes[i].scN);
                tfNodesAndID.Add(nodes[i].tfN);

                i++;
            }

            i = 0;

            List<Vector2> inOutPoints = new List<Vector2>();
            while (i < nodeAmount && nodeAmount > 0)
            {
                if (nodes[i].GetNodeType() != 4)
                    inOutPoints.Add(new Vector2(nodes[i].inPoint.Count, nodes[i].outPoint.Count));
                else
                    inOutPoints.Add(new Vector2(nodes[i].bN.inPins, nodes[i].bN.outPins));
                i++;
            }

            i = 0;

            List<Vector2> pos = new List<Vector2>();
            while (i < nodeAmount && nodeAmount > 0)
            {
                pos.Add(nodes[i].GetPosition());
                i++;
            }

            i = 0;

            List<List<DialogueTreeScriptableObj.InOutConnections>> inNodes = new List<List<DialogueTreeScriptableObj.InOutConnections>>();
            List<List<DialogueTreeScriptableObj.InOutConnections>> outNodes = new List<List<DialogueTreeScriptableObj.InOutConnections>>();
            while (i < nodeAmount && nodeAmount > 0)
            {
                List<NodeConnectionCouple> listOfInNodes = nodes[i].GetInNodes();
                List<NodeConnectionCouple> listOfOutNodes = nodes[i].GetOutNodes();

                List<DialogueTreeScriptableObj.InOutConnections> tempInIntList = new List<DialogueTreeScriptableObj.InOutConnections>();
                inNodes.Add(tempInIntList);
                
                List<DialogueTreeScriptableObj.InOutConnections> tempOutIntList = new List<DialogueTreeScriptableObj.InOutConnections>();
                outNodes.Add(tempOutIntList);

                if (listOfInNodes.Count > 0)
                {
                    foreach (NodeConnectionCouple ncc in listOfInNodes)
                    {
                        int j = 0;
                        while (j < nodeAmount)
                        {
                            if (ncc.n == nodes[j])
                            {
                                DialogueTreeScriptableObj.InOutConnections nccfs;
                                nccfs.nodeIndex = j;
                                nccfs.connectionIndex = ncc.index;
                                inNodes[i].Add(nccfs);
                            }
                            j++;
                        }
                    }
                }

                if (listOfOutNodes.Count > 0)
                {
                    foreach (NodeConnectionCouple ncc in listOfOutNodes)
                    {
                        int j = 0;
                        while (j < nodeAmount)
                        {
                            if (ncc.n == nodes[j])
                            {
                                DialogueTreeScriptableObj.InOutConnections nccfs;
                                nccfs.nodeIndex = j;
                                nccfs.connectionIndex = ncc.index;
                                outNodes[i].Add(nccfs);
                            }
                            j++;
                        }
                    }
                }
                i++;
            }

            inNodes = RemoveConnectionDuplicates(inNodes);
            outNodes = RemoveConnectionDuplicates(outNodes);

            branchingTreeObj.SaveNodeData(index, type, inOutPoints, pos, inNodes, outNodes, iNodesAndID, sNodesAndID, dNodesAndID, bNodesAndID, vNodesAndID, lNodesAndID, eNodesAndID, cNodesAndID, nNodesAndID, scNodesAndID, tfNodesAndID);
        }

        EditorUtility.SetDirty(branchingTreeObj);
    }
    //ADD NEW NODES HERE
    void LoadTreeData()
    {
        zoomScale = 1f;

        List<DialogueTreeScriptableObj.NodeData> loadData = new List<DialogueTreeScriptableObj.NodeData>();

        //If there aren't any nodes, init list
        if (nodes == null)
            nodes = new List<Node>();

        nodes.Clear();

        //Create connections
        if (connections == null)
            connections = new List<ConnectionLine>();

        connections.Clear();

        //For each node saved in the tree obj, add it to a temp list
        for (int j = 0; j < branchingTreeObj.GetNodesFromTree().Count; j++)
        {
            DialogueTreeScriptableObj.NodeData sd = new DialogueTreeScriptableObj.NodeData();

            sd.nodeIndex = branchingTreeObj.GetNodesFromTree()[j].nodeIndex;
            sd.enumType = branchingTreeObj.GetNodesFromTree()[j].enumType;
            sd.inPoints = branchingTreeObj.GetNodesFromTree()[j].inPoints;
            sd.outPoints = branchingTreeObj.GetNodesFromTree()[j].outPoints;
            sd.position = branchingTreeObj.GetNodesFromTree()[j].position;

            sd.inNodeIndexes = branchingTreeObj.GetNodesFromTree()[j].inNodeIndexes;
            sd.outNodeIndexes = branchingTreeObj.GetNodesFromTree()[j].outNodeIndexes;

            sd.iN = branchingTreeObj.GetNodesFromTree()[j].iN;
            sd.sN = branchingTreeObj.GetNodesFromTree()[j].sN;
            sd.dN = branchingTreeObj.GetNodesFromTree()[j].dN;
            sd.bN = branchingTreeObj.GetNodesFromTree()[j].bN;
            sd.vN = branchingTreeObj.GetNodesFromTree()[j].vN;
            sd.lN = branchingTreeObj.GetNodesFromTree()[j].lN;
            sd.eN = branchingTreeObj.GetNodesFromTree()[j].eN;
            sd.cN = branchingTreeObj.GetNodesFromTree()[j].cN;
            sd.nN = branchingTreeObj.GetNodesFromTree()[j].nN;
            sd.scN = branchingTreeObj.GetNodesFromTree()[j].scN;
            sd.tfN = branchingTreeObj.GetNodesFromTree()[j].tfN;

            loadData.Add(sd);
        }

        //If there is at least one node in the temp load list, then add them to node list
        if (loadData.Count > 0)
        {
            //counter delta offsets
            Vector2 initPos = new Vector2(loadData[0].position.x, loadData[0].position.y);

            //Create nodes
            for (int i = 0; i < loadData.Count; i++)
            {
                //Add node to list with values
                nodes.Add(new Node(branchingTreeObj, loadData[i].enumType, loadData[i].position - initPos, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickDuplicateNode));

                switch (loadData[i].enumType)
                {
                    case 0: //Info
                        nodes[i].iN = loadData[i].iN;
                        break;

                    case 1: //Start
                        nodes[i].sN = loadData[i].sN;
                        break;

                    case 2: //End
                        //End has no data
                        break;

                    case 3: //Dialogue
                        nodes[i].dN = loadData[i].dN;
                        break;

                    case 4: //Branch
                        nodes[i].bN = loadData[i].bN;
                        break;

                    case 5: //Variable
                        nodes[i].vN = loadData[i].vN;
                        break;

                    case 6: //Logic
                        nodes[i].lN = loadData[i].lN;
                        break;

                    case 7: //Edit Variable
                        nodes[i].eN = loadData[i].eN;
                        break;

                    case 8: //Comment
                        nodes[i].cN = loadData[i].cN;
                        break;

                    case 9: //New Speaker
                        nodes[i].nN = loadData[i].nN;
                        break;

                    case 10: //Start Change
                        nodes[i].scN = loadData[i].scN;
                        break;

                    case 11: //Teleport Flow
                        nodes[i].tfN = loadData[i].tfN;
                        break;
                }

                if (nodes[i].GetNodeType() == 4)
                    nodes[i].SetInOutPointsExplicit(new Vector2(nodes[i].bN.inPins, nodes[i].bN.outPins));
            }

            if (nodes.Count > 1)
            {
                //Add all the inNodes and outNodes
                for (int i = 0; i < nodes.Count; i++)
                {
                    //Clear just in case
                    nodes[i].inNodes.Clear();
                    nodes[i].outNodes.Clear();

                    //Add in nodes
                    foreach (DialogueTreeScriptableObj.InOutConnections index in loadData[i].inNodeIndexes)
                    {
                        NodeConnectionCouple ncc;
                        ncc.n = nodes[index.nodeIndex];
                        ncc.index = index.connectionIndex;
                        nodes[i].inNodes.Add(ncc);
                    }

                    //Add out nodes
                    foreach (DialogueTreeScriptableObj.InOutConnections index in loadData[i].outNodeIndexes)
                    {
                        NodeConnectionCouple ncc;
                        ncc.n = nodes[index.nodeIndex];
                        ncc.index = index.connectionIndex;
                        nodes[i].outNodes.Add(ncc);
                    }
                }

                //Go through list again, this time making connections
                for (int i = 0; i < nodes.Count; i++)
                {
                    //Add connections
                    int inIndex = 0;
                    foreach (NodeConnectionCouple ncc in nodes[i].outNodes)
                    {
                        selectedInPoint = ncc.n.inPoint[0];

                        for (int j = 0; j < ncc.n.inNodes.Count; j++)
                        {
                            if (ncc.n.inNodes[j].n == nodes[i])
                            {
                                selectedInPoint = ncc.n.inPoint[ncc.n.inNodes[j].index];

                                //Set the new inIndex
                                inIndex = ncc.n.inNodes[j].index;

                                break;
                            }
                        }

                        selectedOutPoint = nodes[i].outPoint[ncc.index];

                        CreateConnectionForLoad(ncc.n, nodes[i], inIndex);
                    }

                    ClearConnectionSelection();
                }
            }
        }

        //nodeCount = nodes.Count;
        //connectionCount = connections.Count;
    }


    //Process mouse events
    void ProcessEvents (Event e)
    {
        drag = Vector2.zero;

        switch (e.type)
        {
            case EventType.MouseDown:
                //Record mouse position
                currMousePos = e.mousePosition;

                //Left Click down
                if (e.button == 0)
                {
                    if (draggingLine)
                        ClearConnectionSelection();
                }

                //Right click down
                if (e.button == 1)
                    ProcessContextMenu();
                break;

            case EventType.MouseDrag:
                if (e.button == 0)
                {
                    OnDrag(e.delta);
                    totalDragOffset += e.delta;
                }
                break;

                //Future feature, not finished but need to push to fix some bugs
           //case EventType.ScrollWheel:
           //    {
           //        bool zoomed = false;
           //
           //        //Zoom out
           //        if (e.delta.y > 0 && zoomScale > .6f)
           //        {
           //            zoomScale -= .25f;
           //            zoomed = true;
           //
           //            //Move the view to orient with the position the user zoomed on
           //            OnDrag(new Vector2((e.mousePosition.x - position.width / 2) * .1f * zoomScale, (e.mousePosition.y - position.height / 2) * .1f * zoomScale));
           //        }
           //
           //        //Zoom in
           //        else if (e.delta.y < 0 && zoomScale < 2.1f)
           //        { 
           //            zoomScale += .25f; 
           //            zoomed = true;
           //
           //            //Move the view to orient with the position the user zoomed on
           //            OnDrag(new Vector2((e.mousePosition.x - position.width / 2) * -.1f * zoomScale, (e.mousePosition.y - position.height / 2) * -.1f * zoomScale));
           //        }
           //
           //        if (zoomed)
           //        {
           //            Repaint();
           //        }
           //    }
                break;
        }
    }
    //Allow the canvas to be dragged
    private void OnDrag(Vector2 delta)
    {
        drag = delta;

        if (nodes != null)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Drag(delta);
            }
        }

        GUI.changed = true;
    }

    //Create a context menu for easy access to features
    //ADD NEW NODES HERE
    void ProcessContextMenu()
    {
        //https://docs.unity3d.com/2017.1/Documentation/ScriptReference/GenericMenu.html
        //Make the menu
        GenericMenu contextMenu = new GenericMenu();

        //Add menu items
        contextMenu.AddItem(new GUIContent("Dialogue node"), false, () => OnClickAddNewNode(3));
        contextMenu.AddItem(new GUIContent("Branch node"), false, () => OnClickAddNewNode(4));
        contextMenu.AddItem(new GUIContent("New Speaker node"), false, () => OnClickAddNewNode(9)); 
        contextMenu.AddSeparator("");
        contextMenu.AddItem(new GUIContent("Start node"), false, () => OnClickAddNewNode(1));
        contextMenu.AddItem(new GUIContent("Start Change node"), false, () => OnClickAddNewNode(10));
        contextMenu.AddItem(new GUIContent("End node"), false, () => OnClickAddNewNode(2));
        contextMenu.AddSeparator("");
        contextMenu.AddItem(new GUIContent("Local Variable node"), false, () => OnClickAddVarNode(true));
        contextMenu.AddItem(new GUIContent("Global Variable node"), false, () => OnClickAddVarNode(false));
        contextMenu.AddItem(new GUIContent("Edit Variable node"), false, () => OnClickAddNewNode(7));
        contextMenu.AddItem(new GUIContent("Logic node"), false, () => OnClickAddNewNode(6));        
        contextMenu.AddSeparator("");
        contextMenu.AddItem(new GUIContent("Teleport Flow In node"), false, () => OnClickAddTeleportNode(true));
        contextMenu.AddItem(new GUIContent("Teleport Flow Out node"), false, () => OnClickAddTeleportNode(false));
        contextMenu.AddSeparator("");
        contextMenu.AddItem(new GUIContent("Comment node"), false, () => OnClickAddNewNode(8));

        //Display window
        contextMenu.ShowAsContext();
    }

    public void OnClickAddNewNode(int index)
    {
        //Save for undoing
        Undo.RecordObject(branchingTreeObj, "Add " + System.Enum.GetName(typeof(Node.NodeType), index) + " node");

        //If there aren't any nodes, init list
        if (nodes == null)
            nodes = new List<Node>();

        //Add node to list with values
        nodes.Add(new Node(branchingTreeObj, index, currMousePos, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickDuplicateNode));

        if (draggingLine && selectedOutPoint != null && nodes[nodes.Count - 1].inPoint.Count > 0)
        {
            if (nodes[nodes.Count - 1].inPoint.Count > 0)
                OnClickInPoint(nodes[nodes.Count - 1].inPoint[0]);
            else
                ClearConnectionSelection();
        }
        else
            ClearConnectionSelection();

        //nodeCount = nodes.Count;

        //Save for undoing
        SaveTree();
    }

    public void OnClickAddVarNode(bool isLocal)
    {
        //Save for undoing
        if (isLocal)
            Undo.RecordObject(branchingTreeObj, "Add LOCAL_VARIABLE node");
        else
            Undo.RecordObject(branchingTreeObj, "Add GLOBAL_VARIABLE node");

        //If there aren't any nodes, init list
        if (nodes == null)
            nodes = new List<Node>();

        //Add node to list with values
        nodes.Add(new Node(branchingTreeObj, 5, currMousePos, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickDuplicateNode));
        
        nodes[nodes.Count - 1].vN.isGlobal = !isLocal;
        nodes[nodes.Count - 1].myHelpInfo = isLocal ? nodes[nodes.Count - 1].vN.GetInfoString(true) : nodes[nodes.Count - 1].vN.GetInfoString(false);

        if (draggingLine && selectedOutPoint != null)
        {
            if (nodes[nodes.Count - 1].inPoint.Count > 0)
                OnClickInPoint(nodes[nodes.Count - 1].inPoint[0]);
            else
                ClearConnectionSelection();
        }
        else
            ClearConnectionSelection();

        //nodeCount = nodes.Count;

        //Save for undoing
        SaveTree();
    }

    public void OnClickAddTeleportNode(bool isIn)
    {
        //Save for undoing
        if (isIn)
            Undo.RecordObject(branchingTreeObj, "Add TELEPORT_FLOW_IN node");
        else
            Undo.RecordObject(branchingTreeObj, "Add TELEPORT_FLOW_OUT node");

        //If there aren't any nodes, init list
        if (nodes == null)
            nodes = new List<Node>();

        //Add node to list with values
        nodes.Add(new Node(branchingTreeObj, 11, currMousePos, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickDuplicateNode));

        nodes[nodes.Count - 1].tfN.isIn = isIn;

        if (draggingLine && isIn && selectedOutPoint != null)
        {
            if (nodes[nodes.Count - 1].inPoint.Count > 0)
                OnClickInPoint(nodes[nodes.Count - 1].inPoint[0]);
            else
                ClearConnectionSelection();
        }
        else
            ClearConnectionSelection();

        //nodeCount = nodes.Count;

        //Save for undoing
        SaveTree();
    }

    private void ProcessNodeEvents(Event e)
    {
        if (nodes != null)
        {
            //Go backwards through the list so last node added is processed first
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                bool guiChanged = nodes[i].ProcessEvents(e);

                if (guiChanged)
                {
                    GUI.changed = true;
                }
            }
        }
    }

    //Set current selected in point. If out point also selected then make connection
    private void OnClickInPoint(ConnectionPoint inPoint)
    {
        selectedInPoint = inPoint;

        if (selectedOutPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection(selectedInPoint.node, selectedOutPoint.node);
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    //Set current selected out point. If in point also selected then make connection
    private void OnClickOutPoint(ConnectionPoint outPoint)
    {
        selectedOutPoint = outPoint;

        if (selectedInPoint != null)
        {
            if (selectedOutPoint.node != selectedInPoint.node)
            {
                CreateConnection(selectedInPoint.node, selectedOutPoint.node);
                ClearConnectionSelection();
            }
            else
            {
                ClearConnectionSelection();
            }
        }
    }

    //Remove node
    private void OnClickRemoveNode(Node node)
    {
        //Save for undoing
        Undo.RecordObject(branchingTreeObj, "Remove node");

        //Clear all connections
        if (connections != null)
        {
            List<ConnectionLine> connectionsToRemove = new List<ConnectionLine>();

            for (int i = 0; i < connections.Count; i++)
            {
                for (int j = 0; j < node.inPoint.Count; j++)
                {
                    if (connections[i].inPoint == node.inPoint[j])
                    {
                        connectionsToRemove.Add(connections[i]);
                    }
                }
                for (int j = 0; j < node.outPoint.Count; j++)
                {
                    if (connections[i].outPoint == node.outPoint[j])
                    {
                        connectionsToRemove.Add(connections[i]);
                    }
                }
            }

            for (int i = 0; i < connectionsToRemove.Count; i++)
            {
                OnClickRemoveConnection(connectionsToRemove[i]);
                //connections.Remove(connectionsToRemove[i]);
            }

            connectionsToRemove = null;
        }

        //Remove self from all nodes that list this as a connection
        // self -> other
        //if (node.inNodes.Count > 0)
        //{
        //    foreach (NodeConnectionCouple ncc in node.outNodes)
        //    {
        //        ncc.n.inNodes.Remove(ncc);
        //    }
        //}
        //
        ////Remove self from all nodes that this node is connected to
        //// other -> self
        //if (node.outNodes.Count > 0)
        //{
        //    foreach (NodeConnectionCouple ncc in node.inNodes)
        //    {
        //        ncc.n.outNodes.Remove(ncc);
        //    }
        //}

        nodes.Remove(node);

        //nodeCount = nodes.Count;

        //Save for undoing
        SaveTree();
    }

    //Remove node
    private void OnClickDuplicateNode(Node node)
    {
        //Save for undoing
        Undo.RecordObject(branchingTreeObj, "Duplicate node");

        //Create and add new node
        nodes.Add(new Node(branchingTreeObj, node.GetNodeType(), currMousePos, inPointStyle, outPointStyle, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode, OnClickDuplicateNode));

        //Set data equal to the given node
        //ADD NEW NODES HERE
        nodes[nodes.Count - 1].iN = new InfoNode(node.iN);
        nodes[nodes.Count - 1].sN = new StartNode(node.sN);
        nodes[nodes.Count - 1].dN = new DialogueNode(node.dN);
        nodes[nodes.Count - 1].bN = new BranchNode(node.bN);
        nodes[nodes.Count - 1].vN = new VariableNode(node.vN);
        nodes[nodes.Count - 1].lN = new LogicNode(node.lN);
        nodes[nodes.Count - 1].eN = new EditVariableNode(node.eN);
        nodes[nodes.Count - 1].cN = new CommentNode(node.cN);
        nodes[nodes.Count - 1].nN = new NewSpeakerNode(node.nN);
        nodes[nodes.Count - 1].scN = new StartChangeNode(node.scN);
        nodes[nodes.Count - 1].tfN = new TeleportFlowNode(node.tfN);

        //If it's a variable node, set the help text correctly
        if (nodes[nodes.Count - 1].GetNodeType() == 5)
        {
            nodes[nodes.Count - 1].vN.isGlobal = node.vN.isGlobal;
            nodes[nodes.Count - 1].myHelpInfo = (!node.vN.isGlobal) ? nodes[nodes.Count - 1].vN.GetInfoString(true) : nodes[nodes.Count - 1].vN.GetInfoString(false);
        }

        //If it's a teleport flow node, set if it's in or out
        if (nodes[nodes.Count - 1].GetNodeType() == 11)
        {
            nodes[nodes.Count - 1].tfN.isIn = node.tfN.isIn;
        }

        //Clear connection selection in case a line was active
        ClearConnectionSelection();

        //Save for undoing
        SaveTree();
    }

    //Remove connection
    private void OnClickRemoveConnection(ConnectionLine connection)
    {
        //Save for undoing
        Undo.RecordObject(branchingTreeObj, "Remove connection");

        int tempIn = -1;
        int tempOut = -1;
        //Remove the connections between the nodes
        for (int i = 0; i < nodes.Count; i++)
        {
            if (connection.inNode == nodes[i])
                tempIn = i;
            else if (connection.outNode == nodes[i])
                tempOut = i;
        }

        NodeConnectionCouple tempNCC = new NodeConnectionCouple();

        //foreach inNode connection on the inNode, check to see if it's connected to the outNode
        //If so, remove that connection
        for(int i = 0; i < nodes[tempIn].inNodes.Count; i++)
        {
            if (nodes[tempIn].inNodes[i].n == connection.outNode)
            {
                tempNCC = nodes[tempIn].inNodes[i];
                break;
            }
        }

        nodes[tempIn].RemoveInNode(tempNCC);

        //foreach inNode connection on the inNode, check to see if it's connected to the outNode
        //If so, remove that connection
        for (int i = 0; i < nodes[tempOut].outNodes.Count; i++)
        {
            if (nodes[tempOut].outNodes[i].n == connection.inNode)
            {
                tempNCC = nodes[tempOut].outNodes[i];
                break;
            }
        }

        nodes[tempOut].RemoveOutNode(tempNCC);

        connections.Remove(connection);
        
        //connectionCount = connections.Count;

        //Save for undoing
        SaveTree();
    }

    //Create connection between two nodes
    private void CreateConnection(Node inNode, Node outNode)
    {
        //Save for undoing
        Undo.RecordObject(branchingTreeObj, "Add connection");

        if (connections == null)
        {
            connections = new List<ConnectionLine>();
        }

        bool done = false;

        int inIndex = 0;
        int outIndex = 0;

        for (int i = 0; i < nodes.Count; i++)
        {
            //Find the node we're talking about
            if (nodes[i] == outNode)
            {
                //Go through all the out connections on that node
                for (int j = 0; j < nodes[i].outPoint.Count; j++)
                {
                    //If the out connection on this node is the current selected out connected, make that the in index
                    if (nodes[i].outPoint[j] == selectedOutPoint)
                    {
                        outIndex = j;
                        done = true;
                        break;
                    }
                }
            }
            if (done)
                break;
        }

        done = false;

        for (int i = 0; i < nodes.Count; i++)
        {
            //Find the node we're talking about
            if (nodes[i] == inNode)
            {
                //Go through all the in connections on that node
                for (int j = 0; j < nodes[i].inPoint.Count; j++)
                {
                    //If the in connection on this node is the current selected in connected, make that the out index
                    if (nodes[i].inPoint[j] == selectedInPoint)
                    {
                        inIndex = j;
                        done = true;
                        break;
                    }
                }
            }
            if (done)
                break;
        }

        inNode.AddInNode(outNode, inIndex);
        outNode.AddOutNode(inNode, outIndex);

        connections.Add(new ConnectionLine(selectedInPoint, selectedOutPoint, inNode, outNode, OnClickRemoveConnection));
        connections[connections.Count - 1].isFlowLine = (inIndex == 0) ? true : false;

        //connectionCount = connections.Count;

        //Save for undoing
        SaveTree();
    }

    private void CreateConnectionForLoad(Node inNode, Node outNode, int inIndex)
    {
        if (connections == null)
        {
            connections = new List<ConnectionLine>();
        }

        connections.Add(new ConnectionLine(selectedInPoint, selectedOutPoint, inNode, outNode, OnClickRemoveConnection));
        connections[connections.Count - 1].isFlowLine = (inIndex == 0) ? true : false;

        //connectionCount = connections.Count;
    }

    //Clear current connection point selection
    private void ClearConnectionSelection()
    {
        selectedInPoint = null;
        selectedOutPoint = null;
        draggingLine = false;
    }

    private List<List<DialogueTreeScriptableObj.InOutConnections>> RemoveConnectionDuplicates(List<List<DialogueTreeScriptableObj.InOutConnections>> inOutList)
    {
        List<List<DialogueTreeScriptableObj.InOutConnections>> tempReturn = inOutList;

        DialogueTreeScriptableObj.InOutConnections tempConnection;

        for (int i = 0; i < tempReturn.Count; i++)
        {            
            for (int j = 0; j < tempReturn[i].Count; j++)
            {
                tempConnection = tempReturn[i][j];

                for (int k = 0; k < tempReturn[i].Count; k++)
                {
                    if (k != j)
                    {
                        if (tempReturn[i][k].nodeIndex == tempConnection.nodeIndex &&
                            tempReturn[i][k].connectionIndex == tempConnection.connectionIndex)
                        {
                            tempReturn[i].RemoveAt(k);
                            k--;
                        }
                    }
                }
            }
        }

        return tempReturn;
    }
}
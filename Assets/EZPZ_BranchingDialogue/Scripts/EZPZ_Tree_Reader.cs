//Copyright 2020, Zach Phillips, All rights reserved.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EZPZ_Tree_Reader : MonoBehaviour {

    [Tooltip("Needed to access global variables")]
    public EZPZ_TreeInfoHolder TreeInfoHolder;

    public DialogueTreeScriptableObj treeToRead;

    private int currentNodeIndex = 0;
    private List<int> editorStartIndexes = new List<int>();
    private List<DialogueTreeScriptableObj> editorPreviousTrees = new List<DialogueTreeScriptableObj>(); 

    private bool speakerChanged = false;

    private bool dialogueAdvancedSpeakerReplacement = false;
    private string savedSpeakerName;
    private GameObject savedSpeakerObject;
    private Sprite savedSpeakerSprite;

    private List<DialogueTreeScriptableObj.NodeData> nodeList = new List<DialogueTreeScriptableObj.NodeData>();

    private struct SaveVariables
    {
        string name;

        bool bVal;
        int iVal;
        float fVal;
        string sVal;

        #region Getters/Setters
        public string GetName() { return name; }
        public bool GetBVal() { return bVal; }
        public int GetIVal() { return iVal; }
        public float GetFVal() { return fVal; }
        public string GetSVal() { return sVal; }

        public void SetName(string val) { name = val; }
        public void SetBVal(bool val) { bVal = val; }
        public void SetIVal(int val) { iVal = val; }
        public void SetFVal(float val) { fVal = val; }
        public void SetSVal(string val) { sVal = val; }
        #endregion

    }

    private List<InfoNode> speakerInformationList = new List<InfoNode>();

    private List<List<SaveVariables>> variablesToSaveList = new List<List<SaveVariables>>();

    /// <summary>
    /// Types of nodes. Used for reading and returning values
    /// </summary>
    public enum TreeNodeType
    {
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

    void Start()
    {
        //Try to get the TreeInfoHolder if it was not assigned
        if (TreeInfoHolder == null)
            TreeInfoHolder = GameObject.Find("EZPZ_InfoHolder").GetComponent<EZPZ_TreeInfoHolder>();

        //If still not available, flash warning
        if (TreeInfoHolder == null)
            Debug.LogWarning("The EZPZ_InfoHolder is not set up on the EZPZ_Tree_Reader script. Without it you will not be able to access global variables in your dialogue trees.");
    }

    private bool RecordTreeForEditorSaving(DialogueTreeScriptableObj tree)
    {
        if (!editorPreviousTrees.Contains(tree))
        {
            editorPreviousTrees.Add(tree);
            editorStartIndexes.Add(tree.startIndex);
            variablesToSaveList.Add(new List<SaveVariables>());
            speakerInformationList.Add(new InfoNode());
            return true;
        }
        else
        {
            //If it already exists, add it to the back of the list so the most recently changed values get applied last
            int foundIndex = -1;

            for (int i = 0; i < editorPreviousTrees.Count; i++)
            {
                if (editorPreviousTrees[i] == tree)
                {
                    foundIndex = i;
                    break;
                }
            }

            if (foundIndex != -1)
            {
                int tempint = editorStartIndexes[foundIndex];
                List<SaveVariables> tempVarList = variablesToSaveList[foundIndex];
                InfoNode tempNode = speakerInformationList[foundIndex];

                editorPreviousTrees.RemoveAt(foundIndex);
                editorStartIndexes.RemoveAt(foundIndex);
                variablesToSaveList.RemoveAt(foundIndex);
                speakerInformationList.RemoveAt(foundIndex);

                editorPreviousTrees.Add(tree);
                editorStartIndexes.Add(tempint);
                variablesToSaveList.Add(tempVarList);
                speakerInformationList.Add(tempNode);
            }
        }

        return false;
    }

    //Eventually extend this to actually check for paths
    //Right now it simply checks to make sure there is an info, start, and end node
    void CheckToMakeSureTreeIsValid()
    {
        bool hasInfo = false;
        bool hasStart = false;
        bool hasEnd = false;

        foreach(DialogueTreeScriptableObj.NodeData nd in nodeList)
        {
            if (nd.enumType == 0)
                hasInfo = true;
            else if (nd.enumType == 1)
                hasStart = true;
            else if (nd.enumType == 2)
                hasEnd = true;
        }

        if (!hasInfo)
            Debug.LogError("EZPZ Branching Dialogue: The tree you're trying to read is not valid! Reason: " +
                           "There isn't an Info Node! This means your tree (Tree Name: " + treeToRead.treeName + ") has most likely been corrupted." +
                           "You will not be able to end this tree naturally.");

        if (!hasStart)
            Debug.LogError("EZPZ Branching Dialogue: The tree you're trying to read is not valid! Reason: " +
                           "There are no Start Nodes in this tree (Tree Name: " + treeToRead.treeName + "). " +
                           "You will not be able to end this tree naturally.");

        if (!hasEnd)
            Debug.LogError("EZPZ Branching Dialogue: The tree you're trying to read is not valid! Reason: " +
                           "There are no End Nodes in this tree (Tree Name: " +  treeToRead.treeName + "). " +
                           "You will not be able to end this tree naturally.");
    }

    /// <summary>
    /// Set the tree to read
    /// </summary>
    /// <param name="tree"></param>
    public void SetTree(DialogueTreeScriptableObj tree)
    {
#if UNITY_EDITOR
        //Resets variables that changed during runtime in the editor
        //Please let me know if this breaks, and if possible, send me a video of it!
        if (RecordTreeForEditorSaving(tree))
            SaveVariablesForChange(editorPreviousTrees.Count - 1);
        //if (treeToRead != null && variablesToSaveList[editorPreviousTrees.Count - 1].Count > 0)
        //    ResetChangedVariables(editorPreviousTrees.Count - 1);
        //variablesToSaveList[editorPreviousTrees.Count - 1].Clear();
        //SaveVariablesForChange(editorPreviousTrees.Count - 1);
#endif
        treeToRead = tree;
        nodeList = treeToRead.nodeList;
        CheckToMakeSureTreeIsValid();
        StartTree(treeToRead.startIndex);
    }

    /// <summary>
    /// Set the tree to read. Include start node to skip calling StartTree
    /// </summary>
    /// <param name="tree"></param>
    /// <param name="startIndex"></param>
    public void SetTree(DialogueTreeScriptableObj tree, int startIndex)
    {
#if UNITY_EDITOR
        //Resets variables that changed during runtime in the editor
        //Please let me know if this breaks, and if possible, send me a video of it!
        if (RecordTreeForEditorSaving(tree))
            SaveVariablesForChange(editorPreviousTrees.Count - 1);
        //if (treeToRead != null && variablesToSaveList.Count > 0)
        //    ResetChangedVariables(editorPreviousTrees.Count - 1);
        //variablesToSaveList[editorPreviousTrees.Count - 1].Clear();
        //SaveVariablesForChange(editorPreviousTrees.Count - 1);
#endif
        treeToRead = tree;
        nodeList = treeToRead.nodeList;
        CheckToMakeSureTreeIsValid();
        StartTree(startIndex);
    }

    /// <summary>
    /// Sets the current node index to a specific number. Not suggested but hey, you do you
    /// </summary>
    /// <param name="index"></param>
    public void SetNodeIndex(int index)
    {
        currentNodeIndex = index;
    }

    /// <summary>
    /// Set the tree to a specified beginning
    /// </summary>
    /// <param name="startIndex">The index of the chosen start node. 0 is default</param>
    public void StartTree(int startIndex)
    {
        bool found = false;

        for (int i = 0; i < nodeList.Count; i++)
        {
            //If it's a start node
            if (nodeList[i].enumType == 1)
            {
                if (nodeList[i].sN.GetID() == startIndex)
                {
                    currentNodeIndex = i;
                    found = true;
                    break;
                }
            }
        }

        if (!found)
            Debug.LogError("EZPZ Branching Dialogue: There is no Start node with the selected index in the read tree. Please check which Start node index you're calling or make sure there is a Start node with the index 0");
    }

    #region Variable Getters and Setters

    /// <summary>
    /// Returns the name of all the variables in the current tree as a List<string>
    /// </summary>
    /// <returns></returns>
    public List<string> GetVariableNamesAsList()
    {
        List<string> returnList = new List<string>();

        foreach(DialogueTreeScriptableObj.VariableData vd in treeToRead.variableList)
            returnList.Add(vd.name);

        return returnList;
    }

    /// <summary>
    /// Returns the name of all the variables in the current tree as a string array
    /// </summary>
    /// <returns></returns>
    public string[] GetVariableNamesAsArray()
    {
        string[] returnList = new string[treeToRead.variableList.Count];

        for (int i = 0; i < treeToRead.variableList.Count; i++)
            returnList[i] = treeToRead.variableList[i].name;

        return returnList;
    }

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
        treeToRead.AddVariable(varName, bValue, iValue, fValue, sValue);
    }

    /// <summary>
    /// Removes a variable by name. Returns false variable not found, ignoring case
    /// </summary>
    /// <param name="varName"></param>
    /// <returns></returns>
    public bool RemoveVariable(string varName)
    {
        return treeToRead.RemoveVariable(varName);
    }

    /// <summary>
    /// Sets the bool value of the variable. Returns false if variable not found, ignoring case
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetVariableBoolValue(string variableName, bool value)
    {
        return treeToRead.SetVariableBoolValue(variableName, value);
    }

    /// <summary>
    /// Sets the int value of the variable. Returns false if variable not found, ignoring case
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetVariableIntValue(string variableName, int value)
    {
        return treeToRead.SetVariableIntValue(variableName, value);
    }

    /// <summary>
    /// Sets the float value of the variable. Returns false if variable not found, ignoring case
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetVariableFloatValue(string variableName, float value)
    {
        return treeToRead.SetVariableFloatValue(variableName, value);
    }

    /// <summary>
    /// Sets the string value of the variable. Returns false if variable not found, ignoring case
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetVariableStringValue(string variableName, string value)
    {
        return treeToRead.SetVariableStringValue(variableName, value);
    }

    /// <summary>
    /// Returns the bool value of the variable. Returns false if variable is not found
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public bool GetVariableBoolValue(string variableName)
    {
        return treeToRead.GetVariableBoolValue(variableName);
    }

    /// <summary>
    /// Returns the bool value of the variable. Returns -1 if variable is not found
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public int GetVariableIntValue(string variableName)
    {
        return treeToRead.GetVariableIntValue(variableName);
    }

    /// <summary>
    /// Returns the bool value of the variable. Returns -1f if variable is not found
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public float GetVariableFloatValue(string variableName)
    {
        return treeToRead.GetVariableFloatValue(variableName);
    }

    /// <summary>
    /// Returns the string value of the variable. Returns NULL if variable is not found
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public string GetVariableStringValue(string variableName)
    {
        return treeToRead.GetVariableStringValue(variableName);
    }

    #endregion

    /// <summary>
    /// Returns the string name of the current node. Spaces in node names are represented with underscrores (_)
    /// </summary>
    /// <returns></returns>
    public string GetCurrentNodeName()
    {
        return Enum.GetName(typeof(TreeNodeType), nodeList[currentNodeIndex].enumType);
    }

    /// <summary>
    /// Returns the enum index of the curent node.
    /// </summary>
    /// <returns></returns>
    public int GetCurrentNodeEnumNum()
    {
        return nodeList[currentNodeIndex].enumType;
    }

    /// <summary>
    /// Returns the string name of the node that is going to be called next. Returns NULL if no next node exists
    /// </summary>
    /// <returns></returns>
    public string GetNextNodeName()
    {
        string tempReturn = "NULL";

        if (nodeList[currentNodeIndex].outNodeIndexes.Count == 1)
            tempReturn = Enum.GetName(typeof(TreeNodeType), nodeList[nodeList[currentNodeIndex].outNodeIndexes[0].nodeIndex].enumType);

        return tempReturn;
    }

    private bool IsStopableNode(DialogueTreeScriptableObj.NodeData node)
    {
        /*Stopping nodes are:
         * End - 2
         * Dialogue - 3
         * Branch - 4
        */

        bool isIt = false;

        switch(node.enumType)
        {
            case 2:
            case 3:
            case 4:
                isIt = true;
                break;
            //add more cases as more nodes are brought in
        }

        return isIt;
    }

    private bool IsLogicNode(DialogueTreeScriptableObj.NodeData node)
    {
        /*Logic nodes are:
         * Logic - 6
         * Edit Variable - 7
         * Start Change - 10
        */

        bool isIt = false;

        switch (node.enumType)
        {
            case 6:
            case 7:
            case 10:
                isIt = true;
                break;
                //add more cases as more nodes are brought in
        }

        return isIt;
    }

    private bool SolveLogic(DialogueTreeScriptableObj.NodeData node)
    {
        bool returnValue = false;

        //Stand in for enums
        /*
         * Logic = 0
         * Edit Variable = 1
         * Start Change = 2
         */
        int logicType = -1;

        /*
         * Bool = 0
         * Int = 1
         * Float = 2
         * String = 3
         */
        int varType = -1;

        /*
         * Equals/Set = 0
         * Not Equals/Add = 1
         * Greater Than/Subtract = 2
         * Greater or Equal/Multiply = 3
         * Less Than/Divide = 4
         * Less or Equal = 5
         * And = 6
         * Or = 7
         */
        int equationType = -1;

        //Get is global for connections, default to false
        bool isAGlobal = false;
        bool isBGlobal = false;

        if (GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1) != -1)
            isAGlobal = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.isGlobal;
        if (GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2) != -1)
            isBGlobal = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.isGlobal;

        switch (node.enumType)
        {
            case 6:
                logicType = 0;
                break;
            case 7:
                logicType = 1;
                break;
            case 10:
                logicType = 2;
                break;
        }
        
        if (logicType == 0)
        {
            varType = node.lN.GetVarCompareTypeAsInt();
            equationType = node.lN.GetEquationTypeAsInt();

            if (varType == 0)
            {
                //Since bool skips a few options, we need to boost the type index to get the correct equation
                if (equationType == 2 || equationType == 3)
                    equationType += 4;
            }

            switch (varType)
            {
                case 0:
                    bool aBool;
                    bool bBool;
                    if (node.lN.hasA)
                    {
                        if (isAGlobal)
                            aBool = EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].boolVal;
                        else
                        {
                            //if (nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex == 0) //If it's set to a literal value
                            //    aBool = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.bValue;
                            //else
                            //    aBool = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex - 1].boolVal; //Need -1 because 0 index is literal value

                            aBool = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].boolVal;
                        }
                    }
                    else
                        aBool = node.lN.aValue.defaultBValue;

                    if (node.lN.hasB)
                    {
                        if (isBGlobal)
                            bBool = EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].boolVal;
                        else
                        {
                            //if (nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex == 0) //If it's set to a literal value
                            //    bBool = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.bValue;
                            //else
                            //    bBool = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex - 1].boolVal; //Need -1 because 0 index is literal value
                            
                            bBool = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].boolVal;
                        }
                    }
                    else
                        bBool = node.lN.bValue.defaultBValue;


                    switch (equationType)
                    {
                        case 0:
                            if (aBool == bBool)
                                returnValue = true;
                            break;

                        case 1:
                            if (aBool != bBool)
                                returnValue = true;
                            break;

                        case 6:
                            if (aBool && bBool)
                                returnValue = true;
                            break;

                        case 7:
                            if (aBool || bBool)
                                returnValue = true;
                            break;
                    }
                    break;

                case 1:
                    int aInt;
                    int bInt;
                    if (node.lN.hasA)
                    {
                        if (isAGlobal)
                            aInt = EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].intVal;
                        else
                        {
                            //if (nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex == 0) //If it's set to a literal value
                            //    aInt = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.iValue;
                            //else
                            //    aInt = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex - 1].intVal; //Need -1 because 0 index is literal value
                            
                            aInt = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].intVal;
                        }
                    }
                    else
                        aInt = node.lN.aValue.defaultIValue;

                    if (node.lN.hasB)
                    {
                        if (isBGlobal)
                            bInt = EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].intVal;
                        else
                        {
                            //if (nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex == 0) //If it's set to a literal value
                            //    bInt = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.iValue;
                            //else
                            //    bInt = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex - 1].intVal; //Need -1 because 0 index is literal value
                            
                            bInt = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].intVal;
                        }
                    }
                    else
                        bInt = node.lN.bValue.defaultIValue;

                    switch (equationType)
                    {
                        case 0:
                            if (aInt == bInt)
                                returnValue = true;
                            break;

                        case 1:
                            if (aInt != bInt)
                                returnValue = true;
                            break;

                        case 2:
                            if (aInt > bInt)
                                returnValue = true;
                            break;

                        case 3:
                            if (aInt >= bInt)
                                returnValue = true;
                            break;

                        case 4:
                            if (aInt < bInt)
                                returnValue = true;
                            break;

                        case 5:
                            if (aInt <= bInt)
                                returnValue = true;
                            break;
                    }
                    break;

                case 2:
                    float aFloat;
                    float bFloat;
                    if (node.lN.hasA)
                    {
                        if (isAGlobal)
                            aFloat = EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].floatVal;
                        else
                        {
                            //if (nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex == 0) //If it's set to a literal value
                            //    aFloat = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.fValue;
                            //else
                            //    aFloat = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex - 1].floatVal; //Need -1 because 0 index is literal value
                            
                            aFloat = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].floatVal;
                        }
                    }
                    else
                        aFloat = node.lN.aValue.defaultFValue;

                    if (node.lN.hasB)
                    {
                        if (isBGlobal)
                            bFloat = EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].floatVal;
                        else
                        {
                            //if (nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex == 0) //If it's set to a literal value
                            //    bFloat = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.fValue;
                            //else
                            //    bFloat = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex - 1].floatVal; //Need -1 because 0 index is literal value
                            
                            bFloat = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].floatVal;
                        }
                    }
                    else
                        bFloat = node.lN.bValue.defaultFValue;

                    switch (equationType)
                    {
                        case 0:
                            if (aFloat == bFloat)
                                returnValue = true;
                            break;

                        case 1:
                            if (aFloat != bFloat)
                                returnValue = true;
                            break;

                        case 2:
                            if (aFloat > bFloat)
                                returnValue = true;
                            break;

                        case 3:
                            if (aFloat >= bFloat)
                                returnValue = true;
                            break;

                        case 4:
                            if (aFloat < bFloat)
                                returnValue = true;
                            break;

                        case 5:
                            if (aFloat <= bFloat)
                                returnValue = true;
                            break;
                    }
                    break;

                case 3:
                    string aString;
                    string bString;
                    if (node.lN.hasA)
                    {
                        if (isAGlobal)
                            aString = EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].stringVal;
                        else
                        {
                            //if (nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex == 0) //If it's set to a literal value
                            //    aString = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.sValue;
                            //else
                            //    aString = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex - 1].stringVal; //Need -1 because 0 index is literal value
                            
                            aString = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].stringVal;
                        }
                    }
                    else
                        aString = node.lN.aValue.defaultSValue;

                    if (node.lN.hasB)
                    {
                        if (isBGlobal)
                            bString = EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].stringVal;
                        else
                        {
                            //if (nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex == 0) //If it's set to a literal value
                            //    bString = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.sValue;
                            //else
                            //    bString = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex - 1].stringVal; //Need -1 because 0 index is literal value
                            
                            bString = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].stringVal;
                        }
                    }
                    else
                        bString = node.lN.bValue.defaultSValue;

                    switch (equationType)
                    {
                        case 0:
                            if (aString == bString)
                                returnValue = true;
                            break;

                        case 1:
                            if (aString != bString)
                                returnValue = true;
                            break;
                    }
                    break;
            }
        }
        else if (logicType == 1)
        {
            varType = node.eN.GetVarCompareTypeAsInt();
            equationType = node.eN.GetEquationTypeAsInt();

            switch (varType)
            {
                case 0:
                    bool newBool;

                    if (node.eN.hasA)
                    {
                        //if (isAGlobal || nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex != 0) //If it's not set to a literal value
                        //{
                        if (node.eN.hasB)
                        {
                            if (isBGlobal)
                                newBool = EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].boolVal;
                            else
                            {
                                //if (nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex == 0) //If it's set to a literal value
                                //    newBool = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.bValue;
                                //else
                                //    newBool = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex - 1].boolVal; //Need -1 because 0 index is literal value
                                
                                newBool = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].boolVal;
                            }
                        }
                        else
                            newBool = node.eN.bValue.defaultBValue;
                        
                        //Equation type not needed because of single valid equation
                        if (isAGlobal)
                            EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].boolVal = newBool;
                        else
                            treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].boolVal = newBool;
                        returnValue = true;
                        //}
                    }
                    break;

                case 1:
                    int newInt;
                    if (node.eN.hasA)
                    {
                        //if (isAGlobal || nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex != 0) //If it's not set to a literal value
                        //{
                        if (node.eN.hasB)
                        {
                            if (isBGlobal)
                                newInt = EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].intVal;
                            else
                            {
                                //if (nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex == 0) //If it's set to a literal value
                                //    newInt = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.iValue;
                                //else
                                //    newInt = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex - 1].intVal; //Need -1 because 0 index is literal value

                                newInt = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].intVal;
                            }
                        }
                        else
                            newInt = node.eN.bValue.defaultIValue;

                        switch (equationType)
                        {
                            case 0:
                                if (isAGlobal)
                                    EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].intVal = newInt;
                                else
                                    treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].intVal = newInt; //Need -1 because 0 index is literal value
                                returnValue = true;
                                break;

                            case 1:
                                if (isAGlobal)
                                    EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].intVal += newInt;
                                else
                                    treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].intVal += newInt; //Need -1 because 0 index is literal value
                                returnValue = true;
                                break;

                            case 2:
                                if (isAGlobal)
                                    EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].intVal -= newInt;
                                else
                                    treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].intVal -= newInt; //Need -1 because 0 index is literal value
                                returnValue = true;
                                break;

                            case 3:
                                if (isAGlobal)
                                    EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].intVal *= newInt;
                                else
                                    treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].intVal *= newInt; //Need -1 because 0 index is literal value
                                returnValue = true;
                                break;

                            case 4:
                                if (isAGlobal)
                                    EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].intVal /= newInt;
                                else
                                    treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].intVal /= newInt; //Need -1 because 0 index is literal value
                                returnValue = true;
                                break;
                        }
                        //}
                    }
                    break;

                case 2:
                    float newFloat;
                    if (node.eN.hasA)
                    {
                        //if (isAGlobal || nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex != 0) //If it's not set to a literal value
                        //{
                        if (node.eN.hasB)
                        {
                            if (isBGlobal)
                                newFloat = EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].floatVal;
                            else
                            {
                                //if (nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex == 0) //If it's set to a literal value
                                //    newFloat = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.fValue;
                                //else
                                //    newFloat = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex - 1].floatVal; //Need -1 because 0 index is literal value
                                 
                                newFloat = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].floatVal;
                            }
                        }
                        else
                            newFloat = node.eN.bValue.defaultFValue;

                        switch (equationType)
                        {
                            case 0:
                                if (isAGlobal)
                                    EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].floatVal = newFloat;
                                else
                                    treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].floatVal = newFloat; //Need -1 because 0 index is literal value
                                returnValue = true;
                                break;

                            case 1:
                                if (isAGlobal)
                                    EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].floatVal += newFloat;
                                else
                                    treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].floatVal += newFloat; //Need -1 because 0 index is literal value
                                returnValue = true;
                                break;

                            case 2:
                                if (isAGlobal)
                                    EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].floatVal -= newFloat;
                                else
                                    treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].floatVal -= newFloat; //Need -1 because 0 index is literal value
                                returnValue = true;
                                break;

                            case 3:
                                if (isAGlobal)
                                    EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].floatVal *= newFloat;
                                else
                                    treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].floatVal *= newFloat; //Need -1 because 0 index is literal value
                                returnValue = true;
                                break;

                            case 4:
                                if (isAGlobal)
                                    EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].floatVal /= newFloat;
                                else
                                    treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].floatVal /= newFloat; //Need -1 because 0 index is literal value
                                returnValue = true;
                                break;
                        }
                        //}
                    }
                    break;

                case 3:
                    string newString;
                    if (node.eN.hasA)
                    {
                        //if (isAGlobal || nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex != 0) //If it's not set to a literal value
                        //{
                        if (node.eN.hasB)
                        {
                            if (isBGlobal)
                                newString = EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].stringVal;
                            else
                            {
                                //if (nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex == 0) //If it's set to a literal value
                                //    newString = nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.sValue;
                                //else
                                //    newString = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex - 1].stringVal; //Need -1 because 0 index is literal value
                                
                                newString = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 2)].vN.selectedVarIndex].stringVal;
                            }
                        }
                        else
                            newString = node.eN.bValue.defaultSValue;

                        switch (equationType)
                        {
                            case 0:
                                if (isAGlobal)
                                    EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].stringVal = newString;
                                else
                                    treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].stringVal = newString; //Need -1 because 0 index is literal value
                                returnValue = true;
                                break;

                            case 1:
                                if (isAGlobal)
                                    EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].stringVal += newString;
                                else
                                    treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].stringVal += newString; //Need -1 because 0 index is literal value
                                returnValue = true;
                                break;

                            case 2:
                                string tempString;
                                if (isAGlobal)
                                    tempString = EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].stringVal;
                                else
                                    tempString = treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].stringVal; //Need -1 because 0 index is literal value

                                while (tempString.Contains(newString))
                                {
                                    tempString = tempString.Replace(newString, "");
                                }

                                if (isAGlobal)
                                    EZPZ_TreeInfoHolder.Instance.globalVariableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].stringVal = tempString;
                                else
                                    treeToRead.variableList[nodeList[GetNodeIndexOffConnectionIndex(true, node.nodeIndex, 1)].vN.selectedVarIndex].stringVal = tempString; //Need -1 because 0 index is literal value

                                returnValue = true;
                                break;
                        }
                        //}
                    }
                    break;
            }
        }
        else if (logicType == 2)
        {
            treeToRead.startIndex = node.scN.startID;
            returnValue = true;
        }

        return returnValue;
    }

    /// <summary>
    /// Returns true if the speaker changed
    /// </summary>
    /// <returns></returns>
    public bool DidSpeakerChange()
    {
        if (speakerChanged)
        {
            speakerChanged = false;
            return true;
        }

        return false;   
    }

    /// <summary>
    /// Changes the speaker information for the tree. Returns true if successful
    /// </summary>
    /// <param name="newSpeakerName"></param>
    /// <param name="newSpeakerSprite"></param>
    /// <returns></returns>
    public bool ChangeSpeakerInfo(string newSpeakerName, Sprite newSpeakerSprite = null /*, GameObject newSpeakerObject = null*/)
    {
        //Find info node first, should be first
        for(int i = 0; i < nodeList.Count; i++)
        {
            if (nodeList[i].enumType == 0)
            {
                nodeList[i].iN.SetSpeakerName(newSpeakerName);
                nodeList[i].iN.SetSpeakerSprite(newSpeakerSprite);

                //Since Unity can't use Scene GameObjects when outside of the scene there isn't a
                //good way to allow the player to assign a GameObject inside the tree. Will revisit this in the future
                //***
                //nodeList[i].iN.SetSpeakerObject(newSpeakerObject);
                return true;
            }
        }

        Debug.LogError("EZPZ Branching Dialogue: Can not change Speaker Info! Reason: " +
                           "There isn't an Info Node! This means your tree (Tree Name: " + treeToRead.treeName + ") has most likely been corrupted.");
        return false;
    }

    /// <summary>
    /// Gets the current speaker's name. Returns NULL if Info node does not exist
    /// </summary>
    /// <returns></returns>
    public string GetSpeakerName()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            if (nodeList[i].enumType == 0)
            {
                //Run the new speaker name through a variable replacement check
                return ReplaceDialogueVariables(nodeList[i].iN.GetSpeakerName());

            }
        }

        return "NULL";
    }

    /// <summary>
    /// Gets the current speaker's sprite. Returns NULL if Info node does not exist
    /// </summary>
    /// <returns></returns>
    public Sprite GetSpeakerSprite()
    {
        for (int i = 0; i < nodeList.Count; i++)
        {
            if (nodeList[i].enumType == 0)
            {
                return nodeList[i].iN.GetSpeakerSprite();
            }
        }

        return null;
    }

    // <summary>
    // Gets the current speaker's GameObject. Returns NULL if Info node does not exist
    // </summary>
    // <returns></returns>
    // ***
    // Since Unity can't use Scene GameObjects when outside of the scene there isn't a
    // good way to allow the player to assign a GameObject inside the tree. Will revisit this in the future
    // ***
    //public GameObject GetSpeakerObject()
    //{
    //    for (int i = 0; i < nodeList.Count; i++)
    //    {
    //        if (nodeList[i].enumType == 0)
    //        {
    //            return nodeList[i].iN.GetSpeakerObject();
    //        }
    //    }
    //
    //    return null;
    //}

    private int GetNodeIndexOffConnectionIndex(bool isInIndex, int startNodeIndex, int connectionIndex)
    {
        if (isInIndex)
        {
            for (int i = 0; i < nodeList[startNodeIndex].inNodeIndexes.Count; i++)
            {
                if (nodeList[startNodeIndex].inNodeIndexes[i].connectionIndex == connectionIndex)
                {
                    return nodeList[startNodeIndex].inNodeIndexes[i].nodeIndex;
                }
            }
        }
        else
        {
            for (int i = 0; i < nodeList[startNodeIndex].outNodeIndexes.Count; i++)
            {
                if (nodeList[startNodeIndex].outNodeIndexes[i].connectionIndex == connectionIndex)
                {
                    return nodeList[startNodeIndex].outNodeIndexes[i].nodeIndex;
                }
            }
        }

        return -1;
    }

    /// <summary>
    /// Tries to go to the next stopping node with no extra commands. Returns true if succeeded
    /// </summary>
    /// <returns></returns>
    public bool GoToNextNode()
    {        
        bool succeed = false;
        int nextIndex = currentNodeIndex;

        //In case there is a infinate loop created somehow
        int infiniteLoopCounter = 100;

        if (nodeList[nextIndex].outNodeIndexes.Count == 1)
        {
            nextIndex = nodeList[nextIndex].outNodeIndexes[0].nodeIndex;

            while (infiniteLoopCounter > 0)
            {
                infiniteLoopCounter--;

                //If the last node temporarily reset the speaker info
                if (dialogueAdvancedSpeakerReplacement)
                {
                    dialogueAdvancedSpeakerReplacement = false;

                    if (ChangeSpeakerInfo(savedSpeakerName, savedSpeakerSprite/*, savedSpeakerObject*/))
                        speakerChanged = true;
                }
                if (IsStopableNode(nodeList[nextIndex]))
                {
                    //Check if the node is a dialoge node with Advanced Options enabled
                    if (nodeList[nextIndex].enumType == 3)
                    {
                        //If so, change the speaker info and reset it next loop
                        if (nodeList[nextIndex].dN.advancedOptions)
                        {
                            dialogueAdvancedSpeakerReplacement = true;
                            
                            savedSpeakerName = GetSpeakerName();
                            savedSpeakerSprite = GetSpeakerSprite();
                            //savedSpeakerObject = GetSpeakerObject();

                            if (ChangeSpeakerInfo(nodeList[nextIndex].dN.speakerName, nodeList[nextIndex].dN.speakerSprite/*, nodeList[nextIndex].dN.speakerObj*/))
                                speakerChanged = true;
                        }
                    }

                    //need to check if the next node is for a branch
                    if (nodeList[nextIndex].outNodeIndexes.Count > 0 && nodeList[nodeList[nextIndex].outNodeIndexes[0].nodeIndex].enumType == 4)
                    {
                        //Assuming the node before this only has one output...flick me if I'm wrong down the line
                        currentNodeIndex = nodeList[nextIndex].outNodeIndexes[0].nodeIndex;
                    }
                    else
                    {
                        currentNodeIndex = nextIndex;
                    }                    
                    
                    succeed = true;
                    break;
                }
                else if (IsLogicNode(nodeList[nextIndex]))
                {
                    if (SolveLogic(nodeList[nextIndex]))
                        nextIndex = GetNodeIndexOffConnectionIndex(false, nextIndex, 0);
                    else
                        nextIndex = GetNodeIndexOffConnectionIndex(false, nextIndex, 1);
                }
                else if (nodeList[nextIndex].enumType == 9)//A New Speaker node
                {
                    ChangeSpeakerInfo(nodeList[nextIndex].nN.speakerName, nodeList[nextIndex].nN.speakerSprite/*, nodeList[nextIndex].nN.speakerObj*/);

                    speakerChanged = true;

                    nextIndex = nodeList[nextIndex].outNodeIndexes[0].nodeIndex;
                }
                else if (nodeList[nextIndex].enumType == 11)//A Teleport Flow node
                {
                    //Find the related Out teleport and send the flow there
                    foreach (DialogueTreeScriptableObj.NodeData nd in nodeList)
                    {
                        if (nd.enumType == 11)
                        {
                            if (nd.tfN.teleportID == nodeList[nextIndex].tfN.teleportID && !nd.tfN.isIn)
                            {
                                nextIndex = nd.outNodeIndexes[0].nodeIndex;
                                break;
                            }
                        }
                    }
                }
                else
                    nextIndex = nodeList[nextIndex].outNodeIndexes[0].nodeIndex;
            }
            if (infiniteLoopCounter <= 0)
                Debug.LogError("EZPZ Branching Dialogue: You have hit an infinate loop while trying to go to the next node in the " + treeToRead.treeName + " tree. " +
                          "Either some of your nodes aren't connected properly, or you go through 100 nodes before hitting a stoppable node.");
        }

        return succeed;
    }

    /// <summary>
    /// Tries to go to the next stopping node with an int attatched for branching choices. Returns true if succeeded
    /// </summary>
    /// <returns></returns>
    public bool GoToNextNode(int choice)
    {
        bool succeed = false;
        bool wentDownPath = false;
        int nextIndex = currentNodeIndex;

        //In case there is a infinate loop created somehow
        int infiniteLoopCounter = 100;

        //make sure it's a branching node
        if (nodeList[currentNodeIndex].enumType == 4)
        {
            //make sure connections are linked up
            if (nodeList[currentNodeIndex].outNodeIndexes.Count > 1)
            {
                while (infiniteLoopCounter > 0)
                {
                    infiniteLoopCounter--;

                    //If the last node temporarily reset the speaker info
                    if (dialogueAdvancedSpeakerReplacement)
                    {
                        dialogueAdvancedSpeakerReplacement = false;

                        if (ChangeSpeakerInfo(savedSpeakerName, savedSpeakerSprite/*, savedSpeakerObject*/))
                            speakerChanged = true;
                    }

                    if (!wentDownPath)
                    {
                        for (int i = 0; i < nodeList[currentNodeIndex].outNodeIndexes.Count; i++)
                        {
                            if (nodeList[currentNodeIndex].outNodeIndexes[i].connectionIndex == choice)
                            {
                                nextIndex = nodeList[currentNodeIndex].outNodeIndexes[i].nodeIndex;
                                wentDownPath = true;
                            }
                        }
                    }

                    if (IsStopableNode(nodeList[nextIndex]))
                    {
                        //Check if the node is a dialoge node with Advanced Options enabled
                        if (nodeList[nextIndex].enumType == 3)
                        {
                            //If so, change the speaker info and reset it next loop
                            if (nodeList[nextIndex].dN.advancedOptions)
                            {
                                dialogueAdvancedSpeakerReplacement = true;

                                savedSpeakerName = GetSpeakerName();
                                savedSpeakerSprite = GetSpeakerSprite();
                                //savedSpeakerObject = GetSpeakerObject();

                                if (ChangeSpeakerInfo(nodeList[nextIndex].dN.speakerName, nodeList[nextIndex].dN.speakerSprite/*, nodeList[nextIndex].dN.speakerObj*/))
                                    speakerChanged = true;
                            }
                        }

                        //need to check if the next node is for a branch
                        if (nodeList[nextIndex].enumType != 4)
                        {
                            //But wait, is it connected to another branch?
                            if (nodeList[nextIndex].outNodeIndexes.Count > 0 &&
                                nodeList[nodeList[nextIndex].outNodeIndexes[0].nodeIndex].enumType == 4)
                                currentNodeIndex = nodeList[nodeList[nextIndex].outNodeIndexes[0].nodeIndex].nodeIndex;
                            else
                                currentNodeIndex = nodeList[nextIndex].nodeIndex;
                        }
                        else //Assuming the node before this only has one output...flick me if I'm wrong down the line
                        {
                            currentNodeIndex = nodeList[nextIndex].outNodeIndexes[0].nodeIndex;
                        }
                        succeed = true;
                        break;
                    }
                    else if (IsLogicNode(nodeList[nextIndex]))
                    {
                        if (SolveLogic(nodeList[nextIndex]))
                            nextIndex = GetNodeIndexOffConnectionIndex(false, nextIndex, 0);
                        else
                            nextIndex = GetNodeIndexOffConnectionIndex(false, nextIndex, 1);
                    }
                    else if (nodeList[nextIndex].enumType == 9)//A New Speaker node
                    {
                        ChangeSpeakerInfo(nodeList[nextIndex].nN.speakerName, nodeList[nextIndex].nN.speakerSprite/*, nodeList[nextIndex].nN.speakerObj*/);

                        speakerChanged = true;

                        nextIndex = nodeList[nextIndex].outNodeIndexes[0].nodeIndex;
                    }
                    else if (nodeList[nextIndex].enumType == 11)//A Teleport Flow node
                    {
                        //Find the related Out teleport and send the flow there
                        foreach (DialogueTreeScriptableObj.NodeData nd in nodeList)
                        {
                            if (nd.enumType == 11)
                            {
                                if (nd.tfN.teleportID == nodeList[nextIndex].tfN.teleportID && !nd.tfN.isIn)
                                {
                                    nextIndex = nd.outNodeIndexes[0].nodeIndex;
                                    break;
                                }
                            }
                        }
                    }
                    else
                        nextIndex = nodeList[nextIndex].outNodeIndexes[0].nodeIndex;
                }
                if (infiniteLoopCounter <= 0)
                    Debug.LogError("EZPZ Branching Dialogue: You have hit an infinate loop while trying to go to the next node in the " + treeToRead.treeName + " tree. " +
                              "Either some of your nodes aren't connected properly, or you go through 100 nodes before hitting a stoppable node.");
            }
        }

        return succeed;
    }

    /// <summary>
    /// Returns the dialogue on this node if it's a Dialogue Node. Returns NULL if not on a Dialogue Node
    /// </summary>
    /// <returns></returns>
    public string GetNodeDialogue()
    {
        string tempReturn = "NULL";

        if (nodeList[currentNodeIndex].enumType == 3)
            tempReturn = nodeList[currentNodeIndex].dN.GetDialogue();

        //Now switch out the variable replacements with info
        tempReturn = ReplaceDialogueVariables(tempReturn);

        return tempReturn;
    }

    private string ReplaceDialogueVariables(string input)
    {
        //Replaced variables look like {var:VarName:VarType}, such as {var:oranges:int}, ignoring case

        string tempReturn = input;
        char[] charInput = input.ToCharArray();

        //Find a {
        for (int i = 0; i < charInput.Length; i++)
        {
            bool foundSomething = false;
            bool global = false;

            if (charInput[i] == '{')
            {
                //Check to see if the length of the rest of the input can support a variable
                if (i + 11 < charInput.Length)
                {
                    //Next see if the next four are "var:"
                    char[] varToArr = new char[4];
                    for (int temp = 0; temp < varToArr.Length; temp++)
                    {
                        if (i + temp + 1 < charInput.Length)
                            varToArr[temp] = charInput[i + temp + 1];
                    }
                    string fullVarName = new string(varToArr).ToLower();

                    if (fullVarName == "var:")
                        foundSomething = true;
                    else
                    {
                        //Next see if the next ten are "varGlobal:"
                        varToArr = new char[10];
                        for (int temp = 0; temp < varToArr.Length; temp++)
                        {
                            if (i + temp + 1 < charInput.Length)
                                varToArr[temp] = charInput[i + temp + 1];
                        }
                        fullVarName = new string(varToArr).ToLower();

                        if (fullVarName == "varglobal:")
                        {
                            if (TreeInfoHolder != null)
                            {
                                foundSomething = true;
                                global = true;
                            }
                            else
                                Debug.LogError("The EZPZ_InfoHolder is not set up on the EZPZ_Tree_Reader script. You are not able to access global variables in your dialogue trees.");
                        }
                    }
                }
            }

            if (foundSomething)
            {
                int tempIndex = i;

                //adjust for the var: and varGlobal:
                if (global)
                    tempIndex += 11;
                else
                    tempIndex += 5;

                List<char> varName = new List<char>();

                while (tempIndex < charInput.Length && charInput[tempIndex] != ':' && charInput[tempIndex] != '}')
                {
                    varName.Add(charInput[tempIndex]);
                    tempIndex++;
                }

                //Call string and find in variable list
                char[] varToArr = new char[varName.Count];
                for (int temp = 0; temp < varToArr.Length; temp++)
                    varToArr[temp] = varName[temp];
                string fullVarName = new string (varToArr);
                fullVarName = fullVarName.ToLower();

                //make sure they aren't calling the speaker's name
                if (fullVarName != "speakersname" && fullVarName != "speakername")
                {
                    int varIndex = -1;

                    for (int j = 0; j < treeToRead.variableList.Count; j++)
                    {
                        if (!global)
                        {
                            if (treeToRead.variableList[j].name.ToLower() == fullVarName)
                            {
                                varIndex = j;
                                break;
                            }
                        }
                        else
                        {
                            if (TreeInfoHolder.globalVariableList[j].name.ToLower() == fullVarName)
                            {
                                varIndex = j;
                                break;
                            }
                        }
                    }

                    //get the var type and replace as string
                    tempIndex++; //get past the :
                    if (varIndex != -1)
                    {
                        varName.Clear();

                        while (tempIndex < charInput.Length && charInput[tempIndex] != '}')
                        {
                            varName.Add(charInput[tempIndex]);
                            tempIndex++;
                        }

                        varToArr = new char[varName.Count];
                        for (int temp = 0; temp < varToArr.Length; temp++)
                            varToArr[temp] = varName[temp];
                        fullVarName = new string(varToArr).ToLower();

                        string replaceValue = "";
                        if (global)
                        {
                            switch (fullVarName)
                            {
                                case "bool":
                                    replaceValue = TreeInfoHolder.globalVariableList[varIndex].boolVal.ToString();
                                    break;

                                case "int":
                                    replaceValue = TreeInfoHolder.globalVariableList[varIndex].intVal.ToString();
                                    break;

                                case "float":
                                    replaceValue = TreeInfoHolder.globalVariableList[varIndex].floatVal.ToString();
                                    break;

                                case "string":
                                    replaceValue = TreeInfoHolder.globalVariableList[varIndex].stringVal;
                                    break;
                            }
                        }
                        else
                        {
                            switch (fullVarName)
                            {
                                case "bool":
                                    replaceValue = treeToRead.variableList[varIndex].boolVal.ToString();
                                    break;

                                case "int":
                                    replaceValue = treeToRead.variableList[varIndex].intVal.ToString();
                                    break;

                                case "float":
                                    replaceValue = treeToRead.variableList[varIndex].floatVal.ToString();
                                    break;

                                case "string":
                                    replaceValue = treeToRead.variableList[varIndex].stringVal;
                                    break;
                            }
                        }

                        List<char> replaceLine = new List<char>();

                        //repalce the line with the var
                        for (int k = i; k <= tempIndex; k++)
                        {
                            replaceLine.Add(charInput[k]);
                        }

                        char[] replaceCharArr = new char[replaceLine.Count];
                        for (int temp = 0; temp < replaceCharArr.Length; temp++)
                            replaceCharArr[temp] = replaceLine[temp];
                        string replaceString = new string(replaceCharArr);

                        tempReturn = tempReturn.Replace(replaceString, replaceValue);
                    }                    
                }
                else
                {
                    while (tempIndex < charInput.Length && charInput[tempIndex] != '}')
                    {
                        tempIndex++;
                    }

                    List<char> replaceLine = new List<char>();

                    //repalce the line with the var
                    for (int k = i; k <= tempIndex; k++)
                    {
                        replaceLine.Add(charInput[k]);
                    }

                    char[] replaceCharArr = new char[replaceLine.Count];
                    for (int temp = 0; temp < replaceCharArr.Length; temp++)
                        replaceCharArr[temp] = replaceLine[temp];
                    string replaceString = new string(replaceCharArr);

                    tempReturn = tempReturn.Replace(replaceString, GetSpeakerName());
                }

                //Set i to skip the rest of this
                i = tempIndex - 1;
            }
        }

        return tempReturn;
    }

    /// <summary>
    /// Returns the dialogues for the branch node. Sorted in connection order, top down. Top connection is the main question. Returns NULL if not on a Branch node
    /// </summary>
    /// <returns></returns>
    public string[] GetBranchDialogues()
    {
        string[] tempReturn = new string[nodeList[currentNodeIndex].inNodeIndexes.Count];
        tempReturn[0] = "NULL";

        //Make sure it's a branch node
        if (nodeList[currentNodeIndex].enumType == 4)
        {
            //Go through each in node
            for (int i = 0; i < nodeList[currentNodeIndex].inNodeIndexes.Count; i++)
            {
                //Get each in node in connection order
                for (int j = 0; j < nodeList[currentNodeIndex].inNodeIndexes.Count; j++)
                {
                    if (nodeList[currentNodeIndex].inNodeIndexes[j].connectionIndex == i)
                    {
                        tempReturn[i] = ReplaceDialogueVariables(nodeList[nodeList[currentNodeIndex].inNodeIndexes[j].nodeIndex].dN.GetDialogue());
                    }
                }
            }
        }

        return tempReturn;
    }

#if UNITY_EDITOR
    void ResetChangedVariables(int index)
    {
        for (int i = 0; i < variablesToSaveList[index].Count; i++)
        {
            //Global vars
            if (TreeInfoHolder != null)
            {
                for (int j = 0; j < TreeInfoHolder.globalVariableList.Count; j++)
                {
                    if (variablesToSaveList[index][i].GetName() == TreeInfoHolder.globalVariableList[j].name)
                    {
                        TreeInfoHolder.globalVariableList[j].boolVal = variablesToSaveList[index][i].GetBVal();
                        TreeInfoHolder.globalVariableList[j].intVal = variablesToSaveList[index][i].GetIVal();
                        TreeInfoHolder.globalVariableList[j].floatVal = variablesToSaveList[index][i].GetFVal();
                        TreeInfoHolder.globalVariableList[j].stringVal = variablesToSaveList[index][i].GetSVal();
                    }
                }
            }

            //Local vars
            for (int j = 0; j < editorPreviousTrees[index].variableList.Count; j++)
            {
                if (variablesToSaveList[index][i].GetName() == editorPreviousTrees[index].variableList[j].name)
                {
                    editorPreviousTrees[index].variableList[j].boolVal = variablesToSaveList[index][i].GetBVal();
                    editorPreviousTrees[index].variableList[j].intVal = variablesToSaveList[index][i].GetIVal();
                    editorPreviousTrees[index].variableList[j].floatVal = variablesToSaveList[index][i].GetFVal();
                    editorPreviousTrees[index].variableList[j].stringVal = variablesToSaveList[index][i].GetSVal();
                }
            }

            //Info node
            for (int j = 0; j < editorPreviousTrees[index].nodeList.Count; j++)
            {
                if (editorPreviousTrees[index].nodeList[j].enumType == 0)
                {
                    editorPreviousTrees[index].nodeList[j].iN.SetSpeakerName(speakerInformationList[index].GetSpeakerName());
                    editorPreviousTrees[index].nodeList[j].iN.SetSpeakerSprite(speakerInformationList[index].GetSpeakerSprite());
                    //editorPreviousTrees[index].nodeList[j].iN.SetSpeakerObject(speakerInformationList[index].GetSpeakerObject());
                    break;
                }
            }
        }
    }

    void SaveVariablesForChange(int index)
    {
        List<SaveVariables> svl = new List<SaveVariables>();

        //Save global nodes
        if (TreeInfoHolder != null)
        {
            for (int j = 0; j < TreeInfoHolder.globalVariableList.Count; j++)
            {
                SaveVariables sv = new SaveVariables();
                sv.SetName(TreeInfoHolder.globalVariableList[j].name);
                sv.SetBVal(TreeInfoHolder.globalVariableList[j].boolVal);
                sv.SetIVal(TreeInfoHolder.globalVariableList[j].intVal);
                sv.SetFVal(TreeInfoHolder.globalVariableList[j].floatVal);
                sv.SetSVal(TreeInfoHolder.globalVariableList[j].stringVal);

                svl.Add(sv);
            }
        }

        //Save local variables
        for (int j = 0; j < editorPreviousTrees[index].variableList.Count; j++)
        {
            SaveVariables sv = new SaveVariables();
            sv.SetName(editorPreviousTrees[index].variableList[j].name);
            sv.SetBVal(editorPreviousTrees[index].variableList[j].boolVal);
            sv.SetIVal(editorPreviousTrees[index].variableList[j].intVal);
            sv.SetFVal(editorPreviousTrees[index].variableList[j].floatVal);
            sv.SetSVal(editorPreviousTrees[index].variableList[j].stringVal);

            svl.Add(sv);
        }

        //Save info node
        for (int j = 0; j < editorPreviousTrees[index].nodeList.Count; j++)
        {
            if (editorPreviousTrees[index].nodeList[j].enumType == 0)
            {
                speakerInformationList[index].SetSpeakerName(editorPreviousTrees[index].nodeList[j].iN.GetSpeakerName());
                speakerInformationList[index].SetSpeakerSprite(editorPreviousTrees[index].nodeList[j].iN.GetSpeakerSprite());
                //speakerInformationList[index].SetSpeakerObject(editorPreviousTrees[index].nodeList[j].iN.GetSpeakerObject());
                break;
            }
        }

        variablesToSaveList[index] = svl;
    }

    void OnApplicationQuit()
    {
        if (treeToRead != null || editorPreviousTrees.Count > 0)
        {
            for (int i = 0; i < editorPreviousTrees.Count; i++)
            {
                editorPreviousTrees[i].startIndex = editorStartIndexes[i];

                ResetChangedVariables(i);
            }
        }
    }
#endif
}

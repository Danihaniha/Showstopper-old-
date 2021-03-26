//Copyright 2020, Zach Phillips, All rights reserved.
using System.Collections.Generic;
using UnityEngine;

public class EZPZ_TreeInfoHolder : MonoBehaviour {

    //Needs an instance to allow trees to grab global variables
    public static EZPZ_TreeInfoHolder _instance = null;
    public static EZPZ_TreeInfoHolder Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load("EZPZ_InfoHolder", typeof(EZPZ_TreeInfoHolder)) as EZPZ_TreeInfoHolder;

            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    [System.Serializable]
    public class VariableData
    {
        public string name = "VariableName";

        public bool boolVal = false;
        public int intVal = 0;
        public float floatVal = 0.0f;
        public string stringVal = "";
    }

    [Header("REMEMBER TO APPLY ALL CHANGES TO THE PREFAB")]
    [SerializeField]
    [Tooltip("This variable list can be accessed in any dialogue tree by using varGlobal instead of var in a Dialogue node")]
    public List<VariableData> globalVariableList = new List<VariableData>();

    [SerializeField]
    [Tooltip("You can search through and grab trees from this list. While you don't need to fill this with every Dialogue Tree you make, it is recommented.")]
    public List<DialogueTreeScriptableObj> dialogueTreeList;

    /// <summary>
    /// Returns a DialogueTreeScriptable object located in the dialogueTreeList on the EZPZ_InfoHolder game object. Returns null if the tree does not exist
    /// </summary>
    /// <param name="name">The name of the tree. Assign it at the top of the tree's inspector</param>
    /// <returns></returns>
    public DialogueTreeScriptableObj GetTree(string name)
    {
        for (int i = 0; i < dialogueTreeList.Count; i++)
        {
            if (dialogueTreeList[i].treeName == name)
                return dialogueTreeList[i];
        }

        Debug.LogWarning("EZPZ Branching Dialogue: You tried to access a tree that didn't exist");

        return null;
    }

    /// <summary>
    /// Returns a DialogueTreeScriptable object located in the dialogueTreeList on the EZPZ_InfoHolder game object. Returns null if the tree does not exist
    /// </summary>
    /// <param name="index">The index of the tree in the list</param>
    /// <returns></returns>
    public DialogueTreeScriptableObj GetTree(int index)
    {
        if (index <= dialogueTreeList.Count - 1)
            return dialogueTreeList[index];

        Debug.LogWarning("EZPZ Branching Dialogue: You tried to access a tree that didn't exist");

        return null;
    }    

    /// <summary>
    /// Add a new global variable to the global variable list. *Persists after runtime*
    /// </summary>
    /// <param name="varName"></param>
    /// <param name="bValue"></param>
    /// <param name="iValue"></param>
    /// <param name="fValue"></param>
    /// <param name="sValue"></param>
    public void AddGlobalVariable(string varName = "New Variable", bool bValue = false, int iValue = 0, float fValue = 0.0f, string sValue = "")
    {
        VariableData vd = new VariableData();

        vd.name = varName;
        vd.boolVal = bValue;
        vd.intVal = iValue;
        vd.floatVal = fValue;
        vd.stringVal = sValue;

        globalVariableList.Add(vd);
    }

    /// <summary>
    /// Removes a global variable by name. Returns false variable not found, ignoring case
    /// </summary>
    /// <param name="varName"></param>
    /// <returns></returns>
    public bool RemoveGlobalVariable(string varName)
    {
        for(int i = 0; i < globalVariableList.Count; i++)
        {
            if (globalVariableList[i].name.ToLower() == varName.ToLower())
            {
                globalVariableList.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sets the bool value of the global variable. Returns false if variable not found, ignoring case
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetGlobalVariableBoolValue(string variableName, bool value)
    {
        for (int i = 0; i < globalVariableList.Count; i++)
        {
            if (globalVariableList[i].name.ToLower() == variableName.ToLower())
            {
                globalVariableList[i].boolVal = value;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sets the int value of the global variable. Returns false if variable not found, ignoring case
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetGlobalVariableIntValue(string variableName, int value)
    {
        for (int i = 0; i < globalVariableList.Count; i++)
        {
            if (globalVariableList[i].name.ToLower() == variableName.ToLower())
            {
                globalVariableList[i].intVal = value;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sets the float value of the global variable. Returns false if variable not found, ignoring case
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetGlobalVariableFloatValue(string variableName, float value)
    {
        for (int i = 0; i < globalVariableList.Count; i++)
        {
            if (globalVariableList[i].name.ToLower() == variableName.ToLower())
            {
                globalVariableList[i].floatVal = value;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Sets the string value of the global variable. Returns false if variable not found, ignoring case
    /// </summary>
    /// <param name="variableName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetGlobalVariableStringValue(string variableName, string value)
    {
        for (int i = 0; i < globalVariableList.Count; i++)
        {
            if (globalVariableList[i].name.ToLower() == variableName.ToLower())
            {
                globalVariableList[i].stringVal = value;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the bool value of the global variable. Returns false if variable is not found
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public bool GetGlobalVariableBoolValue(string variableName)
    {
        for (int i = 0; i < globalVariableList.Count; i++)
        {
            if (globalVariableList[i].name.ToLower() == variableName.ToLower())
            {
                return globalVariableList[i].boolVal;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the bool value of the global variable. Returns -1 if variable is not found
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public int GetGlobalVariableIntValue(string variableName)
    {
        for (int i = 0; i < globalVariableList.Count; i++)
        {
            if (globalVariableList[i].name.ToLower() == variableName.ToLower())
            {
                return globalVariableList[i].intVal;
            }
        }

        return -1;
    }

    /// <summary>
    /// Returns the bool value of the global variable. Returns -1f if variable is not found
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public float GetGlobalVariableFloatValue(string variableName)
    {
        for (int i = 0; i < globalVariableList.Count; i++)
        {
            if (globalVariableList[i].name.ToLower() == variableName.ToLower())
            {
                return globalVariableList[i].floatVal;
            }
        }

        return -1f;
    }

    /// <summary>
    /// Returns the string value of the global variable. Returns NULL if variable is not found
    /// </summary>
    /// <param name="variableName"></param>
    /// <returns></returns>
    public string GetGlobalVariableStringValue(string variableName)
    {
        for (int i = 0; i < globalVariableList.Count; i++)
        {
            if (globalVariableList[i].name.ToLower() == variableName.ToLower())
            {
                return globalVariableList[i].stringVal;
            }
        }

        return "NULL";
    }
}

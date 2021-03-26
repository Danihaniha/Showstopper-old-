//Code by Zach Phillips for the EZPZ Branching Dialogue toolset
//Used tutorial for basic window set up: https://gram.gs/gramlog/creating-node-based-editor-unity/
//All other code is copyright 2020, Zach Phillips, All rights reserved
using System;
using UnityEngine;
using UnityEditor;

public class ConnectionLine {

    public ConnectionPoint inPoint;
    public ConnectionPoint outPoint;

    public Node inNode;
    public Node outNode;

    public bool isFlowLine = false;

    //An Action delegate is put simply an encapsulated method that returns no value, or a void encapsulated method
    //https://answers.unity.com/questions/335953/what-is-systemaction.html
    public Action<ConnectionLine> OnClickRemoveConnection;

    public ConnectionLine(ConnectionPoint inPoint, ConnectionPoint outPoint, Node inNode, Node outNode, Action<ConnectionLine> OnClickRemoveConnection)
    {
        this.inPoint = inPoint;
        this.outPoint = outPoint;
        this.inNode = inNode;
        this.outNode = outNode;
        this.OnClickRemoveConnection = OnClickRemoveConnection;
    }

    //Draw the line between connection points using a bezier curve
    public void Draw()
    {
        //Draw curve
        if (isFlowLine)
            Handles.DrawBezier(
                inPoint.rect.center,
                outPoint.rect.center,
                inPoint.rect.center + Vector2.left * 50f,
                outPoint.rect.center - Vector2.left * 50f,
                Color.cyan,
                null,
                5f);
        else
            Handles.DrawBezier(
                inPoint.rect.center,
                outPoint.rect.center,
                inPoint.rect.center + Vector2.left * 50f,
                outPoint.rect.center - Vector2.left * 50f,
                Color.white,
                null,
                3f);

        //If handle clicked remove connection
        if (Handles.Button((inPoint.rect.center + outPoint.rect.center) * 0.5f, Quaternion.identity, 4, 8, Handles.RectangleHandleCap))
        {
            if (OnClickRemoveConnection != null)
            {
                OnClickRemoveConnection(this);
            }
        }
    }
}

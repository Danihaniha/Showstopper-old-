//Code by Zach Phillips for the EZPZ Branching Dialogue toolset
//Used tutorial for basic window set up: https://gram.gs/gramlog/creating-node-based-editor-unity/
//All other code is copyright 2020, Zach Phillips, All rights reserved
using System;
using UnityEngine;
public enum ConnectionPointType { In, Out }

public class ConnectionPoint {

    public Rect rect;

    public ConnectionPointType type;

    public Node node;

    public GUIStyle style;

    //An Action delegate is put simply an encapsulated method that returns no value, or a void encapsulated method
    //https://answers.unity.com/questions/335953/what-is-systemaction.html
    public Action<ConnectionPoint> OnClickConnectionPoint;

    //Constructor for connection point
    public ConnectionPoint(Node node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint)
    {
        this.node = node;
        this.type = type;
        this.style = style;
        this.OnClickConnectionPoint = OnClickConnectionPoint;
        rect = new Rect(0, 0, 10f, 20f);
    }

    //Draw connection points on nodes
    public void Draw(float zoomScale)
    {
        rect.y = node.rect.y + (node.rect.height * 0.5f * zoomScale) - rect.height * 0.5f;

        switch (type)
        {
            case ConnectionPointType.In:
                rect.x = node.rect.x - rect.width + 8f;
                break;

            case ConnectionPointType.Out:
                rect.x = node.rect.x + (node.rect.width * zoomScale) - 8f;
                break;
        }

        Rect scaledRect = rect;
        scaledRect.size *= zoomScale;

        if (GUI.Button(scaledRect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                OnClickConnectionPoint(this);
            }
        }
    }

    public void Draw(int vertOffset, float zoomScale)
    {
        rect.y = node.rect.y + (node.rect.height * 0.5f * zoomScale) - rect.height * 0.5f - vertOffset;

        switch (type)
        {
            case ConnectionPointType.In:
                rect.x = node.rect.x - rect.width + 8f;
                break;

            case ConnectionPointType.Out:
                rect.x = node.rect.x + (node.rect.width * zoomScale) - 8f;
                break;
        }

        Rect scaledRect = rect;
        scaledRect.size *= zoomScale;

        if (GUI.Button(scaledRect, "", style))
        {
            if (OnClickConnectionPoint != null)
            {
                OnClickConnectionPoint(this);
            }
        }
    }
}

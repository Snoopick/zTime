using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoursorManager : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Texture2D defaultCursorTexture;
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;
    [SerializeField] private Vector2 hotSpot = Vector2.zero;
    [SerializeField] private bool setOnStart = false;
    
    void OnMouseEnter()
    {
        CursorMode mode = CursorMode.ForceSoftware;
        Vector2 hotSpot = new Vector2(0,0);
        Cursor.SetCursor(cursorTexture, hotSpot, mode);
    }

    void OnMouseExit()
    {
        CursorMode mode = CursorMode.ForceSoftware;
        Vector2 hotSpot = new Vector2(0,0);
        Cursor.SetCursor(defaultCursorTexture, hotSpot, mode);
    }

    private void Start()
    {
        if (setOnStart)
        {
            CursorMode mode = CursorMode.ForceSoftware;
            Vector2 hotSpot = new Vector2(0,0);
            Cursor.SetCursor(defaultCursorTexture, hotSpot, mode);
        }
    }
}

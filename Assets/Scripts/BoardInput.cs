using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// A single pointer path handles mouse and real browser touch events.
public class BoardInput : MonoBehaviour
{
    private GameManager manager;
    private Camera boardCamera;
    private float lastTouchTime = -1;
    private readonly List<RaycastResult> uiHits = new List<RaycastResult>();

    public void Initialize(GameManager game)
    {
        manager = game;
        boardCamera = Camera.main;
        Input.simulateMouseWithTouches = false;
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            lastTouchTime = Time.unscaledTime;
            Touch touch = Input.GetTouch(0);
            if (Input.touchCount == 1 && touch.phase == TouchPhase.Began) Press(touch.position);
            return;
        }
        // Some browsers synthesize a mouse event after a tap.
        if (Time.unscaledTime - lastTouchTime > 0.25f && Input.GetMouseButtonDown(0))
            Press(Input.mousePosition);
    }

    public bool Press(Vector2 screenPosition)
    {
        if (manager == null || boardCamera == null) return false;
        if (EventSystem.current != null)
        {
            uiHits.Clear();
            var pointer = new PointerEventData(EventSystem.current) { position = screenPosition };
            EventSystem.current.RaycastAll(pointer, uiHits);
            if (uiHits.Count > 0) return false;
        }
        var hit = Physics2D.GetRayIntersection(boardCamera.ScreenPointToRay(screenPosition));
        if (hit.collider == null) return false;
        var restart = hit.collider.GetComponent<RestartGameInput>();
        if (restart != null) { restart.Activate(); return true; }
        if (!manager.myTurn) return false;
        var column = hit.collider.GetComponent<Hit>();
        return column != null && manager.TryPlayColumn(column.Column);
    }
}



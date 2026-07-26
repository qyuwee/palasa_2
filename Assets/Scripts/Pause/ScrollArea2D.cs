using UnityEngine;
using UnityEngine.InputSystem;

public class ScrollArea2D : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 300f;
    public float minY = -200f;
    public float maxY = 200f;

    private Mouse mouse;

    void Start()
    {
        mouse = Mouse.current;
    }

    void Update()
    {
        if (mouse == null) return;

        float scroll = mouse.scroll.ReadValue().y;

        if (scroll != 0)
        {
            Vector3 pos = transform.localPosition;

            // ИСПРАВЛЕНИЕ: Используем unscaledDeltaTime, чтобы скроллинг работал во время паузы!
            pos.y += scroll * scrollSpeed * Time.unscaledDeltaTime;

            pos.y = Mathf.Clamp(pos.y, minY, maxY);
            transform.localPosition = pos;
        }
    }
}
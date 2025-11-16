using UnityEngine;
using UnityEngine.InputSystem;

public class ScrollArea2D : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 300f;      // скорость в пикселях
    public float minY = -200f;            // нижняя граница
    public float maxY = 200f;             // верхняя граница

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

            // Плавное движение
            pos.y += scroll * scrollSpeed * Time.deltaTime;

            // Ограничиваем движение
            pos.y = Mathf.Clamp(pos.y, minY, maxY);

            transform.localPosition = pos;
        }
    }
}

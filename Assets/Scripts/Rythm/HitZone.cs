using UnityEngine;

public class HitZone : MonoBehaviour
{
    // Дублируем перечисление для явной настройки в инспекторе
    public enum ZoneType { Left_SD, Right_KL }

    [Header("Тип управляемого кружка")]
    public ZoneType zoneType;
}
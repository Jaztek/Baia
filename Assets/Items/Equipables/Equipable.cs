
using UnityEngine;

public abstract class Equipable : MonoBehaviour
{
    public EquipableId itemId;
    public Sprite itemSprite;
    protected Vector2 currentPosition;

    // Método llamado cuando empieza la acción del ítem
    public abstract void OnActionStart(Vector2 position);

    // Método llamado cuando deja de usarse el ítem
    public virtual void OnActionEnd() { }

    // Actualiza la posición del ítem
    public void SetCurrentPosition(Vector2 position)
    {
        currentPosition = position;
    }
}

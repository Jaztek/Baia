using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Combat parameters")]
    public float maxHp = 20f;
    public float damage = 3f;
    public GameObject currentHpBar;

    [Space(5)]

    [Header("Death parameters")]

    [SerializeField]
    float deathForceMultiplier = 0f;

    [SerializeField]
    float deathAdditionalTorque = 0f;

    [SerializeField]
    [ReadOnly]
    float currentHp;

    // Ítem en el slot0 (rápido)
    Equipable itemAt0 = null;
    // Ítem en el slot1 (preciso)
    Equipable itemAt1 = null;

    bool item0Active;
    bool item1Active;

    Rigidbody2D rb;

    // Mapea los touchId con el slot correspondiente
    private Dictionary<int, int> mouseInfoToSlot;

    void Awake()
    {
        currentHp = maxHp;
        rb = GetComponent<Rigidbody2D>();
        mouseInfoToSlot = new Dictionary<int, int>();
    }

    // Asigna un ítem a un determinado slot
    public void Equip(EquipableId equipableId, int slot)
    {
        Equipable current = (slot == 0) ? itemAt0 : itemAt1;

        if (current)
        {
            if (current.itemId == equipableId)
            {
                // Si ya tenemos equipado este item en el slot correspondiente no hacemos nada
                return;
            }
        }

        // Obtenemos una nueva instancia del equipable de la base de datos y lo asociamos a este jugador
        Equipable newEquipable = ItemsDataBase.GetInstance().GetEquipable(equipableId);
        newEquipable.transform.SetParent(transform, false);

        if (slot == 0)
        {
            itemAt0 = newEquipable;
        }
        else
        {
            itemAt1 = newEquipable;
        }
    }

    public bool IsActive(int slot)
    {
        if (slot == 0)
        {
            return item0Active;
        }
        else
        {
            return item1Active;
        }
    }

    // El InputController llama a este método cuando comienza a usarse un ítem.
    public void OnUsingItemStart(int slot, int mouseId, Vector2 mousePosition)
    {
        if (mouseInfoToSlot.ContainsKey(mouseId)) { return; }

        Equipable e = (slot == 0) ? itemAt0 : itemAt1;
        if (!e)
        {
            // Si no hay ítem equipado no hacemos nada
            Debug.Log("NO ITEM IN SLOT " + slot);
            return;
        }

        // Registramos en la map que el mouseId se asocia a este slot
        mouseInfoToSlot.Add(mouseId, slot);
        // Llamamos al OnActionStart del ítem
        e.OnActionStart(mousePosition);

        if (slot == 0)
        {
            item0Active = true;
        }
        else
        {
            item1Active = true;
        }
    }

    // El InputController llama a este método cuando está usándose un ítem
    public void OnUsingItemHold(int mouseId, Vector2 position)
    {
        // Condición de control por si por alguna condición de carrera ya no está en esta ranura
        if (!mouseInfoToSlot.ContainsKey(mouseId))
        {
            return;
        }

        int slot = mouseInfoToSlot[mouseId];
        Equipable e = (slot == 0) ? itemAt0 : itemAt1;

        // Actualizamos la posición almacenada en el ítem por si la necesita
        e.SetCurrentPosition(position);
    }

    // El InputController llama a este método cuando deja de usarse un ítem
    public void OnUsingItemStop(int mouseId)
    {
        if (!mouseInfoToSlot.ContainsKey(mouseId))
        {
            return;
        }

        int slot = mouseInfoToSlot[mouseId];
        Equipable e = (slot == 0) ? itemAt0 : itemAt1;
        // Llamamos al OnActionEnd del ítem y lo eliminamos de la map de touches
        mouseInfoToSlot.Remove(mouseId);
        e.OnActionEnd();

        if (slot == 0)
        {
            item0Active = false;
        }
        else
        {
            item1Active = false;
        }
    }

    private void ReceiveDamage(DamageData damageData)
    {
        float ammount = damageData.ammount;
        Vector2 direction = damageData.direction;

        currentHp = Mathf.Clamp(currentHp - ammount, 0, maxHp);
        float newXScale = currentHp / maxHp;

        // De momento desactivo la aplicación de fuerzas - hay un bug raro que no consigo localizar

        Vector2 forceVector = ammount * direction / rb.mass;
        if (currentHp == 0)
        {
            CharacterController2D cc = GetComponent<CharacterController2D>();
            if (cc)
            {
                cc.MovementDisabled = true;
            }
            else
            {
                Debug.Log("CharacterController2D not found in Player.cs");
            }

            forceVector = forceVector * deathForceMultiplier;
            rb.freezeRotation = false;
            rb.AddTorque(-Mathf.Sign(direction.x) * deathAdditionalTorque);
        }
        currentHpBar.transform.localScale = new Vector3(newXScale, currentHpBar.transform.localScale.y, currentHpBar.transform.localScale.z);
        rb.AddForce(forceVector, ForceMode2D.Impulse);
    }
}

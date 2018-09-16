using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum EquipableId
{
    BOW, TRANSLOCATOR
}

public class ItemsDataBase : MonoBehaviour
{
    public List<Equipable> values;
    private Dictionary<EquipableId, Equipable> weaponPrefabsCache;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }

    void Start()
    {
        weaponPrefabsCache = new Dictionary<EquipableId, Equipable>();

        for (int i = 0; i < values.Count; i++)
        {
            weaponPrefabsCache.Add(values[i].itemId, values[i]);
        }
    }

    private static ItemsDataBase instance;

    public static ItemsDataBase GetInstance()
    {
        return instance;
    }

    public Equipable GetEquipable(EquipableId id)
    {
        return Instantiate(weaponPrefabsCache[id]);
    }

    public Sprite GetSprite(EquipableId id)
    {
        return weaponPrefabsCache[id].itemSprite;
    }
}

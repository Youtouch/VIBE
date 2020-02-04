using System;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[System.Serializable]
public class ObjectPool<T>  where T : Component
{
    private int actualSize;
    [SerializeField] [HideInInspector] protected T[] pool = new T[0], rented = new T[0];

    [SerializeField,FoldoutGroup("Object pooling settings")]
    public int poolSize;

    [SerializeField,FoldoutGroup("Object pooling settings")]
    public GameObject prefab;

    public ObjectPool(int poolSize, GameObject prefab)
    {
        this.poolSize = poolSize;
        this.prefab = prefab;
    }

    public T[] rentedObjects => rented.Where(x => x != null).ToArray();
    public T[] allObjects => pool.Where(x => x != null).ToArray();

    private int m_ReturnedIndex = 0;

    private bool m_Setup = false;

    public bool initialized
    {
        get
        {
            if(!m_Setup)
                Setup();
            return m_Setup;
        }
    }

    public void Setup()
    {
        m_Setup = true;
        pool = new T[poolSize];
        rented = new T[poolSize];
    }
    public T Rent()
    {
        if(!m_Setup)
            Setup();
        if (actualSize > 0)
        {
            for (var i = 0; i < poolSize; i++)
                if (pool[i] != null)
                {
                    var item = pool[i];
                    pool[i] = null;
                    rented[i] = item;

                    item.gameObject.SetActive(true);
                    return item;
                }

            if (actualSize >= poolSize)
            {
//                Debug.LogError("No vacancy in pool: " + gameObject.name);
                return ReturnFirst();
            }

            return Create();
        }

        return Create();
    }

    public T Create()
    {
        var newGo = MonoBehaviour.Instantiate(prefab);
        T newItem;
        if (newGo.GetComponent<T>())
            newItem = newGo.GetComponent<T>();
        else
            newItem = newGo.AddComponent<T>();
        rented[actualSize] = newItem;
        actualSize++;
        newItem.gameObject.SetActive(true);
        return newItem;
    }

    public void Return(T item)
    {
        if (!rented.Contains(item) || item == null) return;
        var id = Array.FindIndex(rented, row => row == item);
        rented[id] = null;
        pool[id] = item;
//        Debug.Log(item.gameObject.name+" is supposed to be returned");
        item.gameObject.SetActive(false);
    }
    public void Return(T item, Transform parent)
    {
        if (!rented.Contains(item) || item == null) return;
        var id = Array.FindIndex(rented, row => row == item);
        rented[id] = null;
        pool[id] = item;
//        Debug.Log(item.gameObject.name+" is supposed to be returned");
        item.transform.SetParent(parent);
        item.gameObject.SetActive(false);
    }
    public void Reset()
    {
        if(!m_Setup)
            Setup();
        foreach (var obj in rented.Where(x=> x!= null))
        {
            if (obj == null) continue;
            Return(obj);
        }
    }
    public void Reset(Transform newParent)
    {
        if(!m_Setup)
            Setup();
        foreach (var obj in rented.Where(x=> x!= null))
        {
            if (obj == null) continue;
            obj.transform.SetParent(newParent);
            Return(obj);
        }
    }

    public T ReturnFirst()
    {
        Return(rented[m_ReturnedIndex]);
        m_ReturnedIndex++;
        if (m_ReturnedIndex >= poolSize)
            m_ReturnedIndex = 0;
        return Rent();
    }
}
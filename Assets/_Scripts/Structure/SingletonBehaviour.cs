using Sirenix.OdinInspector;
using UnityEngine;

public class SingletonBehaviour<T> : SerializedMonoBehaviour where T : SingletonBehaviour<T>
{
    public static T instance { get; protected set; }

    internal virtual void Awake()
    {
        if (instance == this)
            return;
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
//            throw new System.Exception("An instance of this singleton already exists.");
        }
        else
        {
            instance = (T) this;
            if (!gameObject.transform.parent)
                DontDestroyOnLoad(gameObject);
//            Debug.LogWarning("Created an instance of  : " + instance);
        }
    }
}
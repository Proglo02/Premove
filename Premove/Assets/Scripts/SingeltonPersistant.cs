using UnityEngine;

public abstract class SingeltonPersistant<T> : MonoBehaviour where T : MonoBehaviour
{
    protected virtual bool autoCreate => false;
    protected virtual bool persistant => false;

    private static T instance;

    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                instance = FindAnyObjectByType<T>();

                if(instance == null)
                {
                    CreateInstance();
                }
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;

            if(persistant)
                DontDestroyOnLoad(gameObject);
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

    }

    private static void CreateInstance()
    {
        GameObject obj = new GameObject(typeof(T).Name);
        instance = obj.AddComponent<T>();

        if(IsPersistant())
            DontDestroyOnLoad(obj);
    }

    private static bool IsPersistant()
    {
        var temp = new GameObject("TempCheck").AddComponent<T>() as SingeltonPersistant<T>;
        bool value = temp.persistant;
        DestroyImmediate(temp.gameObject);
        return value;
    }
}

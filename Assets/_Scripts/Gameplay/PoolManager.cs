using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<int, Queue<ObjectInstance>> poolDictionary = new Dictionary<int, Queue<ObjectInstance>>();

    public void CreatePool (GameObject prefab, int poolSize)
    {
        GameObject poolHolder = new GameObject($"{prefab.name} havuzu");
        poolHolder.transform.parent = transform;
        CreatePool(prefab, poolSize, poolHolder.transform);
    }

    public void CreatePool (GameObject prefab, int poolSize, Transform parent)
    {
        CreatePool(prefab,poolSize,parent,prefab.transform.position,prefab.transform.rotation);
    }

    public void CreatePool(GameObject prefab, int poolSize, Transform parent, Vector3 pos, Quaternion rot)
    {
        int poolKey = prefab.GetInstanceID();

        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<ObjectInstance>());

            for (int i = 0; i < poolSize; i++)
            {
                ObjectInstance newObject = new ObjectInstance(Instantiate(prefab,pos,rot, parent));
                poolDictionary[poolKey].Enqueue(newObject);
            }
        }
    }


    public T ReuseObject<T> (GameObject prefab) where T : PoolObject
    {
        return ReuseObject<T>(prefab, Vector3.zero, Quaternion.identity);
    }

    public T ReuseObject<T> (GameObject prefab, Vector3 position, Quaternion rotation) where T : PoolObject
    {
        int poolKey = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(poolKey))
        {
            ObjectInstance objectToReuse = poolDictionary[poolKey].Dequeue();
            poolDictionary[poolKey].Enqueue(objectToReuse);

            objectToReuse.Reuse(position, rotation);
            return objectToReuse.poolObject as T;
        }
        return null;
    }

    public class ObjectInstance
    {
        private GameObject gameObject;
        private Transform transform;

        private bool hasPoolObject;
        public PoolObject poolObject { get; private set; }

        public ObjectInstance (GameObject gameObject)
        {
            this.gameObject = gameObject;
            transform = gameObject.transform;
            gameObject.SetActive(false);

            if (gameObject.GetComponent<PoolObject>())
            {
                hasPoolObject = true;
                poolObject = gameObject.GetComponent<PoolObject>();
                //poolObject.isPoolItem = true;
            }
        }

        public void Reuse (Vector3 position, Quaternion rotation)
        {
            if (hasPoolObject)
            {
                poolObject.OnObjectReuseBeforeActivation();
            }

            gameObject.SetActive(true);
            if (hasPoolObject)
            {
                poolObject.OnObjectReused();
            }
            transform.SetPositionAndRotation(position, rotation);
        }
    }
}

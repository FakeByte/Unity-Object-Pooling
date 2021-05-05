using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectPool {

    private static Dictionary<int, Pool> pooledObjects = new Dictionary<int, Pool>();
    private static Dictionary<int, Pool> pools = new Dictionary<int, Pool>();

    public static GameObject Instantiate(GameObject prefab, Vector3 positon, Quaternion rotation, Transform parent = null) {
        Init(prefab);

        GameObject gameObject = pools[prefab.GetInstanceID()].Get(positon, rotation, parent);

        if (!pooledObjects.ContainsKey(gameObject.GetInstanceID())) {
            pooledObjects.Add(gameObject.GetInstanceID(), pools[prefab.GetInstanceID()]);
        }

        return gameObject;
    }

    public static void Destroy(GameObject gameObject) {

        if (pooledObjects.ContainsKey(gameObject.GetInstanceID())) {

            pooledObjects[gameObject.GetInstanceID()].Return(gameObject);

        } else {

#if (DEVELOPMENT_BUILD || UNITY_EDITOR)
            Debug.LogWarning("Tried to destroy non pooled object " + gameObject.name);
#endif
            Destroy(gameObject);
        }
    }

    public static void Preload(GameObject prefab, int quantity) {

        if (quantity <= 0) quantity = 1;

        if (!pools.ContainsKey(prefab.GetInstanceID())) {

            pools.Add(prefab.GetInstanceID(), new Pool(prefab, quantity));

        } else {

            pools[prefab.GetInstanceID()].Preload(quantity);

        }

    }

    private static void Init(GameObject prefab) {
        
        if (!pools.ContainsKey(prefab.GetInstanceID())) {

            pools.Add(prefab.GetInstanceID(), new Pool(prefab));
            
        }

    }

    private class Pool {
        private GameObject prefab;
        private Stack<GameObject> objects;

        public Pool(GameObject prefab, int initialQuantity = 0) {

            if (initialQuantity < 0) initialQuantity = 0;

            this.prefab = prefab;

            objects = new Stack<GameObject>();

            if(initialQuantity != 0) {

                for(int i = 0; i < initialQuantity; i++) {
                    objects.Push(Instantiate(false, Vector3.zero, Quaternion.identity));
                }

            }

        }

        public GameObject Get(Vector3 positon, Quaternion rotation, Transform parent = null) {

            GameObject gameObject;
            if (objects.Count > 0) {
                gameObject = objects.Pop();
            } else {
                gameObject = Instantiate(true, positon, rotation, parent);
            }

            gameObject.SetActive(true);
            gameObject.transform.position = positon;
            gameObject.transform.rotation = rotation;
            gameObject.transform.parent = parent;

            return gameObject;

        }

        public void Return(GameObject gameObject) {

            gameObject.SetActive(false);

            objects.Push(gameObject);

        }

        public void Preload(int quantity) {

            for(int i = 0; i < quantity; i++) {
                Return(Instantiate(false, Vector3.zero, Quaternion.identity));
            }

        }

        private GameObject Instantiate(bool active, Vector3 positon, Quaternion rotation, Transform parent = null) {

            GameObject gameObject = GameObject.Instantiate(prefab, positon, rotation, parent);

            gameObject.SetActive(active);

            return gameObject;

        }
    }
}

public static class ObjectPoolGameObjectExtensions {
    public static void Destroy(this GameObject gameObject) {
        ObjectPool.Destroy(gameObject);
    }
}
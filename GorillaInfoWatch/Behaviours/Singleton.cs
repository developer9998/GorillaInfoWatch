using GorillaInfoWatch.Tools;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }
        public static bool HasInstance => Instance != null && (bool)Instance;

        public void Awake()
        {
            T component = gameObject.GetComponent<T>();

            if (HasInstance && Instance != component)
            {
                Destroy(component);
                return;
            }

            Instance = component;
            Initialize();
        }

        public virtual void Initialize()
        {
            Logging.Message($"{typeof(T).Name}: Singleton initializing");
        }

        public static bool TryGetInstance(out T instance)
        {
            instance = Instance;
            return HasInstance;
        }
    }
}
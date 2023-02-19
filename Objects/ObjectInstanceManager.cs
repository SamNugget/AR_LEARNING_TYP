using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ObjectInstances
{
    public class ObjectInstanceManager : MonoBehaviour
    {
        public static ObjectInstanceManager singleton;

        [SerializeField] private GameObject objectFab;

        // TODO: inject this into snippets code
        public static void createObjectInstance(object inMemory, Vector3 spawnPoint, Quaternion spawnRotation = default)
        {
            if (inMemory is int)         createObjectInstance<IntInstance>(inMemory, spawnPoint, spawnRotation);
            else if (inMemory is string) createObjectInstance<StringInstance>(inMemory, spawnPoint, spawnRotation);
            else if (inMemory is bool)   createObjectInstance<BoolInstance>(inMemory, spawnPoint, spawnRotation);
            else                         createObjectInstance<CustomTypeInstance>(inMemory, spawnPoint, spawnRotation);
        }

        public static void createObjectInstance<T>(object inMemory, Vector3 spawnPoint, Quaternion spawnRotation = default) where T : ObjectInstance
        {
            GameObject g = Instantiate(singleton.objectFab, spawnPoint, spawnRotation, singleton.transform);

            ObjectInstance oI = g.AddComponent<T>();
            oI.inMemory = inMemory;
        }

        void Awake()
        {
            if (singleton == null) singleton = this;
            else Debug.LogError("Two ObjectInstancesManager singletons.");
        }

        void Start()
        {
            createObjectInstance<IntInstance>(10, new Vector3(0.3f, 0.5f, 0f));
            createObjectInstance<StringInstance>("howdy", new Vector3(0.3f, 0.6f, 0f));
            createObjectInstance<CustomTypeInstance>(new Person(), new Vector3(0.3f, 0.7f, 0f));
            createObjectInstance<BoolInstance>(true, new Vector3(0.3f, 0.8f, 0f));
        }
    }



    public static class ObjectInstanceGetter
    {
        private static List<ObjectInstance> objectInstances = new List<ObjectInstance>();

        public static int getCustomTypeInstanceKey(ObjectInstance objectInstance)
        {
            if (!objectInstances.Contains(objectInstance))
                objectInstances.Add(objectInstance);

            return objectInstances.IndexOf(objectInstance);
        }

        public static object getCustomTypeInstance(int key)
        {
            if (key < 0 || key >= objectInstances.Count || objectInstances[key] == null)
                return null;

            return objectInstances[key].inMemory;
        }
    }



    public abstract class ObjectInstance : Cube
    {
        private object _inMemory;
        public object inMemory
        {
            get
            {
                return _inMemory;
            }
            set
            {
                _inMemory = value;
                setup();
            }
        }

        public abstract string getLabel();
        public abstract string getInspectText();
        public abstract string toCode();

        protected virtual void setup()
        {
            TextMeshPro[] textBoxes = GetComponentsInChildren<TextMeshPro>();
            foreach (TextMeshPro textBox in textBoxes)
                textBox.text = getLabel();
        }
    }



    // CUSTOM TYPES
    public class CustomTypeInstance : ObjectInstance
    {
        private static Dictionary<Type, Color> colours = new Dictionary<Type, Color>();

        public override string getLabel()
        {
            return inMemory.GetType().Name;
        }

        public override string getInspectText()
        {
            string json = JsonUtility.ToJson(inMemory, true);
            string[] lines = json.Split('\n');
            
            json = "";
            for (int i = 1; i < lines.Length - 1; i++)
                json += lines[i].Substring(4) + '\n';

            return json;
        }

        public override string toCode()
        {
            return "ObjectInstanceGetter.getCustomTypeInstance(" + ObjectInstanceGetter.getCustomTypeInstanceKey(this) + ')';
        }

        protected override void setup()
        {
            base.setup();


            Type t = inMemory.GetType();

            // get colour for this custom type
            if (!colours.ContainsKey(t))
            {
                // generate random colour
                float r = 0f, g = 0f, b = 0f, sum = 0f;
                while (sum < 0.5f)
                {
                    r = UnityEngine.Random.Range(0f, 1f);
                    g = UnityEngine.Random.Range(0f, 1f);
                    b = UnityEngine.Random.Range(0f, 1f);
                    sum = r + g + b;
                }
                colours.Add(t, new Color(r, g, b, 1f));
            }

            _colour = colours[t];
        }
    }

    // test
    [System.Serializable]
    public class Person
    {
        public int age = 22;
        public int height = 150;
        public bool married = false;
    }



    // BUILT-IN TYPES
    public abstract class BuiltInTypeInstance : ObjectInstance
    {
        public override string getLabel()
        {
            return "" + inMemory;
        }

        public override string toCode()
        {
            return "" + inMemory;
        }
    }

    public class IntInstance : BuiltInTypeInstance
    {
        public override string getInspectText()
        {
            return "int: " + (int)inMemory;
        }

        void Start()
        {
            _colour = Color.blue;
            _size = 8;
        }
    }

    public class StringInstance : BuiltInTypeInstance
    {
        public override string getLabel()
        {
            return '\"' + (string)inMemory + '\"';
        }

        public override string getInspectText()
        {
            return "string: \"" + (string)inMemory + '\"';
        }

        public override string toCode()
        {
            return '\"' + (string)inMemory + '\"';
        }

        void Start()
        {
            _colour = Color.magenta;
        }
    }

    public class ErrInstance : StringInstance
    {
        public override string getLabel()
        {
            return "ERR";
        }

        void Start()
        {
            _colour = Color.red;
        }
    }

    public class BoolInstance : BuiltInTypeInstance
    {
        public override string getInspectText()
        {
            return "bool: " + (bool)inMemory;
        }

        void Start()
        {
            _colour = Color.cyan;
        }
    }



    // VISUALISING
    public class Cube : MonoBehaviour
    {
        public int _size
        {
            set
            {
                // set size of object
                float s = value / 10f;
                transform.localScale = new Vector3(s, s, s);
            }
        }

        public Color _colour
        {
            set
            {
                // set the colour of the object
                GetComponentInChildren<Renderer>().material.color = value;
            }
        }
    }
}

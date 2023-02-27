using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// temp
using System.Reflection;

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
            // temp
            createObjectInstance<IntInstance>(10, new Vector3(0.3f, -1.45f, 0f));
            createObjectInstance<StringInstance>("howdy", new Vector3(0.3f, -1.35f, 0f));
            createObjectInstance<CustomTypeInstance>(new Person(), new Vector3(0.3f, -1.25f, 0f));
            createObjectInstance<BoolInstance>(true, new Vector3(0.3f, -1.15f, 0f));

            //Debug.Log(Assembly.GetAssembly(this.GetType()).GetName().Name);
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
        private static Dictionary<string, Color> colours = new Dictionary<string, Color>();

        public override string getLabel()
        {
            return inMemory.GetType().Name;
        }

        public override string getInspectText()
        {
            string json = JsonUtility.ToJson(inMemory, true);
            string[] lines = json.Split('\n');
            
            json = getLabel() + '(' + inMemory.GetHashCode() + ")\n";
            for (int i = 1; i < lines.Length - 1; i++)
                json += lines[i].Substring(4) + '\n';

            return json;
        }

        protected override void setup()
        {
            base.setup();


            //string typeName = ((CustomType)inMemory).name;
            string typeName = inMemory.GetType().Name;

            // get colour for this custom type
            if (!colours.ContainsKey(typeName))
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
                colours.Add(typeName, new Color(r, g, b, 1f));
            }

            _colour = colours[typeName];
        }
    }

    /*public class CustomType
    {
        public string name;
        public List<Field> fields;

        public class Field
        {
            public string name;
            public object value;
        }
    }*/

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

        public override string getInspectText()
        {
            return inMemory.GetType().Name + ": " + inMemory;
        }
    }

    public class IntInstance : BuiltInTypeInstance
    {
        protected override void setup()
        {
            base.setup();

            _colour = Color.blue;
            _size = 8;
        }
    }

    public class BoolInstance : BuiltInTypeInstance
    {
        protected override void setup()
        {
            base.setup();

            _colour = Color.cyan;
            _size = 7;
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
            return "String: \"" + (string)inMemory + '\"';
        }

        protected override void setup()
        {
            base.setup();

            _colour = Color.magenta;
            _size = 9;
        }
    }

    public class ErrInstance : BuiltInTypeInstance
    {
        public override string getLabel()
        {
            return "ERR";
        }

        public override string getInspectText()
        {
            return "" + inMemory;
        }

        protected override void setup()
        {
            base.setup();

            _colour = Color.red;

            Debug.Log(inMemory);
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

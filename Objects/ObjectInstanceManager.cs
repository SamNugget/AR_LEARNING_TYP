using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace ObjectInstances
{
    public class ObjectInstanceManager : MonoBehaviour
    {
        [SerializeField] private GameObject objectFab;

        public void createObjectInstance<T>(object inMemory, Vector3 spawnPoint, Quaternion spawnRotation = default) where T : ObjectInstance
        {
            GameObject g = Instantiate(objectFab, spawnPoint, spawnRotation, transform);

            ObjectInstance oI = g.AddComponent<T>();
            oI.inMemory = inMemory;
        }

        void Start()
        {
            createObjectInstance<IntInstance>(10, new Vector3(0.3f, 0f, 0f));
            createObjectInstance<StringInstance>("howdy", new Vector3(0.3f, 0.1f, 0f));
            createObjectInstance<CustomTypeInstance>(new Person(), new Vector3(0.3f, 0.2f, 0f));
        }
    }



    public static class ObjectInstanceGetter
    {
        private static List<CustomTypeInstance> customTypeInstances = new List<CustomTypeInstance>();

        public static int getCustomTypeInstanceKey(CustomTypeInstance customTypeInstance)
        {
            if (!customTypeInstances.Contains(customTypeInstance))
                customTypeInstances.Add(customTypeInstance);

            return customTypeInstances.IndexOf(customTypeInstance);
        }

        public static object getCustomTypeInstance(int key)
        {
            if (key < 0 || key >= customTypeInstances.Count || customTypeInstances[key] == null)
                return null;

            return customTypeInstances[key].inMemory;
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
            return "" + (int)inMemory;
        }
    }

    public class IntInstance : BuiltInTypeInstance
    {
        public override string getInspectText()
        {
            return "int: " + (int)inMemory;
        }

        public override string toCode()
        {
            return "" + (int)inMemory;
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
            _colour = Color.red;
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

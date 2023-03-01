using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using FileManagement;
using System.Reflection;
using System.Text;

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
            createObjectInstance<IntInstance>(1, new Vector3(0.3f, -1.45f, 0f));
            createObjectInstance<IntInstance>(2, new Vector3(0.3f, -1.35f, 0f));
            createObjectInstance<StringInstance>("Hello!", new Vector3(0.3f, -1.25f, 0f));
            createObjectInstance<BoolInstance>(true, new Vector3(0.3f, -1.15f, 0f));

            //Debug.Log(Assembly.GetAssembly(this.GetType()).GetName().Name);
        }
    }



    public abstract class ObjectInstance : Cube
    {
        protected object _inMemory;
        public virtual object inMemory
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
        private static int idCounter = 0;
        private static Dictionary<string, Color> colours = new Dictionary<string, Color>();

        public override object inMemory
        {
            get
            {
                // convert to object
                return ((CustomType)_inMemory).ToObject();
            }
            set
            {
                // convert to CustomType
                int id = (_inMemory == null ? idCounter++ : ((CustomType)_inMemory).id);
                _inMemory = new CustomType(value, id);
                setup();
            }
        }

        public override string getLabel()
        {
            return ((CustomType)_inMemory).name;
        }

        public override string getInspectText()
        {
            /*string json = JsonUtility.ToJson(inMemory, true);
            string[] lines = json.Split('\n');
            
            json = getLabel() + '(' + inMemory.GetHashCode() + ")\n";
            for (int i = 1; i < lines.Length - 1; i++)
                json += lines[i].Substring(4) + '\n';

            return json;*/

            return ((CustomType)_inMemory).ToString();
        }

        protected override void setup()
        {
            base.setup();


            string typeName = ((CustomType)_inMemory)._name;
            //string typeName = inMemory.GetType().Name;

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

    public class CustomType
    {
        public string _name;
        public int id;

        public string name
        {
            get
            {
                return _name + "(id" + id + ')';
            }
        }

        //                name    value
        private Dictionary<string, object> fields;
        // TODO: currently doesn't work for composition



        public CustomType(object o, int id)
        {
            this.id = id;

            try
            {
                fields = new Dictionary<string, object>();

                Type t = o.GetType();
                _name = t.Name;

                FieldInfo[] fI = t.GetFields();
                foreach (FieldInfo f in fI)
                {
                    string fieldName = f.Name;
                    object value = f.GetValue(o);
                    fields.Add(fieldName, value);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Issue converting from type " + (_name == null ? "null" : _name) + " to CustomType.");
                Debug.Log(e.StackTrace);
            }
        }

        public object ToObject()
        {
            try
            {
                Type t = CompilationManager.lastASM.GetType(_name, true, false);

                object o = CompilationManager.lastASM.CreateInstance(_name);

                foreach (string fieldName in fields.Keys)
                {
                    try
                    {
                        FieldInfo fI = t.GetField(fieldName);
                        fI.SetValue(o, fields[fieldName]);
                    }
                    catch
                    {
                        fields.Remove(fieldName);
                    }
                }

                return o;
            }
            catch (Exception e)
            {
                Debug.Log("Issue converting from object to CustomType.");
                Debug.Log(e.StackTrace);
                return null;
            }
        }

        public string ToString()
        {
            StringBuilder src = new StringBuilder();
            
            src.AppendLine(name + '\n');

            foreach (string fieldName in fields.Keys)
                src.AppendLine(fieldName + ": " + fields[fieldName]);

            return src.ToString();
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
            if (((string)inMemory).Length > 8)
                return "String";
            else
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

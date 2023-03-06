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

        [SerializeField] private List<GameObject> objectFabs;
        [SerializeField] private GameObject defaultObjectFab;

        private GameObject getObjectFab(string name)
        {
            name = name.ToLower();

            foreach (GameObject g in objectFabs)
                if (g.name.ToLower() == name)
                    return g;
            return defaultObjectFab;
        }

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
            GameObject g = Instantiate(singleton.getObjectFab(inMemory.GetType().Name), spawnPoint, spawnRotation, singleton.transform);

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
            /*createObjectInstance<IntInstance>(1, new Vector3(0.3f, -1.45f, 0f));
            createObjectInstance<IntInstance>(2, new Vector3(0.3f, -1.35f, 0f));
            createObjectInstance<StringInstance>("Hello!", new Vector3(0.3f, -1.25f, 0f));
            createObjectInstance<BoolInstance>(true, new Vector3(0.3f, -1.15f, 0f));*/
        }
    }



    public abstract class ObjectInstance : MonoBehaviour
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

        protected Skin skin;

        public abstract string getLabel();
        public abstract string getInspectText();

        protected virtual void setup()
        {
            skin = GetComponent<Skin>();
            skin.setProperty("_name", getLabel());
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
                _inMemory = value;
                setup();
            }
        }

        public override string getLabel()
        {
            return ((CustomType)_inMemory).name;
        }

        public override string getInspectText()
        {
            return ((CustomType)_inMemory).ToString();
        }

        protected override void setup()
        {
            string typeName = _inMemory.GetType().Name;

            // get colour for this custom type, must be before CustomType construction
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


            skin = GetComponent<Skin>();
            skin.setProperty("_colour", colours[typeName]);


            // convert to CustomType
            int id = (_inMemory is CustomType ? ((CustomType)_inMemory).id : idCounter++);
            _inMemory = new CustomType(_inMemory, id, skin);


            skin.setProperty("_name", getLabel());
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



        public CustomType(object o, int id, Skin skin)
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

                    skin.setProperty(fieldName, value);
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

            skin.setProperty("colour", "blue");
            skin.setProperty("size", 8);
        }
    }

    public class BoolInstance : BuiltInTypeInstance
    {
        protected override void setup()
        {
            base.setup();

            skin.setProperty("colour", "cyan");
            skin.setProperty("size", 7);
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

            skin.setProperty("colour", "magenta");
            skin.setProperty("size", 9);
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

            skin.setProperty("colour", "red");

            Debug.Log(inMemory);
        }
    }



    // VISUALISING
    public abstract class Skin : MonoBehaviour
    {
        public void setProperty(string name, object value)
        {
            name = name.ToLower();

            Type t = this.GetType();
            PropertyInfo[] pI = t.GetProperties();
            foreach (PropertyInfo p in pI)
            {
                if (p.Name == name)
                {
                    try
                    {
                        p.SetValue(this, value);
                    }
                    catch { }
                    return;
                }
            }
        }



        public string _name
        {
            set
            {
                TextMeshPro[] textBoxes = GetComponentsInChildren<TextMeshPro>();
                foreach (TextMeshPro textBox in textBoxes)
                    textBox.text = value;
            }
        }

        public virtual Color _colour
        {
            set
            {
                GetComponentInChildren<Renderer>().material.color = value;
            }
        }

        public string color { set { colour = value; } }
        public virtual string colour
        {
            set
            {
                // set the colour of the object
                GetComponentInChildren<Renderer>().material.color = getColour(value);
            }
        }

        protected static Color getColour(string name)
        {
            name = name.ToLower();

            switch (name)
            {
                case "black": return Color.black;
                case "blue": return Color.blue;
                case "gray": case "grey": return Color.gray;
                case "green": return Color.green;
                case "red": return Color.red;
                case "yellow": return Color.yellow;
                case "pink": case "magenta": return Color.magenta;
                case "orange": return new Color(1f, 0.647f, 0f, 1f);
                case "cyan": return Color.cyan;
                default: return Color.white;
            }
        }
    }
}

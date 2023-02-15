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
            createObjectInstance<IntInstance>(10, new Vector3(1f, 0f, 0f));
            createObjectInstance<StringInstance>("howdy", new Vector3(-1f, 0f, 0f));
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

                TextMeshPro[] textBoxes = GetComponentsInChildren<TextMeshPro>();
                foreach (TextMeshPro textBox in textBoxes)
                    textBox.text = getLabel();
            }
        }

        public abstract string getLabel();
        public abstract string getInspectText();
        public abstract string toCode();
    }



    // CUSTOM TYPES
    public abstract class CustomTypeInstance : ObjectInstance
    {
        public override string getLabel()
        {
            return inMemory.GetType().Name;
        }

        public override string getInspectText()
        {
            return JsonUtility.ToJson(inMemory, true);
        }

        public override string toCode()
        {
            return "ObjectInstanceGetter.getCustomTypeInstance(" + ObjectInstanceGetter.getCustomTypeInstanceKey(this) + ')';
        }
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

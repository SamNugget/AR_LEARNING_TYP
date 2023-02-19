using System.Collections.Generic;

[System.Serializable]
public class ReferenceTypeS
{
    // class, interface, delegate or record
    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/reference-types

    // only class in this implementation

    // TODO: why am I saving path?
    public string path;
    public string name;

    public List<FieldS> fields;
    public List<MethodS> methods;

    // public bool locked;
    // public bool opened
    // public float[] position

    public ReferenceTypeS(string path, string name)
    {
        this.path = path;
        this.name = name;
        
        methods = new List<MethodS>();
        fields = new List<FieldS>();
    }

    public void save()
    {
        foreach (MethodS m in methods)
            m.save();

        foreach (FieldS f in fields)
            f.save();
    }

    public string getCode()
    {
        // first line
        string code = "public class " + name + "\n{\n\n";


        // fields
        foreach (FieldS f in fields)
            code += f.getCode();
        code += '\n';

        // methods
        foreach (MethodS m in methods)
            code += m.getCode(true) + '\n';


        code += '}';
        return code;
    }





    public void addField(Block fieldBlock)
    {
        fields.Add(new FieldS(fieldBlock));
    }

    public void removeField(Block fieldBlock)
    {
        foreach (FieldS f in fields)
        {
            if (f.fieldBlock == fieldBlock)
            {
                fields.Remove(f);
                return;
            }
        }
    }

    public void addMethod(Block methodDeclaration)
    {
        methods.Add(new MethodS(methodDeclaration));
    }

    public void removeMethod(Block methodDeclaration)
    {
        MethodS m = findMethodSave(methodDeclaration);
        methods.Remove(m);
    }

    public MethodS findMethodSave(Block b, bool dec = true)
    {
        foreach (MethodS m in methods)
        {
            if (dec && m.methodDeclaration == b)
                return m;
            else if (!dec && m.methodBodyMaster == b)
                return m;
        }
        return null;
    }
}

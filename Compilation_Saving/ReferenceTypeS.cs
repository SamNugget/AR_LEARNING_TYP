using System.Collections.Generic;
using System.Text;

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
    public void addField(Block fieldBlock) { fields.Add(new FieldS(fieldBlock)); }
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

    public List<MethodS> methods;
    public void addMethod(MethodS mS) { methods.Add(mS); }
    public void removeMethod(MethodS mS) { methods.Remove(mS); }

    // public bool locked;
    // public bool opened
    // public float[] position

    public ReferenceTypeS(string path, string name)
    {
        this.path = path;
        this.name = name;
        
        methods = new List<MethodS>();
        // add constructor method
        methods.Add(createConstructorS(name));

        fields = new List<FieldS>();
    }

    public static MethodS createConstructorS(string name)
    {
        MethodS constructor = new MethodS();

        BlockSave[] subblocks = new BlockSave[] {
            new BlockSave(BlockManager.createNameBlock(name)),
            new BlockSave(BlockManager.getBlockVariantIndex("Place Variable")) };
        constructor.methodDeclarationS = new BlockSave(BlockManager.getBlockVariantIndex("Constructor"), subblocks);

        // create body block (with splittable empty)
        int bVI = BlockManager.getBlockVariantIndex("Method Block");
        int eBVI = BlockManager.getBlockVariantIndex("EmptyV");
        constructor.methodBodyMasterS = new BlockSave(bVI, new BlockSave[] {
            new BlockSave(eBVI, new BlockSave[] {
                new BlockSave(0) }) });

        return constructor;
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
        StringBuilder src = new StringBuilder();
        src.AppendLine("public class " + name + "\n{\n");


        // fields
        foreach (FieldS f in fields)
            src.AppendLine(f.getCode());
        src.AppendLine();

        Block constructor = methods[0].methodDeclaration;
        bool defaultConstructor = constructor.getBlocksOfVariant(BlockManager.getBlockVariant("VariableH")).Count == 0;
        // if empty constructor does not exist, make one
        if (!defaultConstructor)
            src.AppendLine("public " + name + "() { }");

        // methods
        foreach (MethodS m in methods)
            src.AppendLine(m.getCode());


        src.Append('}');
        return src.ToString();
    }

    /*public MethodS findMethodSave(Block b)
    {
        foreach (MethodS m in methods)
            if (m.methodDeclaration == b || m.methodBodyMaster == b)
                return m;
        return null;
    }*/
}

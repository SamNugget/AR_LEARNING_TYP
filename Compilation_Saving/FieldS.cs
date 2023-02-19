using System;

[System.Serializable]
public class FieldS
{
    [NonSerialized] public Block fieldBlock;
    public BlockSave fieldBlockS;

    public FieldS(Block fieldBlock)
    {
        this.fieldBlock = fieldBlock;
    }

    public void save()
    {
        fieldBlockS = fieldBlock.saveBlock();
    }

    public string getCode()
    {
        if (fieldBlock == null)
            return null;

        return fieldBlock.getBlockText(true) + ";\n";
    }
}

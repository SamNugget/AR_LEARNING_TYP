using UnityEngine;

[System.Serializable]
public class BlockVariantS
{
    public int key;
    public string name;
    public string blockType;

    public float[] color;
    public bool splittableV;
    public bool splittableH;
    public bool deleteable;
    public bool editorOnly;

    public string[] lines;

    public BlockVariantS(int key, BlockManager.BlockVariant bV)
    {
        this.key = key;
        name = bV.getName();
        blockType = bV.getBlockType();

        Color c = bV.getColor();
        color = new float[] { c.r, c.g, c.b, c.a };
        splittableV = bV.getSplittableV();
        splittableH = bV.getSplittableH();
        deleteable = bV.getDeleteable();
        editorOnly = bV.getEditorOnly();

        lines = bV.getLines();
    }
}

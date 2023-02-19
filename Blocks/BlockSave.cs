[System.Serializable]
public class BlockSave
{
    public int blockVariant;
    public BlockSave[] subBlocks;

    public BlockSave(int blockVariant, BlockSave[] subBlocks)
    {
        this.blockVariant = blockVariant;
        this.subBlocks = subBlocks;
    }
}

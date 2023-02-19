using System.Collections.Generic;

[System.Serializable]
public class WorkspaceS
{
    public List<BlockVariantS> customBlockVariants;

    public WorkspaceS()
    {
        customBlockVariants = new List<BlockVariantS>();

        Dictionary<int, BlockManager.BlockVariant> custom = BlockManager.getCustomBlockVariants();
        foreach (KeyValuePair<int, BlockManager.BlockVariant> kvp in custom)
            customBlockVariants.Add(new BlockVariantS(kvp.Key, kvp.Value));
    }
}
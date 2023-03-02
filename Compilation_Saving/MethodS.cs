using System;
using UnityEngine;
using System.Text;

[System.Serializable]
public class MethodS
{
    [NonSerialized] public Block methodDeclaration;
    public BlockSave methodDeclarationS;
    [NonSerialized] public Block methodBodyMaster;
    public BlockSave methodBodyMasterS;

    public void save()
    {
        methodDeclarationS = methodDeclaration.saveBlock();

        if (methodBodyMaster != null)
            methodBodyMasterS = methodBodyMaster.saveBlock();
        else if (bodySaveBuffer != null)
            methodBodyMasterS = bodySaveBuffer;
    }

    [NonSerialized] public BlockSave bodySaveBuffer;
    public void saveBodyToBuffer()
    {
        if (methodBodyMaster == null)
        {
            Debug.Log("Err, trying to save null block tree to buffer.");
            return;
        }
        bodySaveBuffer = methodBodyMaster.saveBlock();
    }

    public string getCode()
    {
        if (methodDeclaration == null)
            return null;

        StringBuilder src = new StringBuilder();
        src.AppendLine("public " + methodDeclaration.getBlockText(true) + "\n{");
        if (methodBodyMaster != null)
            src.AppendLine(methodBodyMaster.getBlockText(true));
        else
        {
            BlockSave bS = methodBodyMasterS;
            if (bS == null) bS = bodySaveBuffer;
            if (bS != null)
                src.AppendLine(BlockManager.blockSaveToText(bS));
        }
        src.AppendLine("}");

        return src.ToString();
    }
}

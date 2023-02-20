using System;
using UnityEngine;

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

        // TODO: string builder
        string result = methodDeclaration.getBlockText(true);
        string bodyText = "";
        if (methodBodyMaster != null)
            bodyText = methodBodyMaster.getBlockText(true);
        else
        {
            BlockSave bS = methodBodyMasterS;
            if (bS == null) bS = bodySaveBuffer;
            if (bS != null)
                bodyText = BlockManager.blockSaveToText(bS);
        }

        result += "{\n" + bodyText + "\n}\n";


        return result;
    }
}

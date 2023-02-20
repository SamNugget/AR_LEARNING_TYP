using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snippet : Window
{
    private MethodS _methodSave;
    public MethodS methodSave
    {
        get
        {
            return _methodSave;
        }
        set
        {
            if (_methodSave == null)
            {
                _methodSave = value;
                initialiseBlocks();
                scaleWindow();
            }
        }
    }

    private void initialiseBlocks()
    {
        if (_methodSave.methodDeclarationS == null)
        {
            // create declaration block
            int bVI = BlockManager.getBlockVariantIndex(this is MethodSnippet ? "Method" : "Snippet Declaration");
            _methodSave.methodDeclaration = BlockManager.createMasterBlock(bVI, transform);
            // create body block
            bVI = BlockManager.getBlockVariantIndex("Method Block");
            _methodSave.methodBodyMaster = BlockManager.createMasterBlock(bVI, transform);
        }
        else
        {
            // create declaration block
            _methodSave.methodDeclaration = BlockManager.createMasterBlock(-1, transform, _methodSave.methodDeclarationS);
            // create body block
            _methodSave.methodBodyMaster = BlockManager.createMasterBlock(-1, transform, _methodSave.methodBodyMasterS);
        }

        _methodSave.methodBodyMaster.transform.position -= (Vector3)FontManager.lettersAndLinesToVector(0, 1, false) * 1.5f;
    }

    public override void scaleWindow()
    {
        if (_methodSave == null || _methodSave.methodDeclaration == null || _methodSave.methodBodyMaster == null) return;
        int w = _methodSave.methodDeclaration.getWidth();
        if (_methodSave.methodBodyMaster.getWidth() > w) w = _methodSave.methodBodyMaster.getWidth(); // get biggest width
        Vector2 scale = FontManager.lettersAndLinesToVector(w, _methodSave.methodBodyMaster.getHeight() + 2, false);

        width = scale.x;
        height = scale.y;
        base.scaleWindow();
    }
}

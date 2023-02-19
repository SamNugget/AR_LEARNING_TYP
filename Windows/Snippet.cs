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
            }
        }
    }

    private void initialiseBlocks()
    {
        if (_methodSave.methodDeclarationS == null)
        {
            // create declaration block
            int bVI = BlockManager.getBlockVariantIndex("Method");
            _methodSave.methodDeclaration = BlockManager.createMasterBlock(bVI, transform);

            // create body block
            bVI = BlockManager.getBlockVariantIndex("Method Window");
            _methodSave.methodBodyMaster = BlockManager.createMasterBlock(bVI, transform);
        }
        else
        {
            // create declaration block
            _methodSave.methodDeclaration = BlockManager.createMasterBlock(-1, transform, methodDeclarationS);
            
            // create body block
            _methodSave.methodBodyMaster = BlockManager.createMasterBlock(-1, transform, methodBodyMasterS);
        }
    }

    public override void scaleWindow(float width, float height)
    {
        int w = _methodSave.methodDeclaration.getWidth();
        if (_methodSave.methodBodyMaster.getWidth() > w) w = _methodSave.methodBodyMaster.getWidth(); // get biggest width
        Vector2 scale = FontManager.lettersAndLinesToVector(w, _methodSave.methodBodyMaster.getHeight() + 1);
        scale *= BlockManager.blockScale;


        base.scaleWindow(scale.x, scale.y);
    }
}

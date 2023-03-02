using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Snippet : Window
{
    protected virtual string decBlockName
    {
        get { return ""; }
    }

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
                initialise();
            }
        }
    }



    protected virtual void initialise()
    {
        if (_methodSave.methodDeclarationS == null)
            initialiseMethodS();
        initialiseBlocks();
        scaleWindow();
    }

    public void initialiseMethodS()
    {
        int blockCount = (decBlockName == "Method" ? 3 : 2);
        BlockSave[] subblocks = new BlockSave[blockCount];
        for (int i = 0; i < blockCount; i++) subblocks[i] = new BlockSave(0);
        subblocks[(decBlockName == "Method" ? 2 : 0)] = new BlockSave(BlockManager.getBlockVariantIndex("Place Variable"));

        _methodSave.methodDeclarationS = new BlockSave(BlockManager.getBlockVariantIndex(decBlockName), subblocks);

        // create body block (with splittable empty)
        int bVI = BlockManager.getBlockVariantIndex("Method Block");
        int eBVI = BlockManager.getBlockVariantIndex("EmptyV");
        _methodSave.methodBodyMasterS = new BlockSave(bVI, new BlockSave[] {
            new BlockSave(eBVI, new BlockSave[] { new BlockSave(0) })
        });
    }

    protected void initialiseBlocks()
    {
        // create declaration block
        _methodSave.methodDeclaration = BlockManager.createMasterBlock(-1, transform, _methodSave.methodDeclarationS);
        // create body block
        _methodSave.methodBodyMaster = BlockManager.createMasterBlock(-1, transform, _methodSave.methodBodyMasterS);

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



    public override void close()
    {
        ClassWindow cW = GetComponentInParent<ClassWindow>();
        if (cW != null) cW.removeMethod(this);
        base.close();
    }

    public void insert()
    {
        ClassWindow cW = GetComponentInParent<ClassWindow>();
        if (cW != null) cW.insertMethod(this);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassWindow : Window
{
    private Block nameMaster;
    private Block fieldMaster;

    private ReferenceTypeS _referenceTypeSave;
    public ReferenceTypeS referenceTypeSave
    {
        get
        {
            return _referenceTypeSave;
        }
        set
        {
            if (_referenceTypeSave == null)
            {
                _referenceTypeSave = value;
                initialiseBlocks();
                initialiseMethods();
            }
        }
    }

    private void initialiseBlocks()
    {
        // create name block
        int bVI = BlockManager.createNameBlock(_referenceTypeSave.name);
        nameMaster = BlockManager.createMasterBlock(bVI, transform);

        // create field blocks
        bVI = BlockManager.getBlockVariantIndex("Method Block");
        fieldMaster = BlockManager.createMasterBlock(bVI, transform);
        fieldMaster.transform.position -= (Vector3)FontManager.lettersAndLinesToVector(0, 1, false) * 1.5f;

        Block topBlock = fieldMaster.getSubBlock(0);
        if (_referenceTypeSave.fields.Count == 0)
            BlockManager.spawnBlock(BlockManager.getBlockVariantIndex("Place Field"), topBlock);
        else
        {
            topBlock = BlockManager.spawnBlock(0, topBlock, true, _referenceTypeSave.fields[0].fieldBlockS); // spawn first block
            _referenceTypeSave.fields[0].fieldBlock = topBlock; // save reference to spawned block

            for (int i = 1; i < _referenceTypeSave.fields.Count; i++)
            {
                Block splitter = BlockManager.splitBlock(topBlock); // split orig block
                topBlock = BlockManager.spawnBlock(0, splitter.getSubBlock(1), true, _referenceTypeSave.fields[i].fieldBlockS); // spawn new block
                _referenceTypeSave.fields[i].fieldBlock = topBlock; // save reference to spawned block
            }
        }


        // set correct sub blocks active
        fieldMaster.setColliderEnabled(true, BlockManager.blocksEnabledDefault);
    }

    private List<Snippet> methodSnippets = new List<Snippet>();
    private void initialiseMethods()
    {
        loadMethod(_referenceTypeSave.methods[0], true);

        for (int i = 1; i < _referenceTypeSave.methods.Count; i++)
            loadMethod(_referenceTypeSave.methods[i]);
    }

    public void removeMethod(Snippet toRemove)
    {
        methodSnippets.Remove(toRemove);
        _referenceTypeSave.removeMethod(toRemove.methodSave);
        scaleWindow();
    }

    private void loadMethod(MethodS methodSave, bool constructor = false, Snippet toGoBelow = null)
    {
        string window = (constructor ? "ConstructorSnippet" : "MethodSnippet");
        int pos = (toGoBelow == null ? methodSnippets.Count : (methodSnippets.IndexOf(toGoBelow) + 1));


        Snippet methodSnippet = (Snippet)WindowManager.spawnWindow(window);
        methodSnippet.transform.parent = transform;
        methodSnippet.transform.localRotation = Quaternion.identity;

        methodSnippet.methodSave = methodSave;
        methodSnippets.Insert(pos, methodSnippet);

        scaleWindow();
    }

    public void insertMethod(Snippet toGoBelow)
    {
        MethodS mS = new MethodS();
        _referenceTypeSave.addMethod(mS);
        loadMethod(mS, false, toGoBelow);
    }

    public override void scaleWindow()
    {
        if (nameMaster == null || fieldMaster == null) return;
        int w = fieldMaster.getWidth();
        if (nameMaster.getWidth() > w) w = nameMaster.getWidth(); // get biggest width
        Vector2 scale = FontManager.lettersAndLinesToVector(w, fieldMaster.getHeight() + 2, false);


        width = scale.x;
        height = scale.y;
        base.scaleWindow();


        // move snippets
        float yPos = -height;
        foreach (Snippet mS in methodSnippets)
        {
            mS.transform.localPosition = new Vector3(0f, yPos, 0f);
            yPos -= mS.height;
        }
    }
}

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
                scaleWindow();
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
    }

    private List<MethodSnippet> methodSnippets = new List<MethodSnippet>();
    private void initialiseMethods()
    {
        string window = "ConstructorSnippet";
        foreach (MethodS methodSave in _referenceTypeSave.methods)
        {
            Window methodWindow = WindowManager.spawnWindow(window);
            methodWindow.transform.parent = transform;

            MethodSnippet methodSnippet = (MethodSnippet)methodWindow;
            methodSnippet.methodSave = methodSave;
            methodSnippets.Add(methodSnippet);

            if (window[0] == 'C') window = "MethodSnippet";
        }
    }

    public override void scaleWindow()
    {
        int w = fieldMaster.getWidth();
        if (nameMaster.getWidth() > w) w = nameMaster.getWidth(); // get biggest width
        Vector2 scale = FontManager.lettersAndLinesToVector(w, fieldMaster.getHeight() + 2, false);


        width = scale.x;
        height = scale.y;
        base.scaleWindow();


        // move snippets
        float yPos = -height;
        foreach (MethodSnippet mS in methodSnippets)
        {
            mS.transform.localPosition = new Vector3(0f, yPos, 0f);
            yPos -= mS.height;
        }
    }
}

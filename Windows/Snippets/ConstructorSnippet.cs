using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FileManagement;

public class ConstructorSnippet : RunnableSnippet
{
    protected override string decBlockName
    {
        get { return "Constructor"; }
    }

    protected override Block getParametersParent()
    {
        return methodSave.methodDeclaration.getSubBlock(1);
    }

    public override void run()
    {
        ClassWindow cW = GetComponentInParent<ClassWindow>();
        CompilationManager.constructObject(cW.referenceTypeSave.name, spawnPoint.position);
    }
}

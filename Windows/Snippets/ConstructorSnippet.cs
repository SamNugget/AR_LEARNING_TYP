using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FileManagement;
using ObjectInstances;

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

        try {
            CompilationManager.constructObject(cW.referenceTypeSave.name, getParameters(), spawnPoint); }
        catch (Exception e) {
            InspectionPlatform.Log(e.ToString()); }
    }
}

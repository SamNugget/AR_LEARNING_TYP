using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MethodSnippet : Snippet
{
    public override void close()
    {
        GetComponentInParent<ClassWindow>().removeMethod(this);
        base.close();
    }

    public void insert()
    {
        GetComponentInParent<ClassWindow>().insertMethod(this);
    }
}

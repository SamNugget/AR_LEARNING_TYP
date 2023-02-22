using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionManagement;
using TMPro;

public class TextEntryWindow : TitledWindow
{
    [SerializeField] private TMP_InputField inputField;

    public void onEndEdit()
    {
        if (inputField.text.Length != 0)
            ActionManager.callCurrentMode(inputField.text);
    }

    public override void close()
    {
        ActionManager.clearMode();
        base.close();
    }
}

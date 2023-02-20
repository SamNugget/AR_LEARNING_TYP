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
        string input = "";
        foreach (char c in inputField.text)
        {
            if (c == '_') ;
            else if (c >= 48 && c <= 57) ; // numbers
            else if (c >= 65 && c <= 90) ; // uppercase
            else if (c >= 97 && c <= 122) ; // lowercase
            else continue;

            input += c;
        }

        if (input.Length != 0)
            ActionManager.callCurrentMode(input);
    }

    public override void close()
    {
        ActionManager.clearMode();
        base.close();
    }
}

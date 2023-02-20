using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitledWindow : Window
{
    [SerializeField] private TextMeshPro titleText;
    [SerializeField] private string title;
    public void setTitle(string title)
    {
        this.title = title;
        setTitleMessage();
    }

    public void setTitleMessage(string message = "")
    {
        if (message == "")
            titleText.text = title;
        else
            titleText.text = title + '-' + message;
    }

    void Start()
    {
        setTitleMessage();
    }
}

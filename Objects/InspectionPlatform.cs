using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ObjectInstances;

public class InspectionPlatform : SnapListener
{
    [SerializeField] private TextMeshProUGUI screenText;
    [SerializeField] private float printSpeed = 1f;
    [SerializeField] private string defaultMessage;

    public override void onSnap(Transform snapped)
    {
        ObjectInstance oI = snapped.GetComponent<ObjectInstance>();

        if (oI == null)
        {
            Debug.Log("Err: Snapped object has no ObjectInstance component.");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(printText(oI.getInspectText()));
    }

    private IEnumerator printText(string text)
    {
        screenText.text = "";

        float timePerLetter = printSpeed / text.Length;

        for (int i = 0; i < text.Length; i++)
        {
            screenText.text += text[i];
            yield return new WaitForSeconds(timePerLetter);
        }
    }

    public override void onUnsnap()
    {
        StopAllCoroutines();
        StartCoroutine(printText(defaultMessage));
    }

    void Start()
    {
        screenText.text = defaultMessage;
    }
}

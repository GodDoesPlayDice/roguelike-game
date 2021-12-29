using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextTMPController : MonoBehaviour
{
    private TextMeshPro _textMeshPro;

    public void UpdateText(string newValue)
    {
        if (_textMeshPro != null)
        {
            _textMeshPro.text = newValue;
        }
    }
}

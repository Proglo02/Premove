using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_Text))]
public class SliderText : MonoBehaviour
{
    private TMP_Text text;
    private Slider slider;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        slider = GetComponentInParent<Slider>();

        OnSliderValueChanged();
    }

    public void OnSliderValueChanged()
    {
        float deci = slider.value;

        deci = deci - (int)deci;
        deci = MathF.Round(deci, 1, MidpointRounding.AwayFromZero);

        if (!slider.wholeNumbers && deci != 0)
            text.text = slider.value.ToString("#.0");
        else
            text.text = ((int)slider.value).ToString();
    }
}

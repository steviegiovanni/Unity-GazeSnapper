using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.Receivers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapperUI : InteractionReceiver {
    [SerializeField]
    private Snapper _snapper;

	protected override void InputClicked(GameObject obj, InputClickedEventData eventData)
    {
        _snapper.NextMode();
    }

    public void OnSliderValueChange(float f)
    {
        _snapper.Step = Mathf.RoundToInt(f);
    }
}

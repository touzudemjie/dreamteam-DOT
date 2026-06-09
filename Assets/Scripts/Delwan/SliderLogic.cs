using System;
using UnityEngine;
using UnityEngine.UI;

public class SliderLogic : MonoBehaviour
{

    [SerializeField] private float _sliderFullduration;
    private float _currentDuration;
    private bool _canIncrease;
    private Slider _slider;
    public event Action OnValueReachedMax;
    private void Start()
    {
       _slider = GetComponent<Slider>();
    }
    private void Update()
    {
        IncreaseSliderValue();
    }
    private void IncreaseSliderValue()
    {
        if (_sliderFullduration == 0 || !_canIncrease) return;
        _currentDuration += Time.deltaTime;
        float ratio = Mathf.Clamp01(_currentDuration / _sliderFullduration);
        _slider.value = Mathf.Lerp(_slider.minValue,_slider.maxValue, ratio);
        if (ratio >= 1)
        {
            OnValueReachedMax?.Invoke();
            _canIncrease = false;
            _currentDuration = 0;
        }
    }
    public void CanIncreaseValue()
    {
        _canIncrease = true;
    }
    private void OnDisable()
    {
        _slider.value = 0;
    }
}

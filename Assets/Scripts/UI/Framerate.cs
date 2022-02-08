using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class Framerate : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _displayText;
        [SerializeField, Range(0.1f, 2f)] private float _sampleDuration = 1;

        private int _frames;
        private float _duration;

        private float _best;
        private float _worst;

        private void Update()
        {
            float frameDuration = Time.unscaledDeltaTime;
            _frames++;
            _duration += frameDuration;

            if (frameDuration < _best)
            {
                _best = frameDuration;
            }

            if (frameDuration > _worst)
            {
                _worst = frameDuration;
            }

            if (_duration >= _sampleDuration)
            {
                _displayText.SetText($"FPS\n" +
                                     $"{_frames / _duration,0:0}" +
                                     $"\n{1f / _best,0:0}" +
                                     $"\n{1f / _worst,0:0}");

                _frames = 0;
                _duration = 0f;
                _best = float.MaxValue;
                _worst = 0;
            }
        }
    }
}
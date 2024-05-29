using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Tools;
using UnityEngine;

namespace GorillaInfoWatch.Models
{
    public class Relations
    {
        public Main Main;
        public AssetLoader AssetLoader;
        public Configuration Config;

        public AudioClip FriendJoinClip, FriendLeftClip;

        private float _pressTime;
        private Slider _currentSlider;

        public bool Slideable(Slider slider) => (_currentSlider == null || _currentSlider == slider) && Time.realtimeSinceStartup > (_pressTime + 0.33f);

        public bool Pressable() => Time.realtimeSinceStartup > (_pressTime + 0.33f) && !_currentSlider;

        public void Press(Button button, bool isLeftHand)
        {
            _pressTime = Time.realtimeSinceStartup;
            Main.PressButton(button, isLeftHand);
        }

        public void AddSlider(Slider slider)
        {
            _currentSlider = slider;
        }

        public void RemoveSlider(Slider slider, bool isLeftHand)
        {
            _currentSlider = null;
            _pressTime = Time.realtimeSinceStartup + 0.1f;
            Main.PressSlider(slider, isLeftHand);
        }
    }
}

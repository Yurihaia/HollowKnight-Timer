using Modding;

namespace FrameDisplay
{

    public class SaveSettings : IModSettings { }

    public class GlobalSettings : IModSettings
    {
        public string Pause
        {
            get => GetString("f10");
            set => SetString(value);
        }

        public string Reset
        {
            get => GetString("f11");
            set => SetString(value);
        }
    }
}
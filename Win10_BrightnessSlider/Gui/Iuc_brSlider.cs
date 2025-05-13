namespace Win10_BrightnessSlider
{
    internal interface Iuc_brSlider
    {
        int Height { get;  }
        string NotifyIconText { get; }

        void UpdateSliderControl();


        RichInfoScreen richInfoScreen { get; set; }
        void Set_MonitorName(string name );
    }
}
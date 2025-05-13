namespace Win10_BrightnessSlider
{

    public static class StringExt
    {
        /// <summary>
        ///  shorten long Text  , aka SubstringSafe 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) 
                return value;
            return 
                value.Length <= maxLength ? 
                value : 
                value.Substring(0, maxLength);
        }
    }










}






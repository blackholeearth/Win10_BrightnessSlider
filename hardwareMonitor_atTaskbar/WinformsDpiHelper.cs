using System;

public static class WinformsDpiHelper
{
	[System.Runtime.InteropServices.DllImport("user32.dll")]
	private static extern bool SetProcessDPIAware();

	[System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]
	private static extern int SetProcessDpiAwarenessContext(int dpiContext);



	/// <summary>
	///  // *** fix blurry text on High-Dpi Screen  <para></para>
	///  // *** call it Program.cs  Main()   as First Line.   Before EnableVisualStyles.. <para></para>
	///   
	/// <para></para>
	///  WinformsDpiHelper.SetProcess_DPIAware(); <para></para>
	///  
	///  Application.EnableVisualStyles(); <para></para>
	/// </summary>
	public static void SetProcess_DPIAware()
	{
		// ***fix blurry text on High-Dpi Screen
		if (Environment.OSVersion.Version.Major >= 6)
			SetProcessDPIAware();
	}

	/// <summary>
	///  1. Try to set "Per Monitor V2" (The best one, Win10 Creator's Update+)  <para/>
	///  2. If that fails (e.g. Windows 7), fall back to "System Aware"   <para></para>
	/// 
	///  // *** fix blurry text on High-Dpi Screen  <para></para>
	///  // *** call it Program.cs  Main()   as First Line.   Before EnableVisualStyles.. <para></para>
	///   
	/// <para></para>
	///  WinformsDpiHelper.SetProcessDPIAware_PerMonitorV2_withFallback(); <para></para>
	///  
	///  Application.EnableVisualStyles(); <para></para>
	/// </summary>
	/// <returns></returns>
	public static void SetProcessDPIAware_PerMonitorV2_withFallback()
	{
		// 1. Try to set "Per Monitor V2" (The best one, Win10 Creator's Update+)
		if (SetProcessDPIAware_PerMonitorV2())
		{
			// 2. If that fails (e.g. Windows 7), fall back to "System Aware" (Your old way)
			SetProcessDPIAware();
		}
	}


	private static bool SetProcessDPIAware_PerMonitorV2()
	{
		// Windows 10 1703+ only. 
		// Context value -4 means DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2
		try
		{
			int result = SetProcessDpiAwarenessContext(-4);
			return result == 1; // S_OK
		}
		catch (EntryPointNotFoundException)
		{
			// Function doesn't exist (Windows 7 or old Win10)
			return false;
		}
	}








}

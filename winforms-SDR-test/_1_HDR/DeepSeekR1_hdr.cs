using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace winformsTests._1_HDR
{

	public class DeepSeekR1_HdrBrightnessController
	{
		[StructLayout(LayoutKind.Sequential)]
		private struct DISPLAYCONFIG_DEVICE_INFO_HEADER
		{
			public DISPLAYCONFIG_DEVICE_INFO_TYPE type;
			public int size;
			public LUID adapterId;
			public uint id;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct DISPLAYCONFIG_SET_SDR_WHITE_LEVEL
		{
			public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
			public uint SDRWhiteLevel;
			public byte finalValue;
		}

		private enum DISPLAYCONFIG_DEVICE_INFO_TYPE : uint
		{
			SET_SDR_WHITE_LEVEL = 0xFFFFFFEE
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct LUID
		{
			public uint LowPart;
			public int HighPart;
		}

		[DllImport("user32.dll")]
		private static extern int DisplayConfigSetDeviceInfo(ref DISPLAYCONFIG_SET_SDR_WHITE_LEVEL request);

		public static void SetSdrBrightnessAdvanced(int nits, LUID adapterId, uint targetId)
		{
			if (nits < 80 || nits > 480)
				throw new ArgumentOutOfRangeException(nameof(nits), "Must be between 80-480 nits");

			// Get display config data first (implementation omitted for brevity)

			var request = new DISPLAYCONFIG_SET_SDR_WHITE_LEVEL
			{
				header = new DISPLAYCONFIG_DEVICE_INFO_HEADER
				{
					type = DISPLAYCONFIG_DEVICE_INFO_TYPE.SET_SDR_WHITE_LEVEL,
					size = Marshal.SizeOf<DISPLAYCONFIG_SET_SDR_WHITE_LEVEL>(),
					adapterId = adapterId,
					id = targetId
				},
				SDRWhiteLevel = (uint)(nits * 1000 / 80),
				finalValue = 1
			};

			int result = DisplayConfigSetDeviceInfo(ref request);
			if (result != 0)
				Marshal.ThrowExceptionForHR(result);
		}

		private static (LUID adapterId, uint targetId) GetPrimaryDisplayConfig()
		{
			// Implementation requires QueryDisplayConfig P/Invoke
			// See full implementation in GitHub gist below
			throw new NotImplementedException();
		}
	}


}
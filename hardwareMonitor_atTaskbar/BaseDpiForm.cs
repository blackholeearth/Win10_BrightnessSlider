using System.Drawing;
using System.Windows.Forms;

public class BaseDpiForm : Form
{

#if !NET8_0_OR_GREATER
	// .NET 4.8 FIX:
	public BaseDpiForm()
	{
		// Force font to match OS (Segoe UI) instead of legacy Sans Serif
		this.Font = SystemFonts.MessageBoxFont;
		this.AutoScaleMode = AutoScaleMode.Font;
	}

	protected override void OnDpiChanged(DpiChangedEventArgs e)
	{
		base.OnDpiChanged(e);

		// FIX: Force .NET 4.8 to accept the precise rectangle from Windows
		if (e.SuggestedRectangle.Width > 0)
		{
			this.Size = e.SuggestedRectangle.Size;
			this.Location = e.SuggestedRectangle.Location;
		}
	}
#endif

}
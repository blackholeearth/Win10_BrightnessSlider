// --- File: PaddedToolStripDropDown.cs ---
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace winformsTests._4_contextMenuStrip // Ensure this namespace matches yours
{
	// This custom dropdown will contain the logic to apply padding AFTER
	// the framework has decided where it initially wants to place it.
	public class PaddedToolStripDropDown : ToolStripDropDown
	{
		// We need access to the owner ContextMenuStripEX to get padding values and calculation logic
		private readonly ContextMenuStripEX _ownerContextMenuStrip; // Mark readonly

		// Constructor - requires the owner strip
		// CORRECTED: Call the base parameterless constructor `: base()`
		public PaddedToolStripDropDown(ContextMenuStripEX ownerStrip) : base() // <--- FIX HERE
		{
			// Assign the owner strip in the constructor body
			_ownerContextMenuStrip = ownerStrip ?? throw new ArgumentNullException(nameof(ownerStrip));
		}

		// Override SetBoundsCore - this is called internally to set the final position/size
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			// --- Stage 1: Let the base framework calculate its intended bounds first ---
			// Important: We pass the original x, y, width, height here initially.
			// The base call might modify these based on its *own* screen clamping (to unpadded area).
			base.SetBoundsCore(x, y, width, height, specified);

			// --- Stage 2: Apply OUR clamping logic based on the PADDED area ---
			// Check if we should apply logic (relevant context, owner exists)
			// Use OwnerItem property to check if it's attached to a parent item
			if (this.Visible && _ownerContextMenuStrip != null && this.OwnerItem is ToolStripDropDownItem parentItem)
			{
				try
				{
					// Get the bounds *after* the base call finished
					Rectangle currentBounds = this.Bounds;
					// Ensure we have valid bounds before proceeding
					if (currentBounds.Width <= 0 || currentBounds.Height <= 0)
					{
						Debug.WriteLine("!!! PaddedToolStripDropDown.SetBoundsCore: Invalid bounds after base call, skipping clamp.");
						// Ensure base was called if we exit early
						if (!this.Bounds.Equals(new Rectangle(x, y, width, height))) base.SetBoundsCore(x, y, width, height, specified);
						return;
					}


					Screen currentScreen = Screen.FromControl(this); // Screen this dropdown is on
					Rectangle parentItemScreenBounds;
					// Need parent's Owner (the ContextMenuStripEX) to convert parent item bounds to screen
					if (parentItem.Owner != null)
					{
						parentItemScreenBounds = parentItem.Owner.RectangleToScreen(parentItem.Bounds);
					}
					else
					{
						// Cannot get parent screen bounds reliably, might need fallback or skip horizontal clamp
						Debug.WriteLine("!!! PaddedToolStripDropDown.SetBoundsCore: Cannot get parentItem.Owner, horizontal clamping might be inaccurate.");
						// Use a default/placeholder if needed, though clamping might be less accurate
						parentItemScreenBounds = new Rectangle(currentBounds.X - 100, currentBounds.Y, 100, 20); // Placeholder
					}


					// Use the owner strip's calculation method (ensure it's accessible)
					Point finalClampedPosition = _ownerContextMenuStrip.CalculateClampedDropDownPositionPublic(
						currentBounds,          // Clamp the bounds the framework just set
						parentItemScreenBounds,
						currentScreen
					);

					// If our calculation resulted in a different position, apply it
					if (currentBounds.Location != finalClampedPosition)
					{
						Debug.WriteLine($"--- PaddedToolStripDropDown.SetBoundsCore [{parentItem.Text}] ---");
						Debug.WriteLine($"Bounds after base call: {currentBounds}");
						Debug.WriteLine($"Calculated Padded Location: {finalClampedPosition}");

						// --- Stage 3: Call base.SetBoundsCore AGAIN with the corrected location ---
						// We only specify Location because we only want to change that.
						base.SetBoundsCore(finalClampedPosition.X, finalClampedPosition.Y, currentBounds.Width, currentBounds.Height, BoundsSpecified.Location);

						Debug.WriteLine($"Bounds after re-clamp: {this.Bounds}");
					}
					// else: Our calculation agrees with the base result (already clamped or didn't need clamping)
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"!!! ERROR in PaddedToolStripDropDown.SetBoundsCore adjustment: {ex.Message}");
					// Fallback: Ensure the base method still runs even if our logic fails
					if (!this.Bounds.Equals(new Rectangle(x, y, width, height)))
					{
						base.SetBoundsCore(x, y, width, height, specified);
					}
				}
			}
			// Removed the 'else' block that called base.SetBoundsCore again,
			// as the initial call at the top of the method handles the cases
			// where we don't apply custom logic.
		}
	}
}
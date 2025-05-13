// --- File: ContextMenuStripEX.cs ---
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices; // Keep if used elsewhere, not strictly needed here now
using System.Diagnostics;
using System.Linq; // Needed for OfType<>

namespace winformsTests._4_contextMenuStrip // Ensure this namespace matches yours
{
	public class ContextMenuStripEX : ContextMenuStrip
	{
		// --- Padding Configuration ---
		// Padding applied inside the screen's working area for clamping calculations.
		private int workingArea_padding_width = 0;  // Horizontal padding from screen edges
		private int workingArea_padding_height = 8; // Vertical padding from screen edges (e.g., taskbar)

		// --- Constructors ---
		public ContextMenuStripEX(IContainer container) : base(container) { }
		public ContextMenuStripEX() : base() { }

		#region Main Menu Positioning (Win11 Style)

		/// <summary>
		/// Calculates the 'Win11 Style' position for the main context menu
		/// (above cursor, centered, clamped to padded working area).
		/// </summary>
		/// <param name="clickPoint">The screen coordinates where the user clicked (usually Cursor.Position).</param>
		/// <returns>The calculated Point where the menu should be shown.</returns>
		private Point CalculateWin11StylePosition(Point clickPoint)
		{
			Screen currentScreen = Screen.FromPoint(clickPoint);
			Rectangle workingArea = GetPaddedWorkingArea(currentScreen); // Use padded area

			Size menuSize = this.GetPreferredSize(Size.Empty);
			if (menuSize.Width <= 0 || menuSize.Height <= 0)
			{
				// Cannot calculate reliably without size, fallback or default needed
				// For example, just show at click point (less ideal)
				return clickPoint;
				// Or return a default safe position like workingArea.Location
				// return workingArea.Location;
			}


			// Calculate initial desired position: Centered above click point
			int menuX = clickPoint.X - menuSize.Width / 2;
			int menuY = clickPoint.Y - menuSize.Height;

			// Clamp X to padded working area
			if (menuX < workingArea.Left) menuX = workingArea.Left;
			else if (menuX + menuSize.Width > workingArea.Right) menuX = workingArea.Right - menuSize.Width;
			// Final check if menu wider than padded area
			if (menuX < workingArea.Left) menuX = workingArea.Left;


			// Clamp Y to padded working area (Prioritize showing above)
			if (menuY < workingArea.Top) // If menu goes above top edge when placed above cursor
			{
				menuY = clickPoint.Y; // Try showing below cursor instead
				if (menuY + menuSize.Height > workingArea.Bottom) // If still doesn't fit below
				{
					menuY = workingArea.Bottom - menuSize.Height; // Align to padded bottom edge
				}
				// Final check if menu taller than padded area or shifted above top
				if (menuY < workingArea.Top) menuY = workingArea.Top;
			}
			else if (menuY + menuSize.Height > workingArea.Bottom) // If menu goes below bottom edge (when placed above)
			{
				// Try showing below cursor if it fits
				int tryBelowY = clickPoint.Y;
				if (tryBelowY + menuSize.Height <= workingArea.Bottom)
				{
					menuY = tryBelowY;
				}
				else
				{
					// Doesn't fit below either, so align to padded bottom edge
					menuY = workingArea.Bottom - menuSize.Height;
					// Final check if menu taller than padded area or shifted above top
					if (menuY < workingArea.Top) menuY = workingArea.Top;
				}
			}

			return new Point(menuX, menuY);
		}

		/// <summary>
		/// Shows the context menu using the calculated 'Win11 Style' position.
		/// </summary>
		/// <param name="clickPoint">The screen coordinates where the user clicked (usually Cursor.Position).</param>
		public void ShowWin11Style(Point clickPoint)
		{
			// Ensure items are populated before getting preferred size
			if (this.Items.Count == 0 && this.SourceControl != null)
			{
				// This might manually trigger item population if needed, depends on setup
				// base.OnOpening(new CancelEventArgs());
			}

			Point finalLocation = CalculateWin11StylePosition(clickPoint);

			// Call the base Show method with the calculated screen coordinates
			base.Show(finalLocation);

			// Note: SetForegroundWindow logic was moved to NotifyIconEX
		}

		#endregion

		#region SubMenu Positioning Logic (via Custom DropDown)

		/// <summary>
		/// Calculates the screen working area inset by the defined padding.
		/// Marked internal so PaddedToolStripDropDown can access it.
		/// </summary>
		internal Rectangle GetPaddedWorkingArea(Screen screen)
		{
			Rectangle workingArea = screen.WorkingArea;
			// Calculate safe inflation values (prevent negative size)
			int inflateX = Math.Max(0, Math.Min(workingArea_padding_width, workingArea.Width / 2 - 1));
			int inflateY = Math.Max(0, Math.Min(workingArea_padding_height, workingArea.Height / 2 - 1));
			workingArea.Inflate(-inflateX, -inflateY);
			return workingArea;
		}

		/// <summary>
		/// Calculates a clamped screen position for a dropdown menu (submenu),
		/// ensuring it stays within the PADDED working area.
		/// </summary>
		/// <param name="proposedBounds">The screen bounds where the dropdown intends to show.</param>
		/// <param name="parentItemBounds">The screen bounds of the parent menu item.</param>
		/// <param name="screen">The screen the menu is primarily on.</param>
		/// <returns>The adjusted Point (screen coordinates) for the dropdown's top-left corner.</returns>
		private Point CalculateClampedDropDownPosition(Rectangle proposedBounds, Rectangle parentItemBounds, Screen screen)
		{
			Rectangle workingArea = GetPaddedWorkingArea(screen); // Use padded area
			int menuX = proposedBounds.X;
			int menuY = proposedBounds.Y;
			int menuWidth = proposedBounds.Width;
			int menuHeight = proposedBounds.Height;

			// --- Clamp Y ---
			if (menuY < workingArea.Top)
			{
				menuY = workingArea.Top;
			}
			else if (menuY + menuHeight > workingArea.Bottom)
			{
				menuY = workingArea.Bottom - menuHeight;
			}
			// Final Y check if menu is taller than padded working area
			if (menuY < workingArea.Top) menuY = workingArea.Top;


			// --- Clamp X ---
			bool showingRight = (menuX >= parentItemBounds.Left); // Determine intended side

			if (showingRight)
			{
				// Goes past right edge?
				if (menuX + menuWidth > workingArea.Right)
				{
					// Try showing left instead
					int leftX = parentItemBounds.Left - menuWidth;
					if (leftX >= workingArea.Left)
					{ // Does it fit on left?
						menuX = leftX;
					}
					else
					{ // Doesn't fit left either, clamp to right edge
						menuX = workingArea.Right - menuWidth;
						// Final X check if menu wider than padded working area
						if (menuX < workingArea.Left) menuX = workingArea.Left;
					}
				}
				// Goes past left edge? (unlikely if showingRight, but check)
				else if (menuX < workingArea.Left)
				{
					menuX = workingArea.Left;
				}
			}
			else
			{ // Proposed position was already to the left
			  // Goes past left edge?
				if (menuX < workingArea.Left)
				{
					// Try showing right instead
					int rightX = parentItemBounds.Right;
					if (rightX + menuWidth <= workingArea.Right)
					{ // Does it fit on right?
						menuX = rightX;
					}
					else
					{ // Doesn't fit right either, clamp to left edge
						menuX = workingArea.Left;
						// Final X check if menu wider than padded working area
						if (menuX + menuWidth > workingArea.Right) menuX = workingArea.Right - menuWidth;
					}
				}
				// Goes past right edge? (unlikely if showingLeft, but check)
				else if (menuX + menuWidth > workingArea.Right)
				{
					menuX = workingArea.Right - menuWidth;
				}
			}

			return new Point(menuX, menuY);
		}

		/// <summary>
		/// Public/internal wrapper for the clamping logic so PaddedToolStripDropDown can call it.
		/// </summary>
		internal Point CalculateClampedDropDownPositionPublic(Rectangle proposedBounds, Rectangle parentItemBounds, Screen screen)
		{
			return CalculateClampedDropDownPosition(proposedBounds, parentItemBounds, screen);
		}

		/// <summary>
		/// Overridden to ensure that any ToolStripDropDownItem added gets our
		/// custom PaddedToolStripDropDown assigned, which handles the clamping logic.
		/// </summary>
		protected override void OnItemAdded(ToolStripItemEventArgs e)
		{
			base.OnItemAdded(e);

			// If the added item can potentially host a dropdown...
			if (e.Item is ToolStripDropDownItem dropDownItem)
			{
				// Case 1: Item is added WITHOUT a pre-assigned DropDown
				if (dropDownItem.DropDown == null)
				{
					// Assign our custom dropdown immediately.
					// The DropDown property getter will usually create one on demand,
					// but assigning it here ensures it's our type from the start.
					dropDownItem.DropDown = new PaddedToolStripDropDown(this);
					Debug.WriteLine($"Assigned PaddedToolStripDropDown to NEW item: {dropDownItem.Text}");
				}
				// Case 2: Item is added WITH a pre-assigned DropDown (e.g., from Designer)
				else if (!(dropDownItem.DropDown is PaddedToolStripDropDown))
				{
					// The assigned dropdown is NOT our custom type. We must replace it.
					Debug.WriteLine($"Replacing existing DropDown on item: {dropDownItem.Text} with PaddedToolStripDropDown.");

					ToolStripDropDown oldDropDown = dropDownItem.DropDown;
					PaddedToolStripDropDown newDropDown = new PaddedToolStripDropDown(this);

					// Attempt to move existing items from the old dropdown to the new one.
					// Using ToArray() avoids modifying the collection while iterating.
					ToolStripItem[] itemsToMove = oldDropDown.Items.OfType<ToolStripItem>().ToArray();
					if (itemsToMove.Length > 0)
					{
						newDropDown.Items.AddRange(itemsToMove); // AddRange handles removing from old automatically
					}


					// Copy relevant properties from the old dropdown (add more if needed)
					newDropDown.AutoSize = oldDropDown.AutoSize;
					newDropDown.Padding = oldDropDown.Padding;
					newDropDown.Margin = oldDropDown.Margin;
					newDropDown.MaximumSize = oldDropDown.MaximumSize;
					newDropDown.BackColor = oldDropDown.BackColor;
					newDropDown.ForeColor = oldDropDown.ForeColor;
					newDropDown.Font = oldDropDown.Font;
					// Consider copying ImageList, Renderer, RenderMode if customized

					// Assign the new dropdown to the item
					dropDownItem.DropDown = newDropDown;

					// Dispose the old dropdown *after* reassigning the property,
					// as accessing DropDown might trigger creation otherwise.
					oldDropDown.Dispose();
					Debug.WriteLine($"Successfully replaced DropDown for: {dropDownItem.Text}");
				}
				// Case 3: The item already has our custom dropdown (e.g., added programmatically before), do nothing.
				else
				{
					Debug.WriteLine($"Item already has PaddedToolStripDropDown: {dropDownItem.Text}");
				}
			}
		}

		// NOTE: All previous submenu-related event handlers (SubMenu_Opening, SubMenu_Opened,
		// DropDownItem_DropDownOpening, DropDownItem_DropDownClosed) and their associated
		// event subscriptions/unsubscriptions have been removed, as the logic is now
		// handled by the PaddedToolStripDropDown class assigned in OnItemAdded.

		#endregion
	}
}
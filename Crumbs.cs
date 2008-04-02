// Crumbs.cs
//
// Copyright (c) Christian Hergert <christian.hergert@gmail.com>
// Copyright (c) Mark A. Nicolosi <mark.a.nicolosi@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

using Cairo;
using Gdk;
using Gtk;

namespace Manico
{
	public class Crumbs : Gtk.Container
	{
		public event EventHandler Changed;

		private Dictionary<Crumb,Gdk.Rectangle> m_Coords;
		private Dictionary<Crumb,Gdk.Rectangle> m_Requests;
		private List<Crumb>                     m_Crumbs;
		private Crumb                           m_Active;
		private Crumb                           m_Hover;

		private int m_Spacing   = 6;
		private int m_Xoffset   = 9;
		private int m_Yoffset   = 6;
		private int m_Radius    = 5;

		private int m_MinWidth  = 16;
		private int m_MinHeight = 16;

		private Cairo.LinearGradient m_NormalBg;
		private Cairo.LinearGradient m_PrelightBg;
		private Cairo.LinearGradient m_SelectedBg;

		private Cairo.Color m_NormalBgBegin;
		private Cairo.Color m_NormalBgEnd;
		private Cairo.Color m_PrelightBgBegin;
		private Cairo.Color m_PrelightBgEnd;
		private Cairo.Color m_SelectedBgBegin;
		private Cairo.Color m_SelectedBgEnd;

		public Crumbs ()
		{
			this.m_Coords = new Dictionary<Crumb,Gdk.Rectangle> ();
			this.m_Requests = new Dictionary<Crumb,Gdk.Rectangle> ();
			this.m_Crumbs = new List<Crumb> ();
		}

		public Crumb Active {
			get {
				return this.m_Active;
			}
			set {
				this.m_Active = value;
				this.QueueDraw ();

				if (Changed != null) {
					Changed (this, new EventArgs ());
				}
			}
		}

		public IList<Crumb> CrumbList {
			get {
				return this.m_Crumbs.AsReadOnly ();
			}
		}

		private Crumb Hover {
			get {
				return this.m_Hover;
			}
		}

		public int Radius {
			get {
				return this.m_Radius;
			}
			set {
				this.m_Radius = value;
				this.QueueResize ();
			}
		}

		public int Spacing {
			get {
				return this.m_Spacing;
			}
			set {
				this.m_Spacing = value;
				this.QueueResize ();
			}
		}

		public int Xoffset {
			get {
				return this.m_Xoffset;
			}
			set {
				this.m_Xoffset = value;
				this.QueueResize ();
			}
		}

		public int Yoffset {
			get {
				return this.m_Yoffset;
			}
			set {
				this.m_Yoffset = value;
				this.QueueResize ();
			}
		}

		public void Append (Crumb crumb)
		{
			this.m_Crumbs.Add (crumb);

			if (this.IsRealized) {
				this.PrepareCrumb (crumb);
			}

			this.QueueResize ();
		}

		public void Insert (int index, Crumb crumb)
		{
			this.m_Crumbs.Insert (index, crumb);

			if (this.IsRealized) {
				this.PrepareCrumb (crumb);
			}

			this.QueueResize ();
		}

		public void Remove (Crumb crumb)
		{
			this.m_Crumbs.Remove (crumb);
			this.UnprepareCrumb (crumb);
		}

        private void DrawBorder (Gdk.Rectangle area)
        {
            Style.PaintBox (base.Style, base.GdkWindow,
                StateType.Normal, ShadowType.In, area, this,
                "buttondefault", area.X, area.Y,
                area.Width + area.X, area.Height + area.Y);
        }

		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
			Gdk.Rectangle alloc = this.Allocation;

			// XXX: Not sure why we need to transpose this. Probably
			//      setting up our GdkWindow incorrectly.
			if (this.Toplevel as Gtk.Container != null) {
				Container parent = this.Toplevel as Container;
				alloc.X -= (int) parent.BorderWidth;
				alloc.Y -= (int) parent.BorderWidth;
			}

			using (Context cr = CairoHelper.Create (this.GdkWindow)) {
                DrawBorder (alloc);

				int i = 0;
				foreach (Crumb crumb in this.m_Crumbs) {
					Gdk.Rectangle area = m_Coords[crumb];

					// Pad the widget area
					area.X -= 6;
					area.Y -= 4;
					area.Width += 12;
					area.Height += 8;

					// Determine what corners we should draw
					Corners corners;
					if (i == 0)
						corners = Corners.TopLeft | Corners.BottomLeft;
					else if (i + 1 == m_Crumbs.Count)
						corners = Corners.TopRight | Corners.BottomRight;
					else
						corners = Corners.None;

					bool isHover = crumb == Hover;
					bool isActive = crumb == Active;

					if (isHover || isActive) {
						// Select our area for background drawing
						CrumbHelper.RoundedRectangle (
							cr, area, m_Radius, corners);

						// Clear the background
						//cr.SetSourceRGB (1, 1, 1);
						//cr.FillPreserve ();

						// Style our background
						cr.Pattern =
							isActive ? m_SelectedBg : m_PrelightBg;
						cr.FillPreserve ();

						StateType state = isActive ?
							StateType.Selected : StateType.Active;

						// Draw outer line around it
						CairoHelper.SetSourceColor (cr, Style.Mid (state));
						cr.LineWidth = 1;
						cr.Stroke ();

						area.X += 1;
						area.Y += 1;
						area.Width -= 2;
						area.Height -= 2;

						// Draw our inner highlight line
						CrumbHelper.RoundedRectangle (
							cr, area, m_Radius, corners);
						CairoHelper.SetSourceColor (cr, Style.Light (state));
						cr.LineWidth = 1;
						cr.Stroke ();
					}

					// Draw our separator lines if needed
					if (i + 1 != m_Crumbs.Count) {
						cr.MoveTo (area.X + area.Width + 1, area.Y);
						cr.LineTo (area.X + area.Width + 1,
								   area.Y + area.Height);
						CairoHelper.SetSourceColor (
							cr, Style.Dark (StateType.Normal));
						cr.LineWidth = 1;
						cr.Stroke ();
	
						cr.MoveTo (area.X + area.Width + 1.5, area.Y);
						cr.LineTo (area.X + area.Width + 1.5,
								   area.Y + area.Height);
						CairoHelper.SetSourceColor (
							cr, Style.Light (StateType.Normal));
						cr.LineWidth = 1;
						cr.Stroke ();
					}
		
					this.PropagateExpose (crumb.Widget, evnt);

					i++;
				}
			}

			return true;
		}

		protected override void OnRealized ()
		{
			WindowAttr attributes = new WindowAttr ();
			WindowAttributesType attributes_mask;

			this.SetFlag (Gtk.WidgetFlags.Realized);

			attributes.WindowType = Gdk.WindowType.Child;
			attributes.X = this.Allocation.X;
			attributes.Y = this.Allocation.Y;
			attributes.Width = this.Allocation.Width;
			attributes.Height = this.Allocation.Height;
			attributes.Wclass = WindowClass.InputOutput;
			attributes.Visual = this.Visual;
			attributes.Colormap = this.Colormap;
			attributes.EventMask = (int)
				 (Gdk.EventMask.VisibilityNotifyMask
				| Gdk.EventMask.ExposureMask
				| Gdk.EventMask.ScrollMask
				| Gdk.EventMask.PointerMotionMask
                | Gdk.EventMask.LeaveNotifyMask
				| Gdk.EventMask.ButtonPressMask
				| Gdk.EventMask.ButtonReleaseMask
				| Gdk.EventMask.KeyPressMask
				| Gdk.EventMask.KeyReleaseMask);

			attributes_mask = WindowAttributesType.X
				| WindowAttributesType.Y
				| WindowAttributesType.Visual
				| WindowAttributesType.Colormap;

			this.GdkWindow = new Gdk.Window (this.ParentWindow,
											 attributes,
											 attributes_mask);
			this.GdkWindow.UserData = this.Handle;

			this.Style.Attach (this.GdkWindow);
			this.GdkWindow.SetBackPixmap (null, false);
			this.Style.SetBackground (this.GdkWindow, StateType.Normal);

			foreach (Crumb crumb in this.m_Crumbs) {
				this.PrepareCrumb (crumb);
			}
		}

		protected override void OnSizeAllocated (Gdk.Rectangle alloc)
		{
			base.OnSizeAllocated (alloc);

			// XXX: Not sure why we need to transpose this. Probably
			//      setting up our GdkWindow incorrectly.
			if (this.Toplevel as Gtk.Container != null) {
				Container parent = this.Toplevel as Container;
				alloc.X -= (int) parent.BorderWidth;
				alloc.Y -= (int) parent.BorderWidth;
			}

			int x = m_Xoffset;
			int y = m_Yoffset;

			// Create and cache our children allocation areas.
			foreach (Crumb crumb in this.m_Crumbs) {
				Gdk.Rectangle crumbAlloc = new Gdk.Rectangle ();

				crumbAlloc.X = x;
				crumbAlloc.Y = y;
				crumbAlloc.Width = m_Requests[crumb].Width;
				crumbAlloc.Height = alloc.Height - (y * 2);
				this.m_Coords[crumb] = crumbAlloc;
				crumb.Widget.SizeAllocate (crumbAlloc);

				x += crumbAlloc.Width + (2 * Spacing) + 2;
			}

			// Normal Crumb Background
			m_NormalBg = new Cairo.LinearGradient (
				alloc.X, alloc.Y, alloc.X, alloc.Y + alloc.Height);
			m_NormalBg.AddColorStop (0.3, m_NormalBgBegin);
			m_NormalBg.AddColorStop (0.9, m_NormalBgEnd);

			// Cursor Hover Crumb Background
			m_PrelightBg = new Cairo.LinearGradient (
				alloc.X, alloc.Y, alloc.X, alloc.Y + alloc.Height);
			m_PrelightBg.AddColorStop (0.3, m_PrelightBgBegin);
			m_PrelightBg.AddColorStop (0.9, m_PrelightBgEnd);

			// Selected Crumb Background
			m_SelectedBg = new Cairo.LinearGradient (
				alloc.X, alloc.Y, alloc.X, alloc.Y + alloc.Height);
			m_SelectedBg.AddColorStop (0.3, m_SelectedBgBegin);
			m_SelectedBg.AddColorStop (0.9, m_SelectedBgEnd);
		}

		protected override void OnSizeRequested (ref Gtk.Requisition req)
		{
			if (this.m_Crumbs.Count == 0) {
				req.Width = -1;
				req.Height = -1;
				return;
			}

			int childrenWidth = 0;
			int childrenHeight = 0;
			int childrenSpacing = 0;

			foreach (Crumb crumb in this.m_Crumbs) {
				Requisition childReq = crumb.Widget.SizeRequest ();

				childrenWidth += childReq.Width;
				childrenHeight = Math.Max (childrenHeight, childReq.Height);

				this.m_Requests[crumb] = new Gdk.Rectangle (
					0, 0, childReq.Width, childReq.Height);
			}

			childrenSpacing = (this.m_Crumbs.Count + 1) * m_Spacing;
			childrenSpacing += (this.m_Crumbs.Count - 1) * 2;

			req.Width = childrenWidth + childrenSpacing + (m_Xoffset * 2);
			req.Height = childrenHeight + (m_Yoffset * 2);

			// HACK: for our growth around the widget
			req.Width += 6;

			req.Width = Math.Max (req.Width, m_MinWidth);
			req.Height = Math.Max (req.Height, m_MinHeight);
		}

		protected override void OnStyleSet (Gtk.Style old_style)
		{
			base.OnStyleSet (old_style);

			// Normal Crumb Background
			this.m_NormalBgBegin = CrumbHelper.ToCairoColor (
				Style.Background (StateType.Normal), 1);
			this.m_NormalBgEnd = CrumbHelper.ToCairoColor (
				Style.Mid (StateType.Normal), 1);

			// Cursor Hover Crumb Background
			this.m_PrelightBgBegin = CrumbHelper.ToCairoColor (
				Style.Background (StateType.Prelight), 1);
			this.m_PrelightBgEnd = CrumbHelper.ToCairoColor (
				Style.Mid (StateType.Prelight), 1);

			// Selected Crumb Background
			this.m_SelectedBgBegin = CrumbHelper.ToCairoColor (
				Style.Light (StateType.Selected), 1);
			this.m_SelectedBgEnd = CrumbHelper.ToCairoColor (
				Style.Mid (StateType.Selected), 1);
		}


		protected override bool OnMotionNotifyEvent (EventMotion evnt)
		{
			foreach (Crumb crumb in this.m_Crumbs) {
				Gdk.Rectangle area = this.m_Coords[crumb];

				if (evnt.X >= area.X && evnt.X <= area.X + area.Width &&
					evnt.Y >= area.Y && evnt.Y <= area.Y + area.Height)
				{
					if (crumb != Hover) {
						this.m_Hover = crumb;
						this.QueueDraw ();
					}

					return true;
				}
			}

			this.m_Hover = null;
			this.QueueDraw ();

			return base.OnMotionNotifyEvent (evnt);
		}

        protected override bool OnLeaveNotifyEvent (EventCrossing evnt)
        {
            if (evnt.Mode == CrossingMode.Normal) {
                this.m_Hover = null;
                this.QueueDraw ();
            }

            return base.OnLeaveNotifyEvent (evnt);
        }

		protected override bool OnButtonReleaseEvent (EventButton evnt)
		{
			if (this.m_Hover != null) {
				this.Active = this.m_Hover;
			}

			return base.OnButtonReleaseEvent (evnt);
		}

        protected override bool OnScrollEvent (EventScroll evnt)
        {
            int active_index = this.m_Crumbs.IndexOf (this.Active);

            switch (evnt.Direction) {
                case ScrollDirection.Up:
                case ScrollDirection.Left:
                    if (active_index != 0)
                        this.Active = this.m_Crumbs[active_index - 1];
                    break;
                case ScrollDirection.Down:
                case ScrollDirection.Right:
                    if (active_index != this.m_Crumbs.Count - 1)
                        this.Active = this.m_Crumbs[active_index + 1];
                    break;
            }

			return base.OnScrollEvent (evnt);
        }

		private void PrepareCrumb (Crumb crumb)
		{
			crumb.Crumbs = this;
			crumb.Widget.Parent = this;
			crumb.Widget.ParentWindow = this.GdkWindow;
			crumb.Widget.Realize ();
		}

		private void UnprepareCrumb (Crumb crumb)
		{
			if (crumb.Widget.IsRealized)
				crumb.Widget.Unrealize ();

			crumb.Widget.ParentWindow = null;
			crumb.Widget.Parent = null;
			crumb.Crumbs = null;
		}
	}

	public enum Corners : byte
	{
		None        = 0,
		TopLeft     = 1,
		TopRight    = 2,
		BottomRight = 4,
		BottomLeft  = 8,
		All         = 15,
	}

	public class CrumbHelper
	{
		

		private static double m_Offset = 0.5;

		public static void RoundedRectangle (Context cr,
											 Gdk.Rectangle area,
											 int radius)
		{
			RoundedRectangle (cr, area, radius, Corners.All);
		}

		public static void RoundedRectangle (Context cr,
											 Gdk.Rectangle area,
											 int radius,
											 Corners corners)
		{
			// Top Line
			cr.MoveTo (area.X + radius + m_Offset, area.Y + m_Offset);
			cr.LineTo (area.X + area.Width - radius - m_Offset,
					   area.Y + m_Offset);

			// Top Right Corner
			if ((corners & Corners.TopRight) > 0) {
				cr.CurveTo (area.X + area.Width - radius - m_Offset,
							area.Y + m_Offset,
							area.X + area.Width,
							area.Y,
							area.X + area.Width - m_Offset,
							area.Y + radius + m_Offset);
			}
			else {
				cr.LineTo (area.X + area.Width - m_Offset,
						   area.Y + m_Offset);
				cr.LineTo (area.X + area.Width - m_Offset,
						   area.Y + m_Offset + radius);
			}

			// Right Line
			cr.LineTo (area.X + area.Width - m_Offset,
					   area.Y + area.Height - radius - m_Offset);

			// Bottom Right Corner
			if ((corners & Corners.BottomRight) > 0) {
				cr.CurveTo (area.X + area.Width - m_Offset,
							area.Y + area.Height - m_Offset - radius,
							area.X + area.Width,
							area.Y + area.Height,
							area.X + area.Width - m_Offset - radius,
							area.Y + area.Height - m_Offset);
			}
			else {
				cr.LineTo (area.X + area.Width - m_Offset,
						   area.Y + area.Height - m_Offset);
				cr.LineTo (area.X + area.Width - m_Offset - radius,
						   area.Y + area.Height - m_Offset);
			}

			// Bottom Line
			cr.LineTo (area.X + m_Offset + radius,
					   area.Y + area.Height - m_Offset);

			// Bottom Left Corner
			if ((corners & Corners.BottomLeft) > 0) {
				cr.CurveTo (area.X + m_Offset + radius,
							area.Y + area.Height - m_Offset,
							area.X,
							area.Y + area.Height,
							area.X + m_Offset,
							area.Y + area.Height - m_Offset - radius);
			}
			else {
				cr.LineTo (area.X + m_Offset,
						   area.Y + area.Height - m_Offset);
				cr.LineTo (area.X + m_Offset,
						   area.Y + area.Height - m_Offset - radius);
			}

			// Left Line
			cr.LineTo (area.X + m_Offset,
					   area.Y + m_Offset + radius);

			// Top Left Corner
			if ((corners & Corners.TopLeft) > 0) {
				cr.CurveTo (area.X + m_Offset,
							area.Y + m_Offset + radius,
							area.X,
							area.Y,
							area.X + m_Offset + radius,
							area.Y + m_Offset);
			}
			else {
				cr.LineTo (area.X + m_Offset, area.Y + m_Offset);
				cr.LineTo (area.X + m_Offset + radius, area.Y + m_Offset);
			}
		}

		public static Cairo.Color ToCairoColor (Gdk.Color color)
		{
			return ToCairoColor (color, 1.0);
		}

		public static Cairo.Color ToCairoColor (Gdk.Color color, double alpha)
		{
			return new Cairo.Color ((double) (color.Red   >> 8) / 255.0,
									(double) (color.Green >> 8) / 255.0,
									(double) (color.Blue  >> 8) / 255.0,
									alpha);
		}
	}
}

// Crumbs.cs
//
// Copyright © Mark A. Nicolosi <mark.a.nicolosi@gmail.com>
// Based on work by Christian Hergert <christian.hergert@gmail.com>
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

using Gdk;
using Gtk;

namespace Manico
{
    public class Crumbs : Gtk.HBox
    {
        public Crumbs () : base ()
        {
            base.BorderWidth = 2;
            base.Spacing = 2;
        }

        public new void Add (Widget widget)
        {
            ToggleButton toggle_button = new ToggleButton ();
            toggle_button.Relief = ReliefStyle.None;
            toggle_button.Add (widget);
            toggle_button.Show ();

            toggle_button.Toggled += delegate (object sender, EventArgs args) {
                if (toggle_button.Active) {
                    foreach (ToggleButton tb in this)
                        if (tb != sender)
                            tb.Active = false;
                }
            };

            base.PackStart (toggle_button, false, false, 0);
        }

		protected override bool OnExposeEvent (Gdk.EventExpose evnt)
		{
            Rectangle area = base.Allocation;
            Style.PaintBox (base.Style, base.GdkWindow, StateType.Normal,
                ShadowType.In, area, this, "buttondefault",
                area.X, area.Y, area.Width, area.Height);

            List<Widget> children = new List<Widget> ();
            foreach (Widget child in this)
                children.Add (child);

            for (int i = 0; i < (children.Count - 1); i++) {
                Widget child = children[i];
                int x = child.Allocation.X + child.Allocation.Width;
                Style.PaintVline (base.Style, base.GdkWindow,
                StateType.Normal, area, this, "buttondefault",
                area.Y + 1, area.Y + area.Height - 1, x);
            }

			return base.OnExposeEvent (evnt);
		}
    }
}


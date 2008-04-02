// Crumb.cs
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

using Gtk;

namespace Manico
{
	public class Crumb : Gtk.Object
	{
		public event EventHandler Clicked;

		private Gtk.Widget m_Widget;
		private Crumbs     m_Crumbs;

		public Crumb ()
		{
			this.m_Widget = new Gtk.Label ();
			this.m_Widget.Show ();
		}

		public Crumb (string label) : this ()
		{
			(this.m_Widget as Gtk.Label).Markup = label;
		}

		public Crumbs Crumbs {
			get {
				return this.m_Crumbs;
			}
			set {
				this.m_Crumbs = value;
			}
		}

		public string Label {
			get {
				if (this.m_Widget as Label != null) {
					return (this.m_Widget as Gtk.Label).Text;
				}

				return String.Empty;
			}
		}

		public Gtk.Widget Widget {
			get {
				return this.m_Widget;
			}
			set {
				this.m_Widget = value;

				if (this.m_Crumbs != null) {
					this.m_Crumbs.QueueResize ();
				}
			}
		}

		public void EmitClicked ()
		{
			if (this.Clicked != null) {
				this.Clicked (this, new EventArgs ());
			}
		}
	}
}


// EntryPoint.cs
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

using Manico;

class EntryPoint
{
    public static void Main ()
    {
        Gtk.Window window;
        Gtk.VBox   vbox;
        Crumbs     crumbs;
        Crumb      home, folder1, folder2, worksheet;

        Application.Init ();

        // Outer Window
        window = new Gtk.Window ("Crumbs Test");
        window.BorderWidth = 12;
        window.SetDefaultSize (640, 480);
        window.Destroyed += OnDestroy;
        window.Show ();

        // Main VBox
        vbox = new Gtk.VBox ();
        window.Add(vbox);
        vbox.Show ();

        // Crumbs widget
        crumbs = new Crumbs ();
        vbox.PackStart (crumbs, false, false, 0);
        crumbs.Show ();

        // Home Button
        home = new Crumb ();
        home.Widget = new Gtk.Image (Gtk.Stock.Home, Gtk.IconSize.Menu);
        home.Widget.Show ();
        crumbs.Append (home);

        // Folder1
        folder1 = new Crumb ();
        HBox hbox1 = new HBox ();
        hbox1.Spacing = 3;
        Gtk.Image img1 = new Gtk.Image (Stock.Directory, IconSize.Menu);
        hbox1.PackStart (img1, false, true, 0);
        hbox1.PackStart (new Label ("Documents"));
        hbox1.ShowAll ();
        folder1.Widget = hbox1;
        crumbs.Append (folder1);

        // Folder2
        folder2 = new Crumb ();
        HBox hbox2 = new HBox ();
        hbox2.Spacing = 3;
        Gtk.Image img2 = new Gtk.Image (Stock.Directory, IconSize.Menu);
        hbox2.PackStart (img2, false, true, 0);
        hbox2.PackStart (new Label ("Spreadsheets"));
        hbox2.ShowAll ();
        folder2.Widget = hbox2;
        crumbs.Append (folder2);

        // Worksheet
        worksheet = new Crumb ("Worksheet");
        crumbs.Append (worksheet);

        crumbs.Active = folder1;
        crumbs.Changed += OnChanged;

        Application.Run ();
    }

    static void OnDestroy (object sender, EventArgs args)
    {
        Application.Quit ();
    }

    static void OnChanged (object sender, EventArgs args)
    {
        Console.WriteLine ("Crumb changed");
    }
}


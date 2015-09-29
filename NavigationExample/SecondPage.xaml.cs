using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace NavigationExample
{
	public partial class SecondPage : ContentPage
	{
		public SecondPage ()
		{
			NavigationPage.SetHasNavigationBar (this, false);
			InitializeComponent ();
			previous.Clicked += delegate(object sender, EventArgs e) {
				Navigation.PopAsync ();
			};
		}
	}
}


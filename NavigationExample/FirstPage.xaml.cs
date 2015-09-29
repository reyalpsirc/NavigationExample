using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace NavigationExample
{
	public partial class FirstPage : ContentPage
	{
		public FirstPage ()
		{
			NavigationPage.SetHasNavigationBar (this, false);
			InitializeComponent ();
			next.Clicked += delegate(object sender, EventArgs e) {
				Navigation.PushAsync (new SecondPage ());
			};
		}
	}
}


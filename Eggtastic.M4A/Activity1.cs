using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Microsoft.Xna.Framework;

namespace Eggtastic.M4A
{
	[Activity(Label = "Eggtastic", MainLauncher = true, Icon = "@drawable/icon")]
	public class Activity1 : AndroidGameActivity
	{
		Eggtastic.Game1 game;
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			Eggtastic.Game1.Activity = this;
			game = new Game1();
			SetContentView(game.Window);
			game.Run();
		}

		protected override void OnPause()
		{
			base.OnPause();
			game.Window.Pause();
		}

		protected override void OnResume()
		{
			base.OnResume();
			game.Window.Resume();
		}
	}
}


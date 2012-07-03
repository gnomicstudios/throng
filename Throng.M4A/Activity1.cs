using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Microsoft.Xna.Framework;

namespace Throng.M4A
{
	[Activity(Label = "Throng", MainLauncher = true, Icon = "@drawable/icon")]
	public class Activity1 : AndroidGameActivity
	{
		Throng.Game1 game;
		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			Throng.Game1.Activity = this;
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


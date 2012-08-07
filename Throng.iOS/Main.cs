using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Throng
{
	[Register("AppDelegate")]
	public class Application : UIApplicationDelegate
	{
		Game1 game;
        public override void FinishedLaunching(UIApplication app)
        {
            // Fun begins..
            game = new Game1();
            game.Run();
        }

		// This is the main entry point of the application.
		static void Main (string[] args)
		{
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main (args, null, "AppDelegate");
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Throng
{
    public class Tweak
    {
        //turn to true to test movement without enemies bothering you
        public static bool PermanentlyFreezeEnemies = false;
//        public static bool PermanentlyFreezeEnemies = true;

        //shows additional text for debugging
        public static bool ShowStats = false;
        public static string Stats = "";

        //player movement speed default 10
        public static float MOVEMENT_FORCE = 10.0f;
        //        public static float MOVEMENT_FORCE = 50.0f;

        //defaults to 1
        public static float SUCK_SPEED_MULTIPLIER = 1.0f;
        //        public static float SUCK_SPEED_MULTIPLIER = 50.0f;

        //defaults to 1, 5 is very big
        public static float SUCK_AREA_MULTIPLER = 1.0f;
        //        public static float SUCK_AREA_MULTIPLER = 5.0f;

        //defaults to 1
        public static int STARTING_EGGS = 1;
        //        public static int STARTING_EGGS = 10;

        //default to false, press f1 to toggle physics view
       public static bool SHOW_PHYSICS_ON_START = false;
//        public static bool SHOW_PHYSICS_ON_START = true;

       public static void Init()
       {
           //Switch to true to test some high values out
           bool testMaxLevel = false;
//           bool testMaxLevel = true;
           if (testMaxLevel)
           {
               MOVEMENT_FORCE = 50.0f;
               SUCK_SPEED_MULTIPLIER = 50.0f;
               SUCK_AREA_MULTIPLER = 5.0f;
               STARTING_EGGS = 10;
           }
       }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPtest1
{
    class GSM
    {
        /// <summary>
        /// Gamestate changes in response to events in the MainPage Code
        /// </summary>
        public static void gsm()
        {
            if (MainPage.roundEnded == true && (MainPage.gameState == 1))
            { 
                MainPage.BG = MainPage.scoreScreen;
            }
            else
            {
                if (MainPage.gameState == 0)
                {
                    MainPage.BG = MainPage.startScreen;
                }
                else if (MainPage.gameState == 1)
                {
                    MainPage.BG = MainPage.levelScreen;
                }
                else if(MainPage.gameState == 2)
                {
                    MainPage.BG = MainPage.pauseScreen;
                }
            }

        }
    }
}

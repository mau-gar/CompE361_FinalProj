using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWPtest1
{
    class GSM
    {
        public static void gsm()
        {
            if (MainPage.roundEnded == true)
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
            }

        }
    }
}

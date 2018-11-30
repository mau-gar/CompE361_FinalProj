using System;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Core;
using UWPtest1.Class;
using System.Collections.Generic;

namespace UWPtest1
{
    public sealed partial class MainPage : Page
    {
        public static CanvasBitmap BG, startScreen, levelScreen, scoreScreen, photon, enemy1, enemy2, enemyImage, tower;
        public static Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
        public static float designWidth = 1280;
        public static float designHeight = 720;
        public static float scaledWidth, scaledHeight;//Used to auto scale image when changing screen resolution 
        public static int gameState = 0;//startScreen set 0

        /*
         * centerX and centerY define the position where the photons originate(origin)  
         * pointX and pointY are the positon of the fired pjojectile releative to the origin
         */ 
        public static float pointX, pointY, centerX, centerY;

        /// Variables for Timers 
        public static int countDown = 60;
        public static bool roundEnded = false;
        public static DispatcherTimer roundTimer = new DispatcherTimer();
        public static DispatcherTimer enemyTimer = new DispatcherTimer();//Enemy timer

        //photon(Projectile)
        public static List<float> photonXPOS = new List<float>();//LL containing xcoordinate of tapped photon 
        public static List<float> photonYPOS = new List<float>();

        //Enemies(Stored in a LL)
        public static List<float> enemyXPOS = new List<float>();
        public static List<float> enemyYPOS = new List<float>();
        public static List<int> enemyImageLL = new List<int>();
        public static List<string> enemyDirection = new List<string>();

        //Random Generator 
        public Random enemyImageRand = new Random();//Randomly Select which image(enemy type)
        public Random enemyGenRand = new Random();//Generation Interval(Frequency of enemy spawns)
        public Random enemyStartPos = new Random();//Random Starting Position for enemies TODO: Make enemies spawn randomly at specified locaitons 

        /*
         * Represents the screen dimension percentage of where you tapped vs. where the photons originate
         * Higher Coordinate position = Higher percent = faster speed
         * Lower Coordinate position = Lower percent = slower speed
         */
        public static List<float> percent = new List<float>(); 

        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += Current_SizeChanged;//Subsribing an window size change event
            Scaling.SetScale();//Set scale upon intialization
            //Set Center Coordinate after scaling 
            centerX = (float)bounds.Width / 2;
            centerY = (float)bounds.Height / 2;
            roundTimer.Tick += RoundTimer_Tick;
            roundTimer.Interval = new TimeSpan(0,0,1);
            enemyTimer.Tick += EnemyTimer_Tick;
            enemyTimer.Interval = new TimeSpan(0, 0, 0, 0, enemyGenRand.Next(300,3000));//enemy attack time between 300 and 3000 ms
        }

        //Spawn location for enemies-TODO: Makes them spawn off screen and at 'four corners'
        private void EnemyTimer_Tick(object sender, object e)
        {   
            int enemySelect = enemyImageRand.Next(1,3);//Cycles between enemies, in this case two enemies
            Random startingCorner = new Random();//Randomly Select Which Corner to Start
            int corner = startingCorner.Next(1,5);
            int start;

            if (corner == 1)//Top
            {
                start = enemyStartPos.Next(0, (int)bounds.Width);
                enemyXPOS.Add(start);
                enemyYPOS.Add(50 * scaledWidth);
            }

            else if (corner == 2)//Left
            {
                start = enemyStartPos.Next(0, (int)bounds.Height);
                enemyYPOS.Add(start);
                enemyXPOS.Add(50 * scaledWidth);
            }

            else if(corner == 3)//Right
            {

            }

            else if(corner == 4)//Bottom
            {

            }
            //enemyXPOS.Add(50 * scaledWidth);//This will make them spawn in the upper left 
            //enemyYPOS.Add(119 * scaledHeight);
            enemyImageLL.Add(enemySelect);//Add selected enemy to LL
            enemyTimer.Interval = new TimeSpan(0,0,0,0,enemyGenRand.Next(300,700));
        }

        private void RoundTimer_Tick(object sender, object e)
        {
            countDown -= 1;//Ticks down by one second 
            if (countDown < 1)//Once count == 0, update
            {
                roundTimer.Stop();
                roundEnded = true;
            }
        }

        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            Scaling.SetScale();//Whenever window size changes update scaling size
            //Update origin (centerX and centerY) after a window change
            centerX = (float)bounds.Width / 2;
            centerY = (float)bounds.Height / 2;
        }

        private void GameCanvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        async Task CreateResourcesAsync(CanvasControl sender)//Images in the game
        {
            startScreen = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/Yoda.png"));
            levelScreen = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/obi-wan.jpg"));
            scoreScreen = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/Droideka.jpg"));
            photon = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/photon.jpg"));
            enemy1 = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/enemy.png"));
            enemy2 = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/enemy2.png"));
            tower = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/tower.jpg"));
        }

        private void GameCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            GSM.gsm(); //Call GSM on first draw
            args.DrawingSession.DrawImage(Scaling.Img(BG));//Generate Background upon method call
            if (gameState > 0)
            {
                args.DrawingSession.DrawImage(Scaling.Img(tower),(float)bounds.Width/2 - (75 * scaledWidth),(float)bounds.Height/2 - (75 * scaledHeight));
                //Drawing Enemies first so projectiles superimpose the enemy images  
                for (int j = 0; j < enemyXPOS.Count; j++)
                {
                    if (enemyImageLL[j] == 1)
                    {
                        enemyImage = enemy1;
                    }
                    if (enemyImageLL[j] == 2)
                    {
                        enemyImage = enemy2;
                    }
                    //enemyXPOS[j] += 3;//Enemy Movement 
                    args.DrawingSession.DrawImage(Scaling.Img(enemyImage), enemyXPOS[j], enemyYPOS[j]);
                }
                //Display photon
                for (int i = 0; i < photonXPOS.Count; i++)
                {
                    //Linear Interpolation to calculate the projectile path between two known coordinates
                    pointX = (centerX + (photonXPOS[i] - centerX) * percent[i]);
                    pointY = (centerY + (photonYPOS[i] - centerY) * percent[i]);
                    args.DrawingSession.DrawImage(Scaling.Img(photon), pointX - (19 * scaledWidth), pointY - (20 * scaledHeight));//19 and 20 come from half of the width and height of the photon.jpg
                    percent[i] += (0.1f * scaledHeight);
                    if (pointY < 0f)//If photon trails off screen, remove the photon 
                    {
                        photonXPOS.RemoveAt(i);
                        photonYPOS.RemoveAt(i);
                        percent.RemoveAt(i);
                    }
                }
            }
            GameCanvas.Invalidate();
        }

        private void GameCanvas_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (roundEnded == true)//When timer ends game state returns to main menu when tapped
            {
                gameState = 0;
                roundEnded = false;
                countDown = 60;

                //Stop and Clear Enemy Timer and Enemies 
                enemyTimer.Stop();
                enemyXPOS.Clear();
                enemyYPOS.Clear();
                enemyImageLL.Clear();
            }
            else
            {
                if (gameState == 0)
                {
                    gameState += 1;
                    roundTimer.Start();//At levelScreen begin timer
                    enemyTimer.Start();
                }
                else if (gameState > 0)//If you are not on the start screen
                {
                    //When screen is tapped add X/Y position of tap to photon list
                    photonXPOS.Add((float)e.GetPosition(GameCanvas).X);
                    photonYPOS.Add((float)e.GetPosition(GameCanvas).Y);
                    percent.Add(0f);//Start at 0%
                }
            }
        }
    }
}

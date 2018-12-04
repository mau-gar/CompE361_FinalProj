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
using Windows.UI;

namespace UWPtest1
{
    public sealed partial class MainPage : Page
    {
        public static CanvasBitmap BG, startScreen, levelScreen, scoreScreen, photon, enemy1, enemy2, enemyImage, tower, blood;
        public static Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
        public static float designWidth = 1280;
        public static float designHeight = 720;
        public static float scaledWidth, scaledHeight;//Used to auto scale image when changing screen resolution 
        public static int gameState = 0;//startScreen set 0
        public static float myScore;

        /*
         * centerX and centerY define the position where the photons originate(origin)  
         * pointX and pointY are the positon of the fired pjojectile releative to the origin
         */ 
        public static float pointX, pointY, centerX, centerY;

        //Collision/Death Variable 
        public static float deathAnimationX, deathAnimationY;//Death Animation
        public static int deathAniFrames = 60;//Duration(in frames) of the death animation - 60 frames @ 60FPS = 1 sec animation 

        /// Variables for Timers 
        public static int countDown = 60;
        public static bool roundEnded = false;
        public static DispatcherTimer roundTimer = new DispatcherTimer();
        public static DispatcherTimer enemyTimer = new DispatcherTimer();//Enemy timer

        //Photon(Projectile)
        public static List<float> photonXPOS = new List<float>();//LL containing xcoordinate of tapped photon 
        public static List<float> photonYPOS = new List<float>();

        //Enemies(Stored in a LL)
        public static List<float> enemyXPOS = new List<float>();
        public static List<float> enemyYPOS = new List<float>();
        public static List<int> enemyImageLL = new List<int>();
        public static List<int> enemyDirection = new List<int>();

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

        private void EnemyTimer_Tick(object sender, object e)
        {   
            int enemySelect = enemyImageRand.Next(1,3);//Cycles between enemies, in this case two enemies
            Random startingCorner = new Random();//Randomly Select Which Corner to Start
            int corner = startingCorner.Next(1,5);

            if (corner == 1)//Top
            {
                enemyXPOS.Add((int)bounds.Width/2);
                enemyYPOS.Add(-25 * scaledHeight);//Start at -25 so enemies 'move' into view
            }
            else if (corner == 2)//Left
            {
                enemyYPOS.Add((int)bounds.Height/2);
                enemyXPOS.Add(-50 * scaledWidth);//Start at -50 so enemies 'move' into view
            }
            else if (corner == 3)//Right
            {
                enemyYPOS.Add((int)bounds.Height/2);
                enemyXPOS.Add((designWidth + 50) * scaledWidth);//Start at designWidth + 50 so enemies 'move' into view
            }
            else if(corner == 4)//Bottom
            {
                enemyXPOS.Add((int)bounds.Width/2);
                enemyYPOS.Add((designHeight + 25) * scaledHeight);//Start at designHeight +25 so enemies 'move' into view
            }
            enemyDirection.Add(corner);//Track enemy spawn location to determine movement(vertical or horizontal)
            enemyImageLL.Add(enemySelect);//Add selected enemy to LL
            enemyTimer.Interval = new TimeSpan(0,0,0,0,enemyGenRand.Next(300,1700));//Spawn interval (1700)
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
            blood = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/blood.png"));
        }

        private void GameCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            GSM.gsm(); //Call GSM on first draw
            args.DrawingSession.DrawImage(Scaling.Img(BG));//Generate Background upon method call
            if (gameState > 0)//If we are playing the game
            {
                args.DrawingSession.DrawImage(Scaling.Img(tower),(float)bounds.Width/2 - (75 * scaledWidth),(float)bounds.Height/2 - (75 * scaledHeight));
                args.DrawingSession.DrawText("Current Score " + myScore.ToString(), (float)bounds.Width/2, 10, Color.FromArgb(255,255,255,255));//Draw score
                //Draw Death animation first so death animation superimposes the enemy image

                if ((deathAnimationX > 0) && (deathAnimationY > 0) && (deathAniFrames > 0))//If we have a 'dead enemy' draw death animation
                {
                    args.DrawingSession.DrawImage(Scaling.Img(blood), deathAnimationX, deathAnimationY);
                    deathAniFrames -= 1;
                }
                else//Reset death animaiton variables 
                {
                    deathAniFrames = 60;
                    deathAnimationX = 0;
                    deathAnimationY = 0;
                }

                //Drawing Enemies second so projectiles superimpose the enemy images  
                for (int j = 0; j < enemyXPOS.Count; j++)
                {
                    //If an enemy makes it to the tower GAME OVER
                    if ((centerX >= enemyXPOS[j]) && (centerX <= (enemyXPOS[j] + (100 * scaledWidth))) && (centerY >= enemyYPOS[j]) && (centerY <= (enemyYPOS[j] + (75 * scaledHeight))))
                    {
                        roundEnded = true;
                        break;
                    }

                    //Enemy Image Select
                    if (enemyImageLL[j] == 1)
                    {
                        enemyImage = enemy1;
                    }
                    else if (enemyImageLL[j] == 2)
                    {
                        enemyImage = enemy2;
                    }
                    
                    //Enemy Direction Select
                    if(enemyDirection[j] == 1)//Start top, move down  
                    {
                        enemyYPOS[j] += 1;
                    }
                    else if(enemyDirection[j] == 2)//Start left, move right
                    {
                        enemyXPOS[j] += 2;
                    }
                    else if(enemyDirection [j] == 3)//Start right, move left
                    {
                        enemyXPOS[j] -= 2;
                    }
                    else if(enemyDirection [j] == 4)//Start down, move up
                    {
                        enemyYPOS[j] -= 1;
                    }
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

                    //Every time a projectile is fired check for collision 
                    for(int j = 0; j < enemyXPOS.Count; j++)//For each projectile check collision cooridantes of every enemy 
                    {
                        //Note: 100 and 95 in the following if statement are relative to the blood spatter dimension image
                        //Compare current projectile x/y coordinate with every enemy x/y position to check for a collision 
                        if((pointX >= enemyXPOS[j]) && (pointX <= (enemyXPOS[j] + (100 * scaledWidth))) && (pointY >= enemyYPOS[j]) && (pointY <= (enemyYPOS[j] + (95 * scaledHeight))))
                        {
                            //Update death animation locaiton for 'blood splatter'
                            deathAnimationX = pointX - (50 * scaledWidth);
                            deathAnimationY = pointY - (47 * scaledHeight);

                            //Remove the enemy off screen
                            enemyXPOS.RemoveAt(j);
                            enemyYPOS.RemoveAt(j);
                            enemyImageLL.RemoveAt(j);
                            enemyDirection.RemoveAt(j);

                            //Remove the current projectile off screen
                            photonXPOS.RemoveAt(i);
                            photonYPOS.RemoveAt(i);
                            percent.RemoveAt(i);

                            myScore += 100;
                            break;
                        }
                    }

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
                enemyDirection.Clear();
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

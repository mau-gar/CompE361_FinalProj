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
using Microsoft.Graphics.Canvas.Text;

namespace UWPtest1
{
    public sealed partial class MainPage : Page
    {
        public static CanvasBitmap BG, startScreen, levelScreen, pauseScreen, scoreScreen, arrow, enemy1, enemy2, enemyImage, tower, blood;
        public static Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
        public static float designWidth = 1920;
        public static float designHeight = 1080;
        public static float scaledWidth, scaledHeight;//Used to auto scale image when changing screen resolution 
        public static int gameState = 0;//startScreen set 0
        public static float myScore, myLevel = 1;
        public static float accuracy, arrowsFired, arrowsHit;
        public static bool levelUp = false;

        /*
         * centerX and centerY define the position where the arrows originate(origin)  
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

        //Arrow(Projectile)
        public static List<float> arrowXPOS = new List<float>();//LL containing xcoordinate of tapped arrow 
        public static List<float> arrowYPOS = new List<float>();

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
         * Represents the screen dimension percentage of where you tapped vs. where the arrows originate
         * Higher Coordinate position = Higher percent = faster speed
         * Lower Coordinate position = Lower percent = slower speed
         */
        public static List<float> percent = new List<float>();

        /// <summary>
        /// Initializing Scaling Components, Centers, and Timer Components (Tick Events & Tick Intervals)
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += Current_SizeChanged;//Subsribing an window size change event
            Scaling.SetScale();//Set scale upon intialization
            //Set Center Coordinate after scaling 
            centerX = (float)bounds.Width / 2;
            centerY = (float)bounds.Height / 2;
            roundTimer.Tick += RoundTimer_Tick;
            roundTimer.Interval = new TimeSpan(0, 0, 1);
            enemyTimer.Tick += EnemyTimer_Tick;
            enemyTimer.Interval = new TimeSpan(0, 0, 0, 0, enemyGenRand.Next(300, 3005-(int)myLevel*5));//enemy attack time between 300 a level-varying time for increased frequency the futher you go
        }

        /// <summary>
        /// Utilizing two random generators to cycle between corner positions and listing their direction for movemement as tick event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnemyTimer_Tick(object sender, object e)
        {
            int enemySelect = enemyImageRand.Next(1, 3);//Cycles between enemies, in this case two enemies
            Random startingCorner = new Random();//Randomly Select Which Corner to Start
            int corner = startingCorner.Next(1, 5);

            if (corner == 1)//Top
            {
                enemyXPOS.Add((int)bounds.Width / 2);
                enemyYPOS.Add(-25 * scaledHeight);//Start at -25 so enemies 'move' into view
            }
            else if (corner == 2)//Left
            {
                enemyYPOS.Add((int)bounds.Height / 2 - 30);
                enemyXPOS.Add(-50 * scaledWidth);//Start at -50 so enemies 'move' into view
            }
            else if (corner == 3)//Right
            {
                enemyYPOS.Add((int)bounds.Height / 2 - 15);
                enemyXPOS.Add((designWidth + 50) * scaledWidth);//Start at designWidth + 50 so enemies 'move' into view
            }
            else if (corner == 4)//Bottom
            {
                enemyXPOS.Add((int)bounds.Width / 2 - 40);
                enemyYPOS.Add((designHeight + 25) * scaledHeight);//Start at designHeight +25 so enemies 'move' into view
            }
            enemyDirection.Add(corner);//Track enemy spawn location to determine movement(vertical or horizontal)
            enemyImageLL.Add(enemySelect);//Add selected enemy to LL
            enemyTimer.Interval = new TimeSpan(0, 0, 0, 0, enemyGenRand.Next(300, 1700));//Spawn interval (1700)
        }

        /// <summary>
        /// Utilized to timer interval to create level up logic and end game definitively
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RoundTimer_Tick(object sender, object e)
        {
            countDown -= 1;//Ticks down by one second 
            
            if (countDown % 20 == 0)
            {

                levelUp = true;
                myLevel++;
            }
            else
                levelUp = false;

        }

        /// <summary>
        /// Retrieves and updates current window measurements to re-scale live 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            Scaling.SetScale();//Whenever window size changes update scaling size
            //Update origin (centerX and centerY) after a window change
            centerX = (float)bounds.Width / 2;
            centerY = (float)bounds.Height / 2;
        }

        /// <summary>
        /// Helper Method for Current_SizeChanged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void GameCanvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResourcesAsync(sender).AsAsyncAction());
        }

        async Task CreateResourcesAsync(CanvasControl sender)//Images in the game
        {
            startScreen = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/Title.png"));
            levelScreen = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/Map.png"));
            pauseScreen = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/PauseScreen.png"));
            scoreScreen = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/GameOver.png"));
            arrow = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/arrow.png"));
            enemy1 = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/enemyT.png"));
            enemy2 = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/enemyH.png"));
            blood = await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:////Assets/Images/blood.png"));
        }

        /// <summary>
        /// Graphical Logic, creating visuals in responses to events, and incorporating edge collision logic 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void GameCanvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            GSM.gsm(); //Call GSM on first draw
            args.DrawingSession.DrawImage(Scaling.Img(BG));//Generate Background upon method call
            if (gameState == 1)//If we are playing the game
            {
                CanvasTextLayout textScore = new CanvasTextLayout(args.DrawingSession, myScore.ToString(), new CanvasTextFormat() { FontFamily = "", FontSize = 40 * scaledHeight, WordWrapping = CanvasWordWrapping.NoWrap }, 0.0f, 0.0f);
                CanvasTextLayout textLevel = new CanvasTextLayout(args.DrawingSession, "Level " + myLevel.ToString() + " Speed Increasing!", new CanvasTextFormat() { FontFamily = "Times New Roman", FontSize = 60 * scaledHeight, WordWrapping = CanvasWordWrapping.NoWrap }, 0.0f, 0.0f);
                args.DrawingSession.DrawTextLayout(textScore, (150 * scaledWidth), (50 * scaledHeight), Color.FromArgb(255, 255, 255, 255));//Draw score

                if (levelUp) // Level Up Notifier
                {
                    for(int i =0; i < 3; i++)
                    args.DrawingSession.DrawTextLayout(textLevel, centerX - 250, centerY - 100, Color.FromArgb(255, 30, 25, 150));//Draw Level Up Sequence
                }

                if (roundEnded == true) //Results Screen Drawing Events
                {
                    accuracy = arrowsHit / arrowsFired * 100;
                    CanvasTextLayout textAccuracy = new CanvasTextLayout(args.DrawingSession, "Accuracy: " + accuracy.ToString() + "% ", new CanvasTextFormat() { FontSize = 40 * scaledHeight, WordWrapping = CanvasWordWrapping.NoWrap }, 0.0f, 0.0f);
                    CanvasTextLayout textFinalScore = new CanvasTextLayout(args.DrawingSession, "Final Score: " + myScore.ToString(), new CanvasTextFormat() { FontFamily = "", FontSize = 40 * scaledHeight, WordWrapping = CanvasWordWrapping.NoWrap }, 0.0f, 0.0f);
                    args.DrawingSession.DrawTextLayout(textFinalScore, (790 * scaledWidth), (645 * scaledHeight), Color.FromArgb(255, 150, 5, 5));
                    args.DrawingSession.DrawTextLayout(textAccuracy, (790 * scaledWidth), (685 * scaledHeight), Color.FromArgb(255, 150, 5, 5));
                }

                //Draw Death animation first so death animation superimposes the enemy image
                if ((deathAnimationX > 0) && (deathAnimationY > 0) && (deathAniFrames > 0))//If we have a 'dead enemy' draw death animation
                {
                    args.DrawingSession.DrawImage(Scaling.Img(blood), deathAnimationX - 80, deathAnimationY - 80);
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
                        countDown = 0;
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
                    if (enemyDirection[j] == 1)//Start top, move down  
                    {
                        enemyYPOS[j] += 1 + (float)(myLevel * 0.1);
                    }
                    else if (enemyDirection[j] == 2)//Start left, move right
                    {
                        enemyXPOS[j] += 2 + (float)(myLevel * 0.2);
                    }
                    else if (enemyDirection[j] == 3)//Start right, move left
                    {
                        enemyXPOS[j] -= 2 + (float)(myLevel * 0.2);
                    }

                    else if (enemyDirection[j] == 4)//Start down, move up
                    {
                        enemyYPOS[j] -= 1 + (float)(myLevel * 0.1);
                    }
                    args.DrawingSession.DrawImage(Scaling.Img(enemyImage), enemyXPOS[j], enemyYPOS[j]);
                }

                //Display arrow
                for (int i = 0; i < arrowXPOS.Count; i++)
                {
                    //Linear Interpolation to calculate the projectile path between two known coordinates
                    pointX = (centerX + (arrowXPOS[i] - centerX) * percent[i]);
                    pointY = (centerY + (arrowYPOS[i] - centerY) * percent[i]);
                    args.DrawingSession.DrawImage(Scaling.Img(arrow), pointX - (19 * scaledWidth), pointY - (20 * scaledHeight));//19 and 20 come from half of the width and height of the arrow.jpg

                    percent[i] += (0.1f * scaledHeight);

                    //Every time a projectile is fired check for collision 
                    for (int j = 0; j < enemyXPOS.Count; j++)//For each projectile check collision cooridantes of every enemy 
                    {
                        //Note: 100 and 95 in the following if statement are relative to the blood spatter dimension image
                        //Compare current projectile x/y coordinate with every enemy x/y position to check for a collision 
                        if ((pointX >= enemyXPOS[j]) && (pointX <= (enemyXPOS[j] + (100 * scaledWidth))) && (pointY >= enemyYPOS[j]) && (pointY <= (enemyYPOS[j] + (95 * scaledHeight))))
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
                            arrowXPOS.RemoveAt(i);
                            arrowYPOS.RemoveAt(i);
                            percent.RemoveAt(i);

                            myScore += 100;
                            arrowsHit++;
                            break;
                        }
                    }

                    if (pointY < 0f)//If arrow trails off screen, remove the arrow 
                    {
                        arrowXPOS.RemoveAt(i);
                        arrowYPOS.RemoveAt(i);
                        percent.RemoveAt(i);
                    }
                }
            }
          
            GameCanvas.Invalidate();
        }

        /// <summary>
        /// Click Event Logic Response, Button Mapping, and varying functionality depending on Game State
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameCanvas_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var tapPos = e.GetPosition(GameCanvas);

            if (gameState == 0) //Menu
            {
                roundEnded = false;
                gameState = 1;
                roundTimer.Start();//At levelScreen begin timer
                enemyTimer.Start();
            }
            else if (gameState == 1)//Play Level Screen
            {
                if (roundEnded == true) // Game Over, Tower was attacked
                { 
                    gameState = 0;

                    myScore = 0;
                    myLevel = 1;
                    arrowsFired = 0;
                    arrowsHit = 0;

                    enemyTimer.Stop();
                    enemyXPOS.Clear();
                    enemyYPOS.Clear();
                    enemyDirection.Clear();
                    enemyImageLL.Clear();
                }
                   
               //Pause Menu Logic
                if ((float)tapPos.X > (1770 * scaledWidth) && (float)tapPos.X < (1878 * scaledWidth) && (float)tapPos.Y > (54 * scaledHeight) && (float)tapPos.Y < (162 * scaledHeight))
                {
                    gameState = 2;

                    enemyXPOS.Clear();
                    enemyYPOS.Clear();
                    enemyTimer.Stop();
                    enemyDirection.Clear();
                    enemyImageLL.Clear();
                }
                    //When screen is tapped add X/Y position of tap to arrow list
                    arrowXPOS.Add((float)e.GetPosition(GameCanvas).X);
                    arrowYPOS.Add((float)e.GetPosition(GameCanvas).Y);
                    percent.Add(0f);//Start at 0%
                    arrowsFired++;
                    
            }
            else if (gameState == 2) // Pause Screen
            {
                    
                    if ((float)tapPos.X > (744 * scaledWidth) && (float)tapPos.X < (1176 * scaledWidth) && (float)tapPos.Y > (324 * scaledHeight) && (float)tapPos.Y < (396 * scaledHeight)) //Continue
                    {
                        gameState = 1;

                        countDown = int.MaxValue - 8;
                        roundTimer.Start();
                        enemyTimer.Start();
                    }

                    if ((float)tapPos.X > (744 * scaledWidth) && (float)tapPos.X < (1176 * scaledWidth) && (float)tapPos.Y > (468 * scaledHeight) && (float)tapPos.Y < (540 * scaledHeight)) //Reset
                    {
                        gameState = 1;
 
                        myScore = 0;
                        myLevel = 1;
                        arrowsFired = 0;
                        arrowsHit = 0;
                        countDown = int.MaxValue - 8;
                        roundTimer.Start();
                        enemyTimer.Start();
                    }

                    if ((float)tapPos.X > (744 * scaledWidth) && (float)tapPos.X < (1176 * scaledWidth) && (float)tapPos.Y > (612 * scaledHeight) && (float)tapPos.Y < (684 * scaledHeight)) //Exit
                    {
                        gameState = 0;
                    }
            }
          
        }
    }
}

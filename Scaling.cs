using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System.Numerics;
using Windows.Foundation;
using Windows.UI.ViewManagement;

namespace UWPtest1.Class
{
    class Scaling
    {
        public static void SetScale()
        {
            //Display Info
            Rect bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            MainPage.scaledWidth = (float)bounds.Width / MainPage.designWidth;
            MainPage.scaledHeight = (float)bounds.Height / MainPage.designHeight;
        }

        public static Transform2DEffect Img(CanvasBitmap src)
        {
            Transform2DEffect image;
            image = new Transform2DEffect() { Source = src };
            image.TransformMatrix = Matrix3x2.CreateScale(MainPage.scaledWidth, MainPage.scaledHeight);//Scale src image to X/Y dimensions 
            return image;
        }
    }
}

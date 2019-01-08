using System;

namespace HandmadeWindow
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            var myGame = new MyGame();
            myGame.Init("My Game", 640, 360);
            myGame.Show();
        }
    }
}

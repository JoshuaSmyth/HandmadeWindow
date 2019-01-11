using System;

namespace HandmadeWindow
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            using(var myGame = new MyGame())
            {
                myGame.Init("My Game", 1280, 720);
                myGame.Show();
            }
        }
    }
}

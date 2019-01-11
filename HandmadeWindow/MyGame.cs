using HandmadeWindow.SEngine;
using System;

namespace HandmadeWindow
{
    class MyGame : GameWindow, IDisposable
    {
        public override void Update()
        {

        }

        public override void Render(OffscreenBuffer buffer)
        {
            var p = 0;
            for(int y = 0; y < buffer.Height; y++)
            {
                for(int x = 0; x < buffer.Width; x++)
                {
                    buffer.Memory[p] = 255;
                    p++;

                    buffer.Memory[p] = 255;
                    p++;

                    buffer.Memory[p] = 0;
                    p++;

                    buffer.Memory[p] = 0;
                    p++;
                }
            }
        }

        public void Dispose()
        {
            // TODO
        }
    }
}

using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;

namespace Chip8
{


    class Program
    {
        static void Main(string[] args)
        {
            using (var gameWindow = new GameWindow(620, 320))
            {
                int frame = 0;

                var chip = new Chip8();
                chip.Initialize("breakout.ch8");

                gameWindow.Load += (sender, e) =>
                {
                    gameWindow.VSync = VSyncMode.On;
                };

                gameWindow.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, 640, 320);
                };

                gameWindow.UpdateFrame += (sender, e) =>
                {
                    chip.SetKeys(gameWindow.Keyboard);

                    gameWindow.Title = (frame % 60).ToString();
                    frame++;

                    chip.EmulateCycle();
                };

                gameWindow.RenderFrame += (sender, e) =>
                {

                    //GL.ClearColor(color.Mi)
                    GL.ClearColor(255, 0, 0, 0);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Ortho(0, 640, 320, 0, -1, 1);

                    chip.DrawGraphics();
                    

                    gameWindow.SwapBuffers();
                };

                gameWindow.Run(120.0);
            }
        }
    }
}

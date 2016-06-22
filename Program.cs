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
                Color[][] palettes = new Color[3][];
                Color[] palette = new Color[1];
                var chip = new Chip8();
                chip.Initialize("breakout.ch8");

                gameWindow.Load += (sender, e) =>
                {
                    
                    palettes[0] = new Color[] { Color.Black, Color.White };
                    palettes[1] = new Color[] { Color.Red, Color.Blue };
                    palettes[2] = new Color[] { Color.Yellow, Color.Green };

                    // Get a random palette
                    Random rnd = new Random();
                    palette = palettes[rnd.Next(palettes.Length)];

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
                    GL.ClearColor(palette[0]);
                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Ortho(0, 640, 320, 0, -1, 1);

                    chip.DrawGraphics(palette[1]);
                    

                    gameWindow.SwapBuffers();
                };

                gameWindow.Run(120.0);
            }
        }
    }
}

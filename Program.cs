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
            using (var gameWindow = new GameWindow())
            {
                int frame = 0;

                var chip = new Chip8();
                chip.Initialize("puzzle.ch8");

                gameWindow.Load += (sender, e) =>
                {
                    gameWindow.VSync = VSyncMode.On;
                };

                gameWindow.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, 640, 320);
                };



                gameWindow.RenderFrame += (sender, e) =>
                {
                    var state = OpenTK.Input.Keyboard.GetState();
                    chip.SetKeys(state);

                    gameWindow.Title = (frame % 60).ToString();
                    frame++;

                    chip.EmulateCycle();

                    GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                    GL.MatrixMode(MatrixMode.Projection);
                    GL.LoadIdentity();
                    GL.Ortho(0, 640, 320, 0, -1, 1);

                    for (int i = 0; i < 64; i++)
                    {
                        for (int j = 0; j < 32; j++)
                        {
                            if (chip.gfx[i, j])
                            {
                                GL.Begin(BeginMode.Quads);
                                GL.Vertex2(i * 10, j * 10);
                                GL.Vertex2(i * 10, j * 10 + 10);
                                GL.Vertex2((i + 1) * 10, j * 10 + 10);
                                GL.Vertex2((i + 1) * 10, j * 10);
                                GL.End();
                            }
                        }
                    }

                    gameWindow.SwapBuffers();
                };

                gameWindow.Run(120.0);
            }
            Console.ReadLine();
        }
    }
}

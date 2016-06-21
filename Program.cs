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
                chip.Initialize("breakout.ch8");

                gameWindow.Load += (sender, e) =>
                {
                    gameWindow.VSync = VSyncMode.On;
                };

                gameWindow.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, 640, 320);
                };

                gameWindow.KeyDown += (sender, e) =>
                {
                    var pressedKey = e.Key.ToString();

                    switch (pressedKey)
                    {
                        case "1":
                            chip.key[0x1] = 1;
                            chip.lastInput = 0x1;
                            break;
                        case "2":
                            chip.key[0x2] = 1;
                            chip.lastInput = 0x2;
                            break;
                        case "3":
                            chip.key[0x3] = 1;
                            chip.lastInput = 0x3;
                            break;
                        case "4":
                            chip.key[0xC] = 1;
                            chip.lastInput = 0xC;
                            break;
                        case "q":
                        case "Q":
                            chip.key[0x4] = 1;
                            chip.lastInput = 0x4;
                            break;
                        case "w":
                        case "W":
                            chip.key[0x5] = 1;
                            chip.lastInput = 0x5;
                            break;
                        case "e":
                        case "E":
                            chip.key[0x6] = 1;
                            chip.lastInput = 0x6;
                            break;
                        case "r":
                        case "R":
                            chip.key[0xD] = 1;
                            chip.lastInput = 0xD;
                            break;
                        case "a":
                        case "A":
                            chip.key[0x7] = 1;
                            chip.lastInput = 0x7;
                            break;
                        case "s":
                        case "S":
                            chip.key[0x8] = 1;
                            chip.lastInput = 0x8;
                            break;
                        case "d":
                        case "D":
                            chip.key[0x9] = 1;
                            chip.lastInput = 0x9;
                            break;
                        case "f":
                        case "F":
                            chip.key[0xE] = 1;
                            chip.lastInput = 0xE;
                            break;
                        case "z":
                        case "Z":
                            chip.key[0xA] = 1;
                            chip.lastInput = 0xA;
                            break;
                        case "x":
                        case "X":
                            chip.key[0x0] = 1;
                            chip.lastInput = 0x0;
                            break;
                        case "c":
                        case "C":
                            chip.key[0xB] = 1;
                            chip.lastInput = 0xB;
                            break;
                        case "v":
                        case "V":
                            chip.key[0xF] = 1;
                            chip.lastInput = 0xF;
                            break;
                    }

                    chip.isInputExecuted = true;
                };

                gameWindow.KeyUp += (sender, e) =>
                {
                    var pressedKey = e.Key.ToString();

                    switch (pressedKey)
                    {
                        case "1":
                            chip.key[0x1] = 0;
                            break;
                        case "2":
                            chip.key[0x2] = 0;
                            break;
                        case "3":
                            chip.key[0x3] = 0;
                            break;
                        case "4":
                            chip.key[0xC] = 0;
                            break;
                        case "q":
                        case "Q":
                            chip.key[0x4] = 0;
                            break;
                        case "w":
                        case "W":
                            chip.key[0x5] = 0;
                            break;
                        case "e":
                        case "E":
                            chip.key[0x6] = 0;
                            break;
                        case "r":
                        case "R":
                            chip.key[0xD] = 0;
                            break;
                        case "a":
                        case "A":
                            chip.key[0x7] = 0;
                            break;
                        case "s":
                        case "S":
                            chip.key[0x8] = 0;
                            break;
                        case "d":
                        case "D":
                            chip.key[0x9] = 0;
                            break;
                        case "f":
                        case "F":
                            chip.key[0xE] = 0;
                            break;
                        case "z":
                        case "Z":
                            chip.key[0xA] = 0;
                            break;
                        case "x":
                        case "X":
                            chip.key[0x0] = 0;
                            break;
                        case "c":
                        case "C":
                            chip.key[0xB] = 0;
                            break;
                        case "v":
                        case "V":
                            chip.key[0xF] = 0;
                            break;
                    }

                    chip.isInputExecuted = true;
                };

                gameWindow.RenderFrame += (sender, e) =>
                {
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

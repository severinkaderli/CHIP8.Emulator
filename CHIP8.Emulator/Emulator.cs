using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using System.IO;
using Mono.Options;
using System.Collections.Generic;

namespace CHIP8.Emulator
{
    class Emulator
    {
        /// <summary>
        /// The current version of the emulator.
        /// </summary>
        const String VERSION = "1.0.0";

        const bool DEBUG = true;

        /// <summary>
        /// 
        /// </summary>
        public static Dictionary<Key, ushort> keyBindings = new Dictionary<Key, ushort>
               {
                { Key.Number0, 0x0 },
                { Key.Number1, 0x1 },
                { Key.Number2, 0x2 },
                { Key.Number3, 0x3 },
                { Key.Q, 0x4 },
                { Key.W, 0x5 },
                { Key.E, 0x6 },
                { Key.A, 0x7 },
                { Key.S, 0x8 },
                { Key.D, 0x9 },
                { Key.Z, 0xA },
                { Key.C, 0xB },
                { Key.Number4, 0xC },
                { Key.R, 0xD },
                { Key.F, 0xE },
                { Key.V, 0xF }
               };

        static void Main(string[] args)
        {
            // Variables that are gonna be set by the command line options.
            bool shouldShowVersion = false;
            bool shouldShowHelp = false;
            String romFileName = null;

            // Command line options
            OptionSet options = new OptionSet
            {
                {"v|version", "Show the version", v => shouldShowVersion = true },
                {"h|help", "Show the help", h => shouldShowHelp = true }
            };

            // Parsing the options
            List<String> extra;
            try
            {
                extra = options.Parse(args);
            } catch(OptionException e)
            {
                Console.Write("Emulator: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try chip8-emulator.exe --help");
                return;
            }

            if(shouldShowHelp)
            {
                showHelp(options);
                return;
            }
            
            if(shouldShowVersion)
            {
                Console.WriteLine("Current version: {0}", Emulator.VERSION);
                return;
            }

            if (extra.Count > 0)
            {
                romFileName = extra.ToArray()[0];
            } else
            {
                // No file given
                Console.WriteLine("No file given!");
                //return;
                romFileName = "C:\\Users\\Severin Kaderli\\Downloads\\Chip-8 Pack\\Chip-8 Games\\Breakout [Carmelo Cortez, 1979].ch8";
            }           

            using (GameWindow gameWindow = new GameWindow(620, 320))
            {
               
        int frame = 0;
                var chip = new Chip8();
                chip.Initialize(romFileName);
                

                gameWindow.Load += (sender, e) =>
                {
                    gameWindow.VSync = VSyncMode.Off;
                    gameWindow.WindowBorder = WindowBorder.Fixed;
                };

                gameWindow.Resize += (sender, e) =>
                {
                    GL.Viewport(0, 0, 640, 320);
                };

                gameWindow.Keyboard.KeyDown += (sender, e) =>
                {
                    if(Emulator.keyBindings.ContainsKey(e.Key)) {
                        chip.SetKey(Emulator.keyBindings[e.Key], true);
                    }
                };

                // Handles all key up events
                gameWindow.Keyboard.KeyUp += (sender, e) =>
                {
                    if (Emulator.keyBindings.ContainsKey(e.Key)) {
                        chip.SetKey(Emulator.keyBindings[e.Key], false);
                    }
                };

                gameWindow.UpdateFrame += (sender, e) =>
                {
                    gameWindow.Title = String.Format("FPS: {0}", gameWindow.RenderFrequency);
                    frame++;

                    chip.EmulateCycle();
                };

                gameWindow.RenderFrame += (sender, e) =>
                {
                    // Only update screen if it's neccesary
                    if(chip.RenderFlag)
                    {
                        if(Emulator.DEBUG)
                            Console.WriteLine("{0:HH:mm:ss}: Updates screen", DateTime.Now);

                        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                        GL.MatrixMode(MatrixMode.Projection);
                        GL.LoadIdentity();
                        GL.Ortho(0, 640, 320, 0, -1, 1);

                        chip.DrawGraphics();


                        gameWindow.SwapBuffers();
                        chip.RenderFlag = false;
                    }
                    
                };

                gameWindow.Run(60);
            }
        }

        // Print out the help information
        static void showHelp(OptionSet options)
        {
            // show some app description message
            Console.WriteLine("Usage: chip8-emulator.exe [OPTIONS] FILE");
            Console.WriteLine("File is the rom that you want to play.");
            Console.WriteLine();

            // output the options
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }
    }
}

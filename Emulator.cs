using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using System.IO;
using Mono.Options;
using System.Collections.Generic;

namespace Chip8
{
    class Emulator
    {
        /// <summary>
        /// The current version of the emulator.
        /// </summary>
        const String VERSION = "1.0.0";
        

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
                Console.WriteLine("Try emulator --help");
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
                return;
            }

            if (!File.Exists(romFileName))
            {
                Console.WriteLine("File: {0} can't be found!", romFileName);
                return;
            }

            

            using (var gameWindow = new GameWindow(620, 320))
            {
                
                int frame = 0;
                Color[][] palettes = new Color[3][];
                Color[] palette = new Color[1];
                var chip = new Chip8();
                chip.Initialize(romFileName);
                

                gameWindow.Load += (sender, e) =>
                {

                    palettes[0] = new Color[] { Color.Black, Color.White };
                    palettes[1] = new Color[] { Color.Red, Color.Blue };
                    palettes[2] = new Color[] { Color.Yellow, Color.Green };

                    // Get a random palette
                    Random rnd = new Random();
                    palette = palettes[rnd.Next(palettes.Length)];

                    gameWindow.VSync = VSyncMode.On;
                    gameWindow.WindowBorder = WindowBorder.Fixed;

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

        static void showHelp(OptionSet options)
        {
            // show some app description message
            Console.WriteLine("Usage: OptionsSample.exe [OPTIONS]+ message");
            Console.WriteLine("Greet a list of individuals with an optional message.");
            Console.WriteLine("If no message is specified, a generic greeting is used.");
            Console.WriteLine();

            // output the options
            Console.WriteLine("Options:");
            options.WriteOptionDescriptions(Console.Out);
        }
    }
}

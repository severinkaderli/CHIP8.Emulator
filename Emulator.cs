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
                return;
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

                gameWindow.UpdateFrame += (sender, e) =>
                {
                    chip.SetKeys(gameWindow.Keyboard);

                    gameWindow.Title = String.Format("FPS: {0}", gameWindow.RenderFrequency);
                    frame++;

                    chip.EmulateCycle();
                };

                gameWindow.RenderFrame += (sender, e) =>
                {
                    // Only update screen if it's neccesary
                    if(chip.renderFlag)
                    {
                        Console.WriteLine("{0:HH:mm:ss}: Updates screen", DateTime.Now);
                        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                        GL.MatrixMode(MatrixMode.Projection);
                        GL.LoadIdentity();
                        GL.Ortho(0, 640, 320, 0, -1, 1);

                        chip.DrawGraphics();


                        gameWindow.SwapBuffers();
                        chip.renderFlag = false;
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

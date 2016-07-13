using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace CHIP8.Emulator
{
    class Chip8
    {
        /// <summary>
        /// Start of the working RAM.
        /// </summary>
        private const int START_PROGRAM_MEMORY = 512;

        /// <summary>
        /// True if an input was executed.
        /// </summary>
        public bool IsInputExecuted { get; set; } = false;

        /// <summary>
        /// Holds the last input.
        /// </summary>
        public ushort LastInput { get; set; }

        /// <summary>
        /// If true the screen should be redrawn.
        /// </summary>
        public bool RenderFlag { get; set; } = false;

        /// <summary>
        /// The memory of the CHIP-8.
        /// </summary>
        private byte[] Memory { get; set; } = new byte[4096];

        /// <summary>
        /// The current opcode.
        /// </summary>
        private ushort Instruction { get; set; }

        /// <summary>
        /// CPU registers from V0 to VE. VF is the carry flag.
        /// </summary>
        private byte[] V { get; set; } = new byte[16];

        /// <summary>
        /// The index register
        /// </summary>
        private ushort I { get; set; } = 0;

        /// <summary>
        /// Program counter
        /// </summary>
        private ushort PC { get; set; } = 0;

        /// <summary>
        /// Array that contains the current screen state.
        /// </summary>
        private bool[,] GFX { get; set; } = new bool[64, 32];

        /// <summary>
        /// If greater than zero, this timer counts down.
        /// </summary>
        private byte DelayTimer { get; set; } = 0x00;

        /// <summary>
        /// If the sound timer reaches zero, a sound is emitted.
        /// </summary>
        private byte SoundTimer { get; set; } = 0x00;

        /// <summary>
        /// The stack
        /// </summary>
        private Stack<ushort> Stack { get; set; } = new Stack<ushort>(16);

        /// <summary>
        /// Pointer that point to the current stack position.
        /// </summary>
        private int StackPointer { get; set; }

        /// <summary>
        /// The current state of the keypad.
        /// </summary>
        public bool[] key = new bool[16];

        /// <summary>
        /// The fontset of the CHIP8.
        /// </summary>
        byte[] fontset = new byte[80]
        {
            0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
            0x20, 0x60, 0x20, 0x20, 0x70, // 1
            0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
            0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
            0x90, 0x90, 0xF0, 0x10, 0x10, // 4
            0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
            0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
            0xF0, 0x10, 0x20, 0x40, 0x40, // 7
            0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
            0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
            0xF0, 0x90, 0xF0, 0x90, 0x90, // A
            0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
            0xF0, 0x80, 0x80, 0x80, 0xF0, // C
            0xE0, 0x90, 0x90, 0x90, 0xE0, // D
            0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
            0xF0, 0x80, 0xF0, 0x80, 0x80  // F
        };

        /// <summary>
        /// Initializes and resets the system.
        /// </summary>
        /// <param name="fileName"></param>
        public void Initialize(String fileName)
        {
            I = 0;
            StackPointer = 0;

            PC = START_PROGRAM_MEMORY;

            LoadGame(fileName);

            // Load fontset into memory
            for(int i = 0; i < fontset.Length; ++i)
            {
                Memory[i] = fontset[i];
            }
        }

        /// <summary>
        /// Loads a game into memory
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadGame(String fileName)
        {

            // If the file doesn't exist, exit the application.
            if(!File.Exists(fileName))
            {
                Console.WriteLine("File {0} doesn't exist!", fileName);
                Environment.Exit(1);
            }

            // Open the file in binary mode
            using (BinaryReader binaryReader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                // Loading the game into memory
                for (int i = 0; i < binaryReader.BaseStream.Length; i++)
                {
                    Memory[START_PROGRAM_MEMORY + i] = binaryReader.ReadByte();
                }
            }; 
        }

        public void EmulateCycle()
        {
            // Fetch the current instruction
            Instruction = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);

            byte X = (byte)((Instruction & 0x0F00) >> 8);
            byte Y = (byte)((Instruction & 0x00F0) >> 4);

            byte N = (byte)(Instruction & 0x000F);
            byte NN = (byte)(Instruction & 0x00FF);
            ushort NNN = (byte)(Instruction & 0x0FFF);

            // Execute the opcode
            switch (Instruction & 0xF000)
            {
                case 0x0000:
                    if (X == 0x0)
                    {
                        if (N == 0x0)
                        {
                            //Clear the display.
                            RenderFlag = true;
                            GFX = new bool[64, 32];
                        }
                        if (N == 0xE)
                        {
                            //Return from subroutine.
                            PC = Stack.Pop();
                        }
                    }
                    else
                    {
                        Console.WriteLine("RCA 1802 CODE -- NOT IPLEMENTED");
                    }

                    break;

                case 0x1000: // 1NNN: Jump to the address NNN
                    PC = (ushort)((Instruction & 0x0FFF) - 2);

                    ushort other = (ushort)((Instruction & 0x0FFF) - 2);
                    ushort stuff = (ushort)(NNN - 2);

                    Console.WriteLine("NNN: {0}, Other: {0}", stuff, other);
                    break;

                case 0x2000: // 2NNN: Jump to the subroutine in address NNN
                    Stack.Push(PC);
                    PC = (ushort)((Instruction & 0x0FFF) - 2);
                    break;

                case 0x3000: // 3XNN: Skip the next instruction if value of register X equals NN
                    if(V[X] == NN) PC += 2;
                    break;

                case 0x4000: // 4XNN: Skip the next instruction if value of register X not equals NNN
                    if(V[X] != Memory[PC + 1]) PC += 2;
                    break;

                case 0x5000: // 5XYN: Skip the next instruction if value of register X and Y are equal
                    if(V[X] == V[Y])
                    {
                        PC += 2;
                    }
                    break;

                case 0x6000: // 6XNN: Sets value of register X to NN
                    V[X] = (byte)NN;
                    break;

                case 0x7000: // 7XNN: Increments value of register X by NN
                    V[X] += (byte)NN;
                    break;

                case 0x8000:
                    switch (N)
                    {
                        case 0x0: // 8XY0
                            V[X] = V[Y];
                            break;

                        case 0x1: // 8XY1
                            V[X] |= V[Y];
                            break;

                        case 0x2: // 8XY2
                            V[X] &= V[Y];
                            break;

                        case 0x3: // 8XY3
                            V[X] ^= V[Y];
                            break;

                        case 0x4: // 8XY4
                            if ((V[X] + V[Y]) > 255)
                            {
                                V[0xF] = 1;
                            } else
                            {
                                V[0xF] = 0;
                            }

                            V[X] = (byte)(V[X] + V[Y]);

                            break;

                        case 0x5: // 8XY5
                            if(V[X] > V[Y])
                            {
                                V[0xF] = 1;
                            } else {
                                V[0xF] = 0;
                            }

                            V[X] = (byte)(V[X] - V[Y]);

                            break;

                        case 0x6: // 8XY6
                            V[0xF] = (byte)(V[X] & 0x1);
                            V[X] >>= 1;

                            break;

                        case 0x7: // 8XY7
                            if(V[Y] > V[X])
                            {
                                V[0xF] = 1;
                            } else
                            {
                                V[0xF] = 0;
                            }

                            V[X] = (byte)(V[Y] - V[X]);

                            break;

                        case 0xE: // 8XYE
                            V[0xF] = (byte)(V[X] >> 7);
                            V[X] <<= 1;

                            break;
                    }
                    break;

                case 0x9000: // 9NNN: Skip next instruction if register X and Y are not equal.
                    if(V[X] != V[Y]) {
                        PC += 2;
                    }
                    break;

                case 0xA000: // ANNN: Sets I to the address NNN
                    I = (ushort)(Instruction & 0x0FFF);
                    break;

                case 0xB000:
                    PC = (ushort)((Instruction & 0x0FFF) + V[0] - 2);
                    break;

                case 0xC000:
                    Random rnd = new Random();
                    V[X] = (byte)(rnd.Next(255) & NN);
                    break;

                case 0xD000: // Print sprite to screen
                    V[0xF] = 0;
                    RenderFlag = true;
                    for(int height = 0; height < N; height++)
                    {
                        byte spritePart = Memory[I + height];

                        for(int width = 0; width < 8; width++)
                        {
                            if((spritePart & (0x80 >> width)) != 0)
                            {
                                ushort _x = (ushort)((V[X] + width) % 64);
                                ushort _y = (ushort)((V[Y] + height) % 32);

                                if(GFX[_x, _y] == true)
                                {
                                    V[0xF] = 1;
                                }

                                GFX[_x, _y] ^= true;
                            }
                        }
                    }
                    break;

                case 0xE000:

                    if(N == 0xE)
                    {
                        if(key[V[X]])
                        {
                            PC += 2;
                        }
                    }
                    else
                    {
                        if(!key[V[X]])
                        {
                            PC += 2;
                        }
                    }

                    break;

                case 0xF000:
                    switch(N)
                    {
                        case 0x7: // Set Vx to delay timer
                            V[X] = DelayTimer;
                            break;

                        case 0xA: // Do not increase program counter until a key is pressed. Then store pressed key.
                            if (IsInputExecuted)
                            {
                                V[X] = (byte)LastInput;
                            }
                            else
                            {
                                PC -= 2;
                            }

                            break;

                        case 0x5:
                            switch (Y)
                            {
                                case 0x1: // Delay Timer = regiserX
                                    DelayTimer = (byte) V[X];
                                    break;

                                case 0x5: // Store registers V0 through Vx in memory starting at location I.

                                    for (int i = 0; i <= X; i++)
                                    {
                                        Memory[I + i] = V[i];
                                    }
                                    break;

                                case 0x6: // Read registers V0 through Vx from memory starting at location I.
                                    for (int i = 0; i <= X; i++)
                                    {
                                        V[i] = Memory[I + i];
                                    }
                                    break;
                            }
                            break;

                        case 0x8: //Sound Timer = registerX
                            SoundTimer = V[X];
                            break;

                        case 0xE: //I = I + regiserX
                            I += V[X];
                            break;

                        case 0x9: //Set I to the location of char in registerX
                            I = (ushort)(V[X] * 5);
                            break;

                        case 0x3: // Store BCD representation of Vx in memory locations I, I+1, and I+2.
                            decimal number = V[X];
                            Memory[I] = (byte)(number / 100);
                            Memory[I + 1] = (byte)((number % 100) / 10);
                            Memory[I + 2] = (byte)((number % 100) % 10);
                            break;
                    }

                    break;

                default:
                    Console.WriteLine("Unknown opcode: {0:X}", Instruction);
                    break;
            }

            IsInputExecuted = false;
            PC += 2;

            if (DelayTimer > 0)
            {
                DelayTimer--;
            }

            if(SoundTimer > 0)
            {
                SoundTimer--;
                if(SoundTimer == 0)
                {
                    Console.Beep();
                }
            }
        }

        /// <summary>
        /// Draw the graphics if needed
        /// </summary>
        public void DrawGraphics()
        {
            for (int i = 0; i < 64; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (GFX[i, j])
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
        }

        /// <summary>
        /// Set the current state of a key.
        /// </summary>
        /// <param name="index">The index of the key</param>
        /// <param name="state">The state to which the key will be set</param>
        public void SetKey(ushort index, bool state)
        {
            key[index] = state;

            if(state)
            {
                IsInputExecuted = true;
                LastInput = index;
            } else
            {
                IsInputExecuted = false;
            }
        }
    }
}

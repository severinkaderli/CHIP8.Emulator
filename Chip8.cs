using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8
{
    class Chip8
    {
        /// <summary>
        /// Start of the working RAM.
        /// </summary>
        private const int START_PROGRAM_MEMORY = 512;

        public bool isInputExecuted = false;

        public byte lastInput;

        /// <summary>
        /// The memory of the CHIP-8.
        /// </summary>
        protected byte[] memory = new byte[4096];

        /// <summary>
        /// The current opcode.
        /// </summary>
        protected ushort opcode;

        /// <summary>
        /// CPU registers from V0 to VE. VF is the carry flag.
        /// </summary>
        protected byte[] V = new byte[16];

        /// <summary>
        /// The index register
        /// </summary>
        protected ushort I = 0;

        /// <summary>
        /// Program counter
        /// </summary>
        protected ushort pc;

        /// <summary>
        /// Array that contains the current screen state.
        /// </summary>
        public bool[,] gfx = new bool[64, 32];

        /// <summary>
        /// If greater than zero, this timer counts down.
        /// </summary>
        protected byte delayTimer;

        /// <summary>
        /// If the sound timer reaches zero, a sound is emitted.
        /// </summary>
        protected byte soundTimer;

        /// <summary>
        /// The stack
        /// </summary>
        protected Stack<ushort> stack = new Stack<ushort>(16);

        /// <summary>
        /// Pointer that point to the current stack position.
        /// </summary>
        protected int stackPointer;

        /// <summary>
        /// The current state of the keypad.
        /// </summary>
        public byte[] key = new byte[16];

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
        /// Runs the emulator.
        /// </summary>
        /// <param name="path">Path to the game</param>
        ////public void Run(String path)
        ////{
        ////    // Initialize the system
        ////    //Initialize();

        ////    // Load the game
        ////    LoadGame(path);
     
        ////    // Main execution loop
        ////    for (;;)
        ////    {
        ////        EmulateCycle();

        ////        if(drawFlag)
        ////        {
        ////            DrawGraphics();
        ////        }

        ////        SetKeys();
        ////    }
        ////}

        /// <summary>
        /// Initializes the system.
        /// </summary>
        public void Initialize(String name)
        {
            I = 0;
            stackPointer = 0;

            pc = START_PROGRAM_MEMORY;

            LoadGame(name);

            // Load fontset into memory
            for(int i = 0; i < fontset.Length; ++i)
            {
                memory[i] = fontset[i];
            }
        }

        /// <summary>
        /// Loads a game into memory
        /// </summary>
        /// <param name="game"></param>
        private void LoadGame(String game)
        {
            // Open the file in binary mode
            BinaryReader binaryReader = new BinaryReader(File.Open(game, FileMode.Open));

            // Loading the game into memory
            for(int i = 0; i < binaryReader.BaseStream.Length; i++)
            {
                memory[START_PROGRAM_MEMORY + i] = binaryReader.ReadByte();
            }
        }

        public void EmulateCycle()
        {
            // Fetch the current opcode
            opcode = (ushort)((ushort)(memory[pc] << 8) | memory[pc + 1]);
            Console.WriteLine("Executing opcode: {0:X}", opcode);

            ushort X = (byte)(memory[pc] & 0x0F);
            ushort Y = (byte)(memory[pc + 1] >> 4);
            ushort N = (byte)(memory[pc + 1] & 0x0F);
            ushort NN = (byte)(memory[pc + 1]);
            ushort NNN = (byte)(opcode & 0x0FFF);

            // Execute the opcode
            switch (opcode & 0xF000)
            {
                case 0x0000:
                    if (X == 0x0)
                    {
                        if (N == 0x0)
                        {
                            //Clear the display.
                            gfx = new bool[64, 32];
                        }
                        if (N == 0xE)
                        {
                            //Return from subroutine.
                            pc = stack.Pop();
                        }
                    }
                    else
                    {
                        Console.WriteLine("RCA 1802 CODE -- NOT IPLEMENTED");
                    }

                    break;

                case 0x1000: // 1NNN: Jump to the address NNN
                    pc = (ushort)((opcode & 0x0FFF) - 2);
                    break;

                case 0x2000: // 2NNN: Jump to the subroutine in address NNN
                    stack.Push(pc);
                    pc = (ushort)((opcode & 0x0FFF) - 2);
                    Console.WriteLine("NNN: {0:X}", NNN);
                    break;

                case 0x3000: // 3XNN: Skip the next instruction if value of register X equals NN
                    if(V[X] == NN)
                    {
                        pc += 2;
                    }
                    break;

                case 0x4000: // 4XNN: Skip the next instruction if value of register X not equals NNN
                    if(V[X] != memory[pc + 1])
                    {
                        pc += 2;
                    }
                    break;

                case 0x5000: // 5XYN: Skip the next instruction if value of register X and Y are equal
                    if(V[X] == V[Y])
                    {
                        pc += 2;
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
                        pc += 2;
                    }
                    break;

                case 0xA000: // ANNN: Sets I to the address NNN
                    I = NNN;
                    break;

                case 0xB000:
                    pc = (ushort)((opcode & 0x0FFF) + V[0] - 2);
                    break;

                case 0xC000:
                    Random rnd = new Random();
                    V[X] = (byte)(rnd.Next(255) & NN);
                    break;

                case 0xD000: // Print sprite to screen
                    V[0xF] = 0;

                    for(int height = 0; height < N; height++)
                    {
                        byte spritePart = memory[I + height];

                        for(int width = 0; width < 8; width++)
                        {
                            if((spritePart & (0x80 >> width)) != 0)
                            {
                                ushort _x = (ushort)((V[X] + width) % 64);
                                ushort _y = (ushort)((V[X] + width) % 32);

                                if(gfx[_x, _y] == true)
                                {
                                    V[0xF] = 1;
                                }

                                gfx[_x, _y] ^= true;
                            }
                        }
                    }
                    break;

                case 0xE000:

                    if(N == 0xE)
                    {
                        if(key[V[X]] == 1)
                        {
                            pc += 2;
                        }
                    }
                    else
                    {
                        if(key[V[X]] == 0)
                        {
                            pc += 2;
                        }
                    }

                    break;

                case 0xF000:
                    switch(N)
                    {
                        case 0x7: // Set Vx to delay timer
                            V[X] = delayTimer;
                            break;

                        case 0xA: // Do not increase program counter until a key is pressed. Then store pressed key.
                            if (isInputExecuted)
                            {
                                V[X] = lastInput;
                            }
                            else
                            {
                                pc -= 2;
                            }

                            break;

                        case 0x5:
                            switch (Y)
                            {
                                case 0x1: // Delay Timer = regiserX
                                    delayTimer = (byte) V[X];
                                    break;

                                case 0x5: // Store registers V0 through Vx in memory starting at location I.

                                    for (int i = 0; i <= X; i++)
                                    {
                                        memory[I + i] = V[i];
                                    }
                                    break;

                                case 0x6: // Read registers V0 through Vx from memory starting at location I.
                                    for (int i = 0; i <= X; i++)
                                    {
                                        V[i] = memory[I + i];
                                    }
                                    break;
                            }
                            break;

                        case 0x8: //Sound Timer = registerX
                            soundTimer = V[X];
                            break;

                        case 0xE: //I = I + regiserX
                            I += V[X];
                            break;

                        case 0x9: //Set I to the location of char in registerX
                            I = (ushort)(V[X] * 5);
                            break;

                        case 0x3: // Store BCD representation of Vx in memory locations I, I+1, and I+2.
                            decimal number = V[X];
                            memory[I] = (byte)(number / 100);
                            memory[I + 1] = (byte)((number % 100) / 10);
                            memory[I + 2] = (byte)((number % 100) % 10);
                            break;
                    }

                    break;

                default:
                    Console.WriteLine("Unknown opcode: {0:X}", opcode);
                    break;
            }

            isInputExecuted = false;
            pc += 2;

            if (delayTimer > 0)
            {
                delayTimer--;
            }

            if(soundTimer > 0)
            {
                soundTimer--;
                if(soundTimer == 0)
                {
                    Console.Beep();
                }
            }
        }

        private void DrawGraphics()
        {

        }

        private void SetKeys()
        {

        }
    }
}

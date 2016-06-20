using System;
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
        private const int PROGRAMM_COUNTER_START = 0x200;

        /// <summary>
        /// If the draw flag is set, we draw something on the screen.
        /// </summary>
        protected bool drawFlag;

        /// <summary>
        /// The memory of the CHIP-8.
        /// </summary>
        protected byte[] memory = new Byte[4096];

        /// <summary>
        /// The current opcode.
        /// </summary>
        protected int opcode;

        /// <summary>
        /// CPU registers from V0 to VE. VF is the carry flag.
        /// </summary>
        protected int[] v = new int[16];

        /// <summary>
        /// The index register
        /// </summary>
        protected int i;

        /// <summary>
        /// Program counter
        /// </summary>
        protected int pc;

        /// <summary>
        /// Array that contains the current screen state.
        /// </summary>
        protected bool[] gfx = new bool[64 * 32];

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
        protected ushort[] stack = new ushort[16];

        /// <summary>
        /// Pointer that point to the current stack position.
        /// </summary>
        protected int stackPointer;

        /// <summary>
        /// The current state of the keypad.
        /// </summary>
        protected char[] keyState = new char[16];

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
        public void Run(String path)
        {
            // Initialize the system
            Initialize();

            // Load the game
            LoadGame(path);
     
            // Main execution loop
            for (;;)
            {
                EmulateCycle();

                if(drawFlag)
                {
                    DrawGraphics();
                }

                SetKeys();
            }
        }

        /// <summary>
        /// Initializes the system.
        /// </summary>
        private void Initialize()
        {
            i = 0;
            stackPointer = 0;

            pc = PROGRAMM_COUNTER_START;

            // Load fontset into memory
            for(int i = 0; i< 80; ++i)
            {
                memory[i] = fontset[i];
            }
        }

        // Loads the game into memory
        private void LoadGame(String game)
        {

            byte[] Buffer = System.IO.File.ReadAllBytes(game);

            for(int i = 0; i < Buffer.Length; ++i)
            {
                memory[i + 512] = Buffer[i];
            }
        }

        private void EmulateCycle()
        {
            // Fetch the current opcode
            opcode = memory[pc] << 8 | memory[pc + 1];

            // Execute the opcode
            switch(opcode & 0xF000)
            {
                case 0xA000: // ANNN: Sets I to the address NNN

                    i = opcode & 0x0FFF;
                    pc += 2;

                    break;

                default:
                    Console.WriteLine("Unknown opcode: " + opcode);

                    break;
            }

            if(delayTimer > 0)
            {
                delayTimer--;
            }

            if(soundTimer > 0)
            {
                soundTimer--;
                if(soundTimer == 0)
                {
                    // BEEP
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

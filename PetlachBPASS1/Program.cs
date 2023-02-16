using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PetlachBPASS1
{
    class MainClass
    {
        static Random rng = new Random();

        static StreamReader inFile;

        static List<string> answerWords = new List<string>();
        static List<string> extraWords = new List<string>();

        const int NUM_ROWS = 6;
        const int NUM_COLS = 5;

        public static void Main(string[] args)
        {
            //Display Menu
            //DisplayMenu();

            //SHORTCUT
            PlayGame();
        }

        private static void LoadDictionaries(List<string> wordList, string fileName)
        {
            //Load answers
            try
            {
                inFile = File.OpenText(fileName);

                while (!inFile.EndOfStream)
                {
                    wordList.Add(inFile.ReadLine());
                }
                inFile.Close();
            }
            catch(FileNotFoundException fnf)
            {
                Console.WriteLine(fnf.Message);
            }
            catch(FormatException fe)
            {
                Console.WriteLine(fe.Message);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void DisplayMenu()
        {
            int input;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Welcome to WORDLE!\n");
                Console.WriteLine("1. Play");
                Console.WriteLine("2. Instructions");
                Console.WriteLine("3. Stats");
                Console.WriteLine("4. Settings\n");
                Console.Write("Enter selection: ");

                try
                {
                    input = Convert.ToInt32(Console.ReadLine());
                    switch (input)
                    {
                        case 1:
                            //Play
                            PlayGame();
                            break;
                        case 2:
                            //Instructions
                            Console.WriteLine("Inst.");
                            break;
                        case 3:
                            //Stats
                            Console.WriteLine("Stats");
                            break;
                        case 4:
                            //Settings
                            Console.WriteLine("Settings");
                            break;
                        default:
                            Console.WriteLine("Not a valid input. Press enter to try again");
                            Console.ReadLine();
                            break;
                    }

                }
                catch
                {
                    Console.WriteLine("Not a valid input. Press enter to try again");
                    Console.ReadLine();
                }
            }
        }

        private static void PlayGame()
        {
            Console.Clear();

            const int INITIAL_CURSOR_LEFT = 2;
            const int INITIAL_CURSOR_TOP = 2;

            const int GRID_PAD_SIZE = 4;

            int cursorPosTop = INITIAL_CURSOR_LEFT;
            int cursorPosLeft = INITIAL_CURSOR_TOP;

            char[] alpha = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X' , 'Y', 'Z' };

            //Load dictionaries and save words
            LoadDictionaries(answerWords, "WordleAnswers.txt");
            LoadDictionaries(extraWords, "WordleExtras.txt");

            //Select random 5-letter word
            string answer = answerWords[rng.Next(0, answerWords.Count)];

            //Display alphabet
            for (int i = 0; i < alpha.Length; i++)
            {
                Console.Write(alpha[i]);
            }
            Console.WriteLine("\n");

            //Draw Grid
            Console.WriteLine("---------------------");
            for (int i = 0; i < NUM_ROWS; i++)
            {
                for (int j = 0; j < NUM_COLS; j++)
                {
                    Console.Write("|".PadRight(GRID_PAD_SIZE));
                }
                Console.Write("|\n");
                Console.WriteLine("---------------------");
            }

            bool doneGuessing = false;

            while (doneGuessing == false)
            {
                Console.SetCursorPosition(cursorPosLeft, cursorPosTop);
                if (Console.Read() != 32 && (cursorPosLeft + GRID_PAD_SIZE) < (INITIAL_CURSOR_LEFT + GRID_PAD_SIZE * NUM_COLS))
                {
                    //ConsoleKey prevKey = Console.ReadKey().Key;
                    //if (prevKey == ConsoleKey.Backspace && cursorPosLeft - 1 >= INITIAL_CURSOR_LEFT)
                    {
                        //    cursorPosLeft -= GRID_PAD_SIZE;
                    }
                    //else
                    //{
                        cursorPosLeft += GRID_PAD_SIZE;

                    //}
                }
                if (Console.ReadKey().Key == ConsoleKey.Backspace)
                {
                    Console.WriteLine("Success!");
                }
            }
        }
    }
}

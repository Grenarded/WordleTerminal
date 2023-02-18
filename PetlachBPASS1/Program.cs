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

        const int INITIAL_CURSOR_LEFT = 2;
        const int INITIAL_CURSOR_TOP = 3;

        const int GRID_PAD_SIZE = 4;

        static char[] alpha = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        static char[,] guessBoard = new char[NUM_ROWS, NUM_COLS];

        static int curRow = 0;
        static int curCol = 0;

        static string answer;

        public static void Main(string[] args)
        {
            //Load dictionaries and save words
            LoadDictionaries(answerWords, "WordleAnswers.txt");
            LoadDictionaries(extraWords, "WordleExtras.txt");

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

            SetUpGame();

            //Select random 5-letter word <--ADJUST
            answer = answerWords[rng.Next(0, answerWords.Count)];

            HandleInput();
        }

        private static void SetUpGame()
        {
            //Fill board array w/ empty chars
            for (int i = 0; i < NUM_ROWS; i++)
            {
                for (int j = 0; j < NUM_COLS; j++)
                {
                    guessBoard[i, j] = ' ';
                }
            }

            //Draw Grid
            DrawGame();
        }

        private static void DrawGame()
        {
            Console.Clear();

            //TEMPORARY
            Console.WriteLine(answer);
            //Console.WriteLine();
            //

            //Display alphabet
            for (int i = 0; i < alpha.Length; i++)
            {
                Console.Write(alpha[i]);
            }
            Console.WriteLine("\n");

            Console.WriteLine("---------------------");
            for (int i = 0; i < NUM_ROWS; i++)
            {
                for (int j = 0; j < NUM_COLS; j++)
                {
                    //Console.Write("|".PadRight(GRID_PAD_SIZE));
                    Console.Write("| " + guessBoard[i, j] + " ");
                }
                Console.Write("|\n");
                Console.WriteLine("---------------------");
            }
        }

        private static void HandleInput()
        {
            bool guessComplete = false;
            while (guessComplete == false)
            {
                int userInput = Console.Read();

                if ((userInput >= 65 && userInput <= 90) || (userInput >= 97 && userInput <= 122))
                {
                    if (curCol < NUM_COLS)
                    {
                        guessBoard[curRow, curCol] = char.ToUpper((char)userInput);
                        curCol++;
                        DrawGame();
                    }
                    else
                    {
                        ClearLine();
                    }
                }
                else if (userInput == 0) //Backspace ASCII
                {
                    if (curCol > 0)
                    {
                        guessBoard[curRow, curCol - 1] = ' ';
                        curCol--;
                        DrawGame();
                    }
                }
                else if (userInput == 10) //Enter ASCII
                {
                    if (guessBoard[curRow, NUM_COLS - 1] != ' ')
                    {
                        CheckWord();
                    }
                    else
                    {
                        //Stop cursor from moving downwards
                        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
                    }
                }
                else
                {
                    ClearLine();
                }
            }
        }

        private static void ClearLine() 
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(answer); //REMOVE AFTER
        }

        private static void CheckWord()
        {
            int correctLetters = 0;

            for (int i = 0; i < NUM_COLS; i++)
            {
                if (guessBoard[curRow, i] == char.ToUpper(answer[i]))
                {
                    correctLetters++;
                }
            }

            if (correctLetters == 5)
            {
                //User guessed the word correctly
                Console.WriteLine("You guessed correctly!");
            }
            else
            {
                Console.WriteLine("You guessed wrong!");
            }
        }
    }
}
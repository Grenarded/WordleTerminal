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

        //1: Correct spot. 2: Correct letter wrong spot. 3: Wrong letter
        static int[,] correctness = new int[NUM_ROWS, NUM_COLS];
        static int[] alphaStatus = new int[alpha.Length];

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

            //Select random 5-letter word <--ADJUST
            answer = answerWords[rng.Next(0, answerWords.Count)];

            SetUpGame();

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

            Console.WriteLine(answer);

            //Display alphabet
            for (int i = 0; i < alpha.Length; i++)
            {
                //TODO: COLOR THEM LETTERS!
                if (alphaStatus[i] == 1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
                else if (alphaStatus[i] == 2)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                }
                else if (alphaStatus[i] == 3)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                
                Console.Write(alpha[i]);
                Console.ResetColor();
                Console.Write(" ");
            }

            Console.WriteLine();//FIGURE OUT WHY YOU CAN"T DO TWO LINES. Some issue w/ clearing

            //Draw Grid
            Console.WriteLine("---------------------");
            for (int row = 0; row < NUM_ROWS; row++)
            {
                for (int col = 0; col < NUM_COLS; col++)
                {
                    Console.Write("|");
                    if (correctness[row, col] == 1)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                    }
                    else if (correctness[row, col] == 2)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                    }
                    else if (correctness[row, col] == 3)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    }
                    Console.Write(" " + guessBoard[row, col] + " ");
                    Console.ResetColor();
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
                        CheckGuess();
                    }
                    else
                    {
                        Console.WriteLine("Not enough letters");
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
            Console.SetCursorPosition(0, Console.CursorTop);
            //Console.Write(answer); //REMOVE AFTER
        }

        private static void CheckGuess()
        {
            //TODO: ONLY PUSH THRU IF ITS A VALID WORD

            string guess = "";
            bool validGuess = false;

            //Combine chars making up the guess word into a string so it's easier to compare w/ other dictionary strings
            for (int i = 0; i < NUM_COLS; i++)
            {
                guess += Char.ToString(Char.ToLower(guessBoard[curRow, i]));
            }

            //Check if either list contains specified word. First check the extras dictionary (more common). If not found, then check the answers dictionary
            if (!validGuess)
            {
                for (int i = 0; i < extraWords.Count; i++)
                {
                    if (guess == extraWords[i])
                    {
                        validGuess = true;
                    }
                }
            }
            if (!validGuess)
            {
                for (int i = 0; i < answerWords.Count; i++)
                {
                    if (guess == answerWords[i])
                    {
                        validGuess = true;
                    }
                }
            }

            if (validGuess)
            {
                CheckWord();
            }
            else
            {
                Console.WriteLine("Invalid word. Please try again");
            }
        }

        private static void CheckWord()
        {
            //Store the index of a correct letter in the incorrect position to check for duplicate occurances
            int prevLetterIndex = -1;

            //Loop through all the characters
            for (int i = 0; i < NUM_COLS; i++)
            {
                //Check if the letter from the guessed word is in the same position as the answer's letter. 
                if (Char.ToLower(guessBoard[curRow, i]) == answer[i])
                {
                    correctness[curRow, i] = 1;
                    alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] = 1;
                }
                //Check if the guess contains any characters from the answer in the wrong position
                else if(answer.Contains(Char.ToLower(guessBoard[curRow, i]))) 
                {
                    //Loop through answer's characters
                    for (int j = 0; j < NUM_COLS; j++)
                    {
                        //Check if the guess character matches the answer character, check if it's already in the correct position, and check if it's a duplicate character
                        if (Char.ToLower(guessBoard[curRow, i]) == answer[j] && Char.ToLower(guessBoard[curRow, j]) != answer[j] && prevLetterIndex != j)
                        {
                            correctness[curRow, i] = 2;
                            prevLetterIndex = j;

                            if (alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] != 1)
                            {
                                alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] = 2;
                            }
                        }
                        else
                        {
                            correctness[curRow, i] = 3;

                            if (alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] != 1 && alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] != 2)
                            {
                                alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] = 3;
                            }
                        }
                    }
                }
                else
                {
                    correctness[curRow, i] = 3;

                    if (alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] != 1 && alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] != 2)
                    {
                        alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] = 3;
                    }
                }
            }

            //TEMP
            for (int i = 0; i < NUM_COLS; i++)
            {
                Console.Write(correctness[curRow, i] + ",");
            }
            //

            int correctLetters = 0;

            for (int i = 0; i < NUM_COLS; i++)
            {
                if (correctness[curRow, i] == 1)
                {
                    correctLetters++;
                }
            }

            if (correctLetters == 5)
            {
                //CORRECT ANSWER
                DrawGame();
                Console.WriteLine("Correct guess. Good job!");
            }
            else
            {
                if (curRow + 1 < NUM_ROWS)
                {
                    //Wrong answer
                    DrawGame();
                    curCol = 0;
                    curRow++;
                }
                else
                {
                    //Wrong answer and out of turns
                    DrawGame();
                    Console.WriteLine("You lost :((");
                }
            }
        }
    }
}
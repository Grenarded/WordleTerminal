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
        static StreamWriter outFile;

        const string STATS_FILE = "Stats.txt";

        static List<string> answerWords = new List<string>();
        static List<string> extraWords = new List<string>();

        const int NUM_ROWS = 6;
        const int NUM_COLS = 5;

        static char[] alpha = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        static char[,] guessBoard = new char[NUM_ROWS, NUM_COLS];

        //1: Correct spot. 2: Correct letter wrong spot. 3: Wrong letter
        static int[,] correctness = new int[NUM_ROWS, NUM_COLS];
        static int[] alphaStatus = new int[alpha.Length];

        static int curRow = -1;
        static int curCol = 0;

        static string answer;

        //Stats
        static List<int> guessDistrib = new List<int>();
        static float gamesPlayed = 0;
        static int currentStreak;
        static int maxStreak;

        public static void Main(string[] args)
        {
            //Load dictionaries and save words
            LoadDictionaries(answerWords, "WordleAnswers.txt");
            LoadDictionaries(extraWords, "WordleExtras.txt");

            //Load the stats
            LoadStats();

            //Display Menu
            DisplayMenu();
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
                Console.WriteLine("4. Settings");
                Console.WriteLine("5. Exit\n");
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
                            DisplayInstructions();
                            break;
                        case 3:
                            //Stats
                            DisplayStats(false);
                            break;
                        case 4:
                            //Settings
                            SettingsMenu();
                            break;
                        case 5:
                            //Exit game
                            Console.WriteLine("Game should exit now!");
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

        private static void PlayGame()//REDUNDANT. FIX
        {
            Console.Clear();

            //Select random 5-letter word <--ADJUST
            answer = answerWords[rng.Next(0, answerWords.Count)];

            SetUpGame();

            HandleInput();
        }

        private static void SetUpGame()
        {
            curCol = 0;
            curRow = 0;

            //Fill board array w/ empty chars and reset correctness
            for (int i = 0; i < NUM_ROWS; i++)
            {
                for (int j = 0; j < NUM_COLS; j++)
                {
                    guessBoard[i, j] = ' ';
                    correctness[i, j] = 0;
                }
            }

            for (int i = 0; i < alphaStatus.Length; i++)
            {
                alphaStatus[i] = 0;
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
                else if (userInput == 0) //Backspace ASCII TODO: arrow keys also delete
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
                            //correctness[curRow, i] = 3; TODO: Fix this logic. Causes issues when duplicate letters appear (and only one is correct)

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

                //BANDAGE SOLUTION?
                if (correctness[curRow, i] == 0)
                {
                    correctness[curRow, i] = 3;
                }
            }

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
                Console.WriteLine("Correct, good job! \nPress enter to continue");
                Console.ReadLine();
                //STATS
                gamesPlayed++;
                guessDistrib.Add(curRow + 1);
                currentStreak++;
                DisplayStats(true);
            }
            else
            {
                if (curRow + 1 < NUM_ROWS)
                {
                    //Wrong answer
                    DrawGame();
                    //
                    curCol = 0;
                    curRow++;
                }
                else
                {
                    //Wrong answer and out of turns
                    DrawGame();
                    //TEMP
                    Console.WriteLine($"The answer was: {answer.ToUpper()} \nPress enter to continue");
                    Console.ReadLine();
                    //STATS
                    gamesPlayed++;
                    currentStreak = 0;
                    curRow++;
                    DisplayStats(true);
                }
            }
        }

        private static void DisplayInstructions()
        {
            Console.Clear();

            Console.WriteLine("How To Play");
            Console.WriteLine("-----------");
            Console.WriteLine("\nGuess the word in 6 tries\n");
            Console.WriteLine("-Each guess must be a valid 5-letter word");
            Console.WriteLine("\n-The colour of the tiles will change to show how close your guess was to the word");

            Console.Write("\t-Letters in the correct spot will be ");
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("GREEN");
            Console.ResetColor();
            Console.Write("\t-Correct letters in the wrong spot will be ");
            Console.BackgroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("YELLOW");
            Console.ResetColor();
            Console.Write("\t-Incorrect letters will be ");
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("GRAY");
            Console.ResetColor();
            Console.WriteLine("\n-Keep an eye on the alphabet at the top of your game screen,\nit will help you keep track of each letter");

            Console.WriteLine("\nPress enter to return to main menu");
            Console.ReadLine();
        }

        private static void DisplayStats(bool gamePlayed) //DOESN'T WORK RIGHT
        {
            Console.Clear();

            if (currentStreak > maxStreak)
            {
                maxStreak = currentStreak;
            }

            Console.WriteLine("Statistics");
            Console.WriteLine("----------\n");

            Console.WriteLine("Games Played: " + gamesPlayed);

            Console.Write("Win Percentage: ");
            if (gamesPlayed == 0)
            {
                Console.WriteLine("N/A");   
            }
            else
            {
                Console.WriteLine(Math.Round((guessDistrib.Count / gamesPlayed) * 100, 2) + "%");
            }

            Console.WriteLine("Current Streak: " + currentStreak);
            Console.WriteLine("Max Streak: " + maxStreak);

            //Guess distribution
            Console.WriteLine("\nGuess Distribution:\n");

            for (int i = 0; i <= NUM_COLS; i++)
            {
                Console.Write(i + 1 + " ");
                float guessesForRow = 0;
                for (int j = 0; j < guessDistrib.Count; j++)
                {
                    if (guessDistrib[j] == (i+1))
                    {
                        guessesForRow++;
                    }
                }


                float guessGraphPercent;

                if (guessDistrib.Count > 0)
                {
                    guessGraphPercent = ((guessesForRow / guessDistrib.Count) * 24);
                }
                else
                {
                    guessGraphPercent = 0;
                }

                if (curRow < NUM_ROWS)
                {
                    if (i == curRow)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    }
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                }

                for (int count = 0; count < guessGraphPercent; count++)
                {
                    Console.Write(" ");
                }

                Console.Write(guessesForRow);

                Console.ResetColor();

                Console.WriteLine();
            }

            Console.WriteLine();

            if (gamePlayed)
            {
                SaveStats();
            }

            while (true)
            {
                if (gamePlayed)
                {
                    Console.WriteLine("1. Play Again");
                    Console.WriteLine("2. Reset Stats");
                    Console.WriteLine("3. Main Menu");
                    Console.WriteLine("\nEnter Selection: ");

                    try
                    {
                        int input = Convert.ToInt32(Console.ReadLine());
                        switch (input)
                        {
                            case 1:
                                //Play
                                PlayGame();
                                break;
                            case 2:
                                //Reset stats
                                ResetStats();
                                DisplayStats(true);
                                break;
                            case 3:
                                //Stats
                                DisplayMenu();
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
                else
                {
                    Console.WriteLine("1. Reset Stats");
                    Console.WriteLine("2. Main Menu");
                    Console.WriteLine("\nEnter Selection: ");

                    try
                    {
                        int input = Convert.ToInt32(Console.ReadLine());
                        switch (input)
                        {
                            case 1:
                                //Reset stats
                                ResetStats();
                                DisplayStats(false);
                                break;
                            case 2:
                                DisplayMenu();
                                break;
                            default:
                                Console.WriteLine("Not a valid input. Press enter to try again");
                                Console.ReadLine();
                                break;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Not a valid input. Press enter to try again"); //REMOVE THIS BLOCK? IN OTHER INSTANCES AS WELL?
                        Console.ReadLine();
                    }
                }
            }
        }

        private static void SaveStats() //FILE PATH VIOLATION??
        {
            try
            {
                outFile = File.CreateText(STATS_FILE);

                outFile.WriteLine(guessDistrib.Count);

                for (int i = 0; i < guessDistrib.Count; i++)
                {
                    outFile.WriteLine(guessDistrib[i]);
                }

                outFile.WriteLine(gamesPlayed);

                outFile.Write(currentStreak + " " + maxStreak);

                outFile.Close();
            }
            catch (FileLoadException fl)
            {
                Console.WriteLine("SaveStats: " + fl.Message);
            }
            catch (FileNotFoundException fnf)
            {
                Console.WriteLine("SaveStats: " + fnf.Message);
            }
            catch(Exception e)
            {
                Console.WriteLine("SaveStats: " + e.Message);
            }
        }

        private static void LoadStats()
        {
            try
            {
                inFile = File.OpenText(STATS_FILE);

                int guessDistribCount = Convert.ToInt32(inFile.ReadLine());

                for (int i =0; i < guessDistribCount; i++)
                {
                    guessDistrib.Add(Convert.ToInt32(inFile.ReadLine()));
                }

                gamesPlayed = Convert.ToInt32(inFile.ReadLine());
                currentStreak = Convert.ToInt32(inFile.ReadLine().Split(' ')[0]);
                maxStreak = Convert.ToInt32(inFile.ReadLine().Split(' ')[1]);

                inFile.Close();
            }
            catch(FileNotFoundException)
            {
                //Set Default Stats
                gamesPlayed = 0;
                currentStreak = 0;
                maxStreak = 0;
            }
            catch(Exception e)
            {
                Console.WriteLine("Load stats: " + e.Message);
            }
        }

        private static void ResetStats()
        {
            curRow = -1;

            File.Delete(STATS_FILE);

            //Set Default Stats
            guessDistrib.Clear();
            gamesPlayed = 0;
            currentStreak = 0;
            maxStreak = 0;
        }

        private static void SettingsMenu()
        {
            Console.Clear();
            Console.WriteLine("Settings");
            Console.WriteLine("--------");
            Console.WriteLine("If you're seeing this, then unfortunately there was not enough time to complete the settings page.");
            Console.WriteLine("I considered adding ASCII art, but my pride in low line numbers kept me from doing so (and I should probably be spending that time coding anyways...)");
            Console.WriteLine("\nPress enter to return to Main Menu");
            Console.ReadLine();
            DisplayMenu();
        }
    }
}
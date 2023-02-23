//Author: Ben Petlach
//File Name: Program.cs
//Project Name: PetlachBPass1
//Creation Date: Feb. 15, 2023
//Modified Date: Feb. 23, 2023
//Description: Wordle recreation: guess the 5-letter word in 6 guesses or less

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace PetlachBPASS1
{
    class MainClass
    {
        //Board/grid size
        const int NUM_ROWS = 6;
        const int NUM_COLS = 5;

        //Number of chars for guess distribution graph to take up (max)
        const int NUM_CHARS_GRAPH = 24;


        static Random rng = new Random();

        //File IO
        static StreamReader inFile;
        static StreamWriter outFile;

        const string STATS_FILE = "Stats.txt";

        //Store all answers and potential guesses in lists for access later
        static List<string> answerWords = new List<string>();
        static List<string> extraWords = new List<string>();

        //Alphabet display
        static char[] alpha = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        //6x5 word guess board array to keep track of user guesses
        static char[,] guessBoard = new char[NUM_ROWS, NUM_COLS];

        //Arrays to track 'correctness' of guesses on both the board and alphabet display
        //1: Correct spot. 2: Correct letter wrong spot. 3: Wrong letter
        static int[,] correctness = new int[NUM_ROWS, NUM_COLS];
        static int[] alphaStatus = new int[alpha.Length];

        //Store and maintain current active row and column on active grids
        static int curRow = -1;
        static int curCol = 0;

        //Store the final answer
        static string answer;

        //Stats to keep track of
        static List<int> guessDistrib = new List<int>();
        static float gamesPlayed = 0;
        static int currentStreak;
        static int maxStreak;

        //Check if game is being played
        static bool playGame = false;

        public static void Main(string[] args)
        {
            //Load dictionaries and save words
            LoadDictionaries(answerWords, "WordleAnswers.txt");
            LoadDictionaries(extraWords, "WordleExtras.txt");

            //Load the stats from file
            LoadStats(STATS_FILE);

            //Display Menu
            DisplayMenu();
        }

        //Pre: None
        //Post: None
        //Desc: Display user interface/menu
        private static void DisplayMenu()
        {
            //Reset current row
            curRow = -1;

            //Set default input
            int input = 0;

            //Loop through menu options while program hasn't been closed (when the 5 key is pressed)
            while (input != 4)
            {
                string centeredText = "Welcome to Wordle";

                //Menu prompts
                Console.Clear();
                Console.WriteLine(CenterString("Welcome to WORDLE!\n", ""));
                //Console.WriteLine("Welcome to WORDLE!\n");
                Console.WriteLine(CenterString("1. Play", centeredText));
                //Console.WriteLine("1. Play");
                Console.WriteLine(CenterString("2. Instructions", centeredText));
                //Console.WriteLine("2. Instructions");
                Console.WriteLine(CenterString("3. Stats ", centeredText));
                //Console.WriteLine("3. Stats");
                Console.WriteLine(CenterString("4. Exit", centeredText));
                //Console.WriteLine("4. Exit\n");
                Console.WriteLine();
                Console.Write(CenterString("Enter selection: ", centeredText));
                //Console.Write("Enter selection: ");

                try
                {
                    //Store input
                    input = Convert.ToInt32(Console.ReadLine());

                    //Perform appropriate menu action
                    switch (input)
                    {
                        case 1:
                            //Play
                            playGame = true;
                            SetUpGame();
                            break;
                        case 2:
                            //Instructions
                            DisplayInstructions();
                            break;
                        case 3:
                            //Stats
                            StatsPreGameMenu();
                            break;
                        case 4:
                            //Exit game
                            break;
                        default:
                            //Response if user didn't enter valid input
                            Console.WriteLine("Not a valid input. Press <enter> or <return> to try again");
                            Console.ReadLine();
                            break;
                    }
                }
                catch
                {
                    //Response if user didn't enter valid input
                    Console.WriteLine("Not a valid input. Press <enter> or <return> to try again");
                    Console.ReadLine();
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Display instructions on how to play the game
        private static void DisplayInstructions()
        {
            Console.Clear();

            //Display instructions
            Console.WriteLine(CenterString("How to Play", ""));
            Console.WriteLine(CenterString("-----------", ""));
            Console.WriteLine();
            Console.WriteLine(CenterString("Guess the word in 6 tries", ""));
            Console.WriteLine();
            Console.WriteLine("-Each guess must be a real, valid 5-letter word");
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
            Console.WriteLine("\n-Keep an eye on the alphabet at the top of your game screen, it will help you keep track of each letter");
            Console.WriteLine("\n-Remember, there can be repeat letters! If a letter is repeated twice, the same colour coding will apply.");
            Console.WriteLine("-Hint: If the answer has two letters but all your guesses only have that letter once, it may appear that you are missing a letter or ran out of options. Keep this in mind!");

            Console.WriteLine("\nPress <enter> or <return> to return to main menu");
            Console.ReadLine();
        }

        //Pre: None
        //Post: None
        //Desc: Display the stats
        private static void DisplayStats(bool gamePlayed)
        {
            Console.Clear();

            //Check if the current streak is greater than the max streak
            if (currentStreak > maxStreak)
            {
                //Update max streak
                maxStreak = currentStreak;
            }

            Console.WriteLine("Statistics");
            Console.WriteLine("----------\n");

            //Display stats
            Console.WriteLine("Games Played: " + gamesPlayed);

            Console.Write("Win Percentage: ");
            //Check if there were 0 games played
            if (gamesPlayed == 0)
            {
                Console.WriteLine("N/A");
            }
            else
            {
                //Calculate and display win percentage
                Console.WriteLine(Math.Round((guessDistrib.Count / gamesPlayed) * 100, 2) + "%");
            }

            Console.WriteLine("Current Streak: " + currentStreak);
            Console.WriteLine("Max Streak: " + maxStreak);

            //Display guess distribution
            Console.WriteLine("\nGuess Distribution:\n");

            //Loop through each row value
            for (int i = 0; i <= NUM_COLS; i++)
            {
                Console.Write(i + 1 + " ");

                //Store number of correct guesses in the current row
                float guessesForRow = 0;

                //Loop through the row values in which the correct guesses were made
                for (int j = 0; j < guessDistrib.Count; j++)
                {
                    //Check if the integer matches the current row
                    if (guessDistrib[j] == (i + 1))
                    {
                        guessesForRow++;
                    }
                }

                //Define percentage to use for visual graphs
                float guessGraphPercent;

                //Check if there is at least one correct guess
                if (guessDistrib.Count > 0)
                {
                    //Calculate the graph percentage 
                    guessGraphPercent = ((guessesForRow / guessDistrib.Count) * NUM_CHARS_GRAPH);
                }
                else
                {
                    //Set graph percentage equal to 0
                    guessGraphPercent = 0;
                }

                //Check if current row hasn't exceeded max number of rows
                if (curRow < NUM_ROWS)
                {
                    //Check if current row matches the index, meaning this was from the game just played
                    if (i == curRow)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                    }
                    //Otherwise, this is a stat from a previous game
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    }
                }
                else
                {
                    Console.BackgroundColor = ConsoleColor.DarkGray;
                }

                //Draw graphs
                Console.Write("".PadRight((int)guessGraphPercent));

                //Write the number of correct guesses that occured in that particular row
                Console.Write(guessesForRow);

                Console.ResetColor();

                Console.WriteLine();
            }

            Console.WriteLine();

            //Check if a game was just played
            if (gamePlayed)
            {
                SaveStats();
            }
        }

        //Pre: None
        //Post: None
        //Desc: Reset values and call appropriate methods to get the game set up
        private static void SetUpGame()
        {
            while (playGame)
            {
                //Generate answer for the round
                answer = answerWords[rng.Next(0, answerWords.Count)];

                //Default column and row position for play time
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

                //Reset correctness of the alphabet
                for (int i = 0; i < alphaStatus.Length; i++)
                {
                    alphaStatus[i] = 0;
                }

                DrawGame();

                HandleInput();

                StatsPostGameMenu();
            }
        }

        //Pre: None
        //Post: None
        //Desc: Draw and colour the grid and alphabet
        private static void DrawGame()
        {
            Console.Clear();

            Console.Write(CenterString("", "".PadLeft(alpha.Length * 2)));

            //Display alphabet with appropriate colour coding
            for (int i = 0; i < alpha.Length; i++)
            {
                //If the letter is correct spot and correct position
                if (alphaStatus[i] == 1)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }
                //If the letter is correct but in the wrong spot
                else if (alphaStatus[i] == 2)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                }
                //If the letter is wrong
                else if (alphaStatus[i] == 3)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                
                Console.Write(alpha[i]);
                Console.ResetColor();

                //Space the letters for readability
                Console.Write(" ");
            }

            Console.WriteLine("\n");

            Console.WriteLine(CenterString("---------------------", ""));

            //Draw the grid and letters with the appropriate colour coding
            for (int row = 0; row < NUM_ROWS; row++)
            {
                //Center the grid rows
                Console.Write(CenterString(" ", "----------------------"));

                //Draw each column w/ the appropriate colour coding
                for (int col = 0; col < NUM_COLS; col++)
                {
                    Console.Write("|");
                    //If the letter is correct spot and correct position
                    if (correctness[row, col] == 1)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGreen;
                    }
                    //If the letter is correct but in the wrong spot
                    else if (correctness[row, col] == 2)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                    }
                    //If the letter is wrong
                    else if (correctness[row, col] == 3)
                    {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    }
                    //Draw in the char within the grid boxes
                    Console.Write(" " + guessBoard[row, col] + " ");
                    Console.ResetColor();
                }
                Console.Write("|\n");
                Console.WriteLine(CenterString("---------------------", ""));
            }
        }

        //Pre: None
        //Post: None
        //Desc: Handle user input depending on keys pressed
        private static void HandleInput()
        {
            //Maintain if the user has finished guessing the word
            bool guessComplete = false;

            //Loop while the user hasn't finished guessing
            while (!guessComplete)
            {
                //Store the key pressed
                ConsoleKey keyPressed = Console.ReadKey().Key;

                //Check if the backspace key is pressed
                if (keyPressed == ConsoleKey.Backspace)
                {
                    //Check if the player is beyond the first column (after the first letter)
                    if (curCol > 0)
                    {
                        //Replace the letter with an empty space
                        guessBoard[curRow, curCol - 1] = ' ';

                        //Shift back a column
                        curCol--;

                        //Redraw/update the board
                        DrawGame();
                    }
                }
                //Check if the enter key is pressed
                else if (keyPressed == ConsoleKey.Enter)
                {
                    //Check if the column contains a letter (meaning a full 5-word guess)
                    if (guessBoard[curRow, NUM_COLS - 1] != ' ')
                    {
                        //Set to false so loop doesn't repeat after
                        guessComplete = true;

                        //Check the guess
                        CheckGuess();
                    }
                    //If the final column does not have a letter
                    else
                    {
                        Console.WriteLine(CenterString("Not enough letters", ""));
                        //Stop cursor from moving downwards
                        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
                    }
                }
                //Check if the user typed in a valid letter from the alphabet
                else if (Convert.ToChar(keyPressed) >= 'A' && Convert.ToChar(keyPressed) <= 'Z')
                {
                    //If the user hasn't surpassed the 5 columns/letter limit
                    if (curCol < NUM_COLS)
                    {
                        //Convert the pressed key to a char and store it in the guess array
                        guessBoard[curRow, curCol] = Convert.ToChar(keyPressed);

                        //Move user to the next column to guess the next letter
                        curCol++;

                        //Draw/update the board
                        DrawGame();
                    }
                    else
                    {
                        //Stop mouse cursor from moving down on the screen
                        ClearLine();
                    }
                }
                else
                {
                    //Stop mous coursor from moving down on the screen
                    ClearLine();
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Verify the user's 5-letter guess
        private static void CheckGuess()
        {
            //Store the user's guess. Set default value
            string guess = "";

            //Store if guess was valid
            bool validGuess = false;

            //Combine chars making up the guess word into a string so it's easier to compare w/ other dictionary strings
            for (int i = 0; i < NUM_COLS; i++)
            {
                guess += Char.ToString(Char.ToLower(guessBoard[curRow, i]));
            }

            //Check if either list contains specified word. First check the extras dictionary (more common). 
            if (!validGuess)
            {
                if (extraWords.Contains(guess))
                {
                    validGuess = true;
                }
            }
            //If not found in the extras dictionary, then check the answers dictionary
            if (!validGuess)
            {
                if (answerWords.Contains(guess))
                {
                    validGuess = true;
                }
            }

            //Check if the guess was valid
            if (validGuess)
            {
                CheckWord();
            }
            else
            {
                Console.WriteLine(CenterString("Invalid word. Press <backspace> or <delete> try again",""));
                HandleInput();
            }
        }

        //Pre: None
        //Post: None
        //Desc: Check if the valid word that appears in a dictionary is the randomly generated answer from earlier
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
                    //Assign correctness of 1
                    correctness[curRow, i] = 1;

                    //Update the alphabet 
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
                            //Assigned correctness of 2
                            correctness[curRow, i] = 2;

                            //Store index position of current letter so future letters can be compared against it to avoid duplicates
                            prevLetterIndex = j;

                            //Check that the status of the alphabet at this index isn't already 1 (as to not overwrite it)
                            if (alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] != 1)
                            {
                                //Update correctness of the alphabet display
                                alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] = 2;
                            }
                        }
                        else
                        {
                            //Check that the status of the alphabet at this index isn't already 1 oe 2(as to not overwrite it)
                            if (alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] != 1 && alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] != 2)
                            {
                                //Update correctness of the alphabet display
                                alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] = 3;
                            }
                        }
                    }
                }
                else
                {
                    //Set all other characters that didn't meet prior criteria a correctness of 3
                    correctness[curRow, i] = 3;

                    //Check that the status of the alphabet at this index isn't already 1 oe 2(as to not overwrite it)
                    if (alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] != 1 && alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] != 2)
                    {
                        //Update correctness of the alphabet display
                        alphaStatus[Array.IndexOf(alpha, guessBoard[curRow, i])] = 3;
                    }
                }

                //Catch all skipped letters
                if (correctness[curRow, i] == 0)
                {
                    //Update correctness of the alphabet display
                    correctness[curRow, i] = 3;
                }
            }

            //Set default value to track number of correct letters
            int correctLetters = 0;

            //Loop through each char
            for (int i = 0; i < NUM_COLS; i++)
            {
                //Check if the char is a correct letter and in the right spot
                if (correctness[curRow, i] == 1)
                {
                    correctLetters++;
                }
            }

            //Check if all 5 letters are correct and in the right position (Correct answer)
            if (correctLetters == 5)
            {
                //Draw/update game board
                DrawGame();
                Console.WriteLine();
                Console.WriteLine(CenterString("Correct, good job!",""));
                Console.WriteLine(CenterString("Press <enter> or <return> to continue", ""));
                Console.ReadLine();

                //Update stats
                gamesPlayed++;
                guessDistrib.Add(curRow + 1);
                currentStreak++;
            }
            //If the guess was wrong
            else
            {
                //Check if there is another below for user to guess
                if (curRow + 1 < NUM_ROWS)
                {
                    //Draw/update game board
                    DrawGame();

                    //Reset column position
                    curCol = 0;

                    //Move to next row
                    curRow++;

                    HandleInput();
                }
                //If there are no more rows to guess remaining (no more turns)
                else
                {
                    //Update/draw game board
                    DrawGame();

                    //Display the write answer and prompt to continue
                    Console.WriteLine();
                    Console.WriteLine(CenterString($"The answer was: {answer.ToUpper()}",""));
                    Console.WriteLine(CenterString("Press < enter > or <return> to continue", ""));
                    Console.ReadLine();

                    //Update stats
                    gamesPlayed++;
                    currentStreak = 0;
                    curRow++;
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Maintains cursor's horizontal position on the screen
        private static void ClearLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);

            //Replace any typed letter with a space
            Console.Write(new string(' ', Console.WindowWidth));

            Console.SetCursorPosition(0, Console.CursorTop - 1);
        }

        //Pre: text to center as a string, text to horizontally align with
        //Post: text with appropriate padding to center it
        //Desc: Centers given text on the screen, either directly in the center or horizontally aligned with the specified text
        private static string CenterString(string text, string longestText)
        {
            int screenWidth = Console.WindowWidth;
            int stringWidth = text.Length;
            int longStringWidth = longestText.Length;
            int spaces = (screenWidth / 2) + (stringWidth / 2) - (longStringWidth / 2);

            if (longestText != "")
            {
                spaces += (stringWidth / 2);
            }

            return text.PadLeft(spaces);
        }

        //Pre: None
        //Post: None
        //Desc: Display the stats and menu if user wants to see the stats before the game
        private static void StatsPreGameMenu()
        {
            ////Display the stats
            //DisplayStats(false);

            //Define input and set default value
            int input = 0;

            //Loop while 2 (exit value) isn't pressed
            while (input != 2)
            {
                //Display the stats
                DisplayStats(false);

                Console.WriteLine("1. Reset Stats");
                Console.WriteLine("2. Main Menu");
                Console.Write("\nEnter Selection: ");

                try
                {
                    //Convert input to integer
                    input = Convert.ToInt32(Console.ReadLine());
                    //Perform appropriate menu action
                    switch (input)
                    {
                        case 1:
                            //Reset stats
                            ResetStats();
                            DisplayStats(false);
                            break;
                        case 2:
                            break;
                        default:
                            //If a valid input wasn't entered
                            Console.WriteLine("Not a valid input. Press <enter> or <return> to try again");
                            Console.ReadLine();
                            break;
                    }
                }
                catch
                {
                    //If a valid input wasn't entered
                    Console.WriteLine("Not a valid input. Press <enter> or <return> to try again");
                    Console.ReadLine();
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Display the stats and menu after the game is played
        private static void StatsPostGameMenu()
        {
            //Define input and set default value
            int input = 0;

            //Loop through while loop doesn't need to be exited or the exit key (3) isn't pressed
            while (input != 3 && input != 1)
            {
                //Display the stats
                DisplayStats(true);

                //Menu prompts
                Console.WriteLine("1. Play Again");
                Console.WriteLine("2. Reset Stats");
                Console.WriteLine("3. Main Menu");
                Console.Write("\nEnter Selection: ");

                try
                {
                    //Convert input to int
                    input = Convert.ToInt32(Console.ReadLine());

                    //Perform appropriate menu actions
                    switch (input)
                    {
                        case 1:
                            //Break the loop and go back to the SetUpGame loop
                            break;
                        case 2:
                            //Reset stats
                            ResetStats();
                            DisplayStats(true);
                            break;
                        case 3:
                            //Stop the SetUpGame loop to return back to main menu
                            playGame = false;
                            break;
                        default:
                            //If a valid input wasn't entered
                            Console.WriteLine("Not a valid input. Press <enter> or <return> to try again");
                            Console.ReadLine();
                            break;
                    }
                }
                catch
                {
                    //If a valid input wasn't entered
                    Console.WriteLine("Not a valid input. Press <enter> or <return> to try again");
                    Console.ReadLine();
                }
            }
        }

        //Pre: list in which to store the words, file to read
        //Post: None
        //Desc: Read the specified text file and store it into the respective list
        private static void LoadDictionaries(List<string> wordList, string fileName)
        {
            try
            {
                //Open the file to read
                inFile = File.OpenText(fileName);

                //Add each word in the file to the list until the end of the file is reached
                while (!inFile.EndOfStream)
                {
                    wordList.Add(inFile.ReadLine());
                }
            }
            catch (FileNotFoundException fnf)
            {
                Console.WriteLine(fnf.Message);
            }
            catch (FormatException fe)
            {
                Console.WriteLine(fe.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                //Close file if it has been opened
                if (inFile != null)
                {
                    inFile.Close();
                }
            }
        }

        //Pre: stats file name as a string
        //Post: None
        //Desc: Read the stats file and store stats
        private static void LoadStats(string fileName)
        {
            try
            {
                //Open stats file
                inFile = File.OpenText(fileName);

                //Store number of lines to read for the guess distribution of rows
                int guessDistribCount = Convert.ToInt32(inFile.ReadLine());

                //Loop through the lines containing the guess distribution values
                for (int i = 0; i < guessDistribCount; i++)
                {
                    //Store guess distribution values into the appropriate list
                    guessDistrib.Add(Convert.ToInt32(inFile.ReadLine()));
                }

                //Store other stats
                gamesPlayed = Convert.ToInt32(inFile.ReadLine());
                currentStreak = Convert.ToInt32(inFile.ReadLine().Split(' ')[0]);
                maxStreak = Convert.ToInt32(inFile.ReadLine().Split(' ')[1]);
            }
            catch (FileNotFoundException)
            {
                //Set Default Stats if file does not exist (meaning it was reset)
                gamesPlayed = 0;
                currentStreak = 0;
                maxStreak = 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("Load stats: " + e.Message);
            }
            finally
            {
                //Check if file was accessed
                if (inFile != null)
                {
                    inFile.Close();
                }
            }
        }

        //Pre: None
        //Post: None
        //Desc: Delete all saved stats
        private static void ResetStats()
        {
            //Default row value so that bar doesn't show as green on guess distribution
            curRow = -1;

            //Delete stats file
            File.Delete(STATS_FILE);

            //Set Default Stats
            guessDistrib.Clear();
            gamesPlayed = 0;
            currentStreak = 0;
            maxStreak = 0;
        }

        //Pre: None
        //Post: None
        //Desc: Write player stats to a text file
        private static void SaveStats() 
        {
            try
            {
                //Create file (or overwrite if it already exists)
                outFile = File.CreateText(STATS_FILE);

                //Output total count of correct guess distributions over the 6 rows
                outFile.WriteLine(guessDistrib.Count);

                //Loop through each guess distribution row and write it on a new line
                for (int i = 0; i < guessDistrib.Count; i++)
                {
                    outFile.WriteLine(guessDistrib[i]);
                }

                //Write number of games played
                outFile.WriteLine(gamesPlayed);

                //Write the current and max streak
                outFile.Write(currentStreak + " " + maxStreak);
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
            finally
            {
                //Check if file was previously accessed
                if (outFile != null)
                {
                    outFile.Close();
                }
            }
        }
    }
}
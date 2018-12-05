using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

namespace MasterMind
{
    public class Game
    {
        private string _modeInput;
        private readonly ConsoleColor[] _colors = new ConsoleColor[] { ConsoleColor.Blue, ConsoleColor.Cyan, ConsoleColor.Green, ConsoleColor.Magenta, ConsoleColor.Red, ConsoleColor.White, ConsoleColor.Yellow };
        private readonly string[] _separators = { ",", " " };
        private int _numColumns;
        private int _numAttempts;
        private string[] _secretCombination;
        private bool _hasWon = false;

        public void Play()
        {
            Initialization();

            int player = 1;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  Player " + player + "'s Turn!");
            Console.ForegroundColor = ConsoleColor.White;
            
            TakeNumColumns();

            //the array can now have a fixed length
            _secretCombination = new string[_numColumns];

            TakeNumAttempts();

            //either player 2 plays or computer plays
            if(_modeInput == "0") ComputerMove(_numColumns, _secretCombination);
            else Player2Move(_numColumns);

            player = 1;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  Player " + player + "'s Turn!");
            Console.ForegroundColor = ConsoleColor.White;

            //loop in number of attempts
            while (_numAttempts > 0)
            {
                Console.WriteLine("\n  Number of attempts left: " + _numAttempts);

                //takes users input and handle
                string[] takeGuess = TakeGuess(_numColumns);
                int[] cp = CheckGuess(takeGuess, _secretCombination, _numColumns);
                int correctplace = HandleGuesses(_numColumns, cp[0], cp[1]);

                //if correct place and color = same number of columns, winsss
                if (correctplace == _numColumns)
                {
                    _hasWon = true;
                    break;
                }

                _numAttempts--;
            }

            if (_hasWon)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\tYou Won!!!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\n\tYou Lost!!!");
            }

            //waits 5 seconds before closing
            System.Threading.Thread.Sleep(5000);
        }

        public int TakeNumAttempts()
        {
            try
            {
                Console.Write("  Please Select Number Of Attempts: ");
                _numAttempts = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());
                if(_numAttempts < 0) throw new Exception();
            }
            catch (Exception)
            {
                Console.Write("  Write a valid number of attempts: ");
                _numAttempts = TakeNumAttempts();
            }

            return _numAttempts;
        }

        public int TakeNumColumns()
        {
            try
            {
                Console.Write("  Please Select Number Of Columns: ");
                string readLine = Console.ReadLine();
                _numColumns = int.Parse(readLine ?? throw new InvalidOperationException());
                if(_numColumns < 1) throw new Exception();
            }
            catch (Exception)
            {
                Console.Write("  Write a valid number of columns: ");
                _numColumns = TakeNumColumns();
            }

            return _numColumns;
        }


        public int HandleGuesses(int numColumns ,int correctPlace, int correctColor)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Correct Color and Position: " + correctPlace + ", Correct Color Only: " + correctColor +
                              ", Incorrect: " + (numColumns - (correctColor + correctPlace)));
            Console.ForegroundColor = ConsoleColor.White;

            return correctPlace;
        }

        public string[] TakeGuess(int numColumns)
        {
            string[] guess;
            DisplayColors();
            Console.Write("  Please Select a combination to guess(format: x,x,x,x): ");
            try
            {
                guess = Console.ReadLine().Split(_separators, StringSplitOptions.RemoveEmptyEntries);

                if (guess.Length != numColumns)
                    throw new Exception();
            }
            catch (Exception)
            {
                Console.Write("  Please Select a valid combination to guess(format: x,x,x,x): ");
                guess = TakeGuess(numColumns);
            }

            return guess;
        }

        public int[] CheckGuess(string[] guess, string[] secretCombination, int numColumns)
        {
            int correctPlace = 0;
            int correctColor = 0;

            int index = 0;
            //make sure everything is in lower case
            foreach (string secretC in secretCombination)
            {
                if (secretC.ToLower() == guess[index].ToLower())
                {
                    correctPlace++;
                }

                index++;
            }

            //to get index we must use list instead of array
            List<string> guessList = guess.ToList();
            for (int i = 0; i < numColumns; i++)
            {
                if (guess.Contains(secretCombination[i]))
                {
                    correctColor++;
                    int indexGuess = guessList.IndexOf(secretCombination[i]);
                    guess[indexGuess] = null;
                }
            }

            correctColor -= correctPlace;

            return new int[]{correctPlace, correctColor};
        }

        public string[] Player2Move(int numColumns)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  Player 2's Turn!");
            Console.ForegroundColor = ConsoleColor.White;

            DisplayColors();
            Console.Write("  Please Select A Secret Combination with " + numColumns + " Columns(format: x,x,x,x): ");
            
            _secretCombination = TakeSecretCombination(numColumns);

            //make sure everything is in lower case
            _secretCombination = _secretCombination.Select(s => s.ToLowerInvariant()).ToArray();

            return _secretCombination;
        }

        private string[] TakeSecretCombination(int numColumns)
        {
            try
            {
                _secretCombination = Console.ReadLine().Split(_separators, StringSplitOptions.RemoveEmptyEntries);

                if(_secretCombination.Length != numColumns)
                    throw new Exception();
            }
            catch (Exception)
            {
                Console.Write("  Please select a valid combination " + numColumns + " Columns(format: x,x,x,x): ");
                _secretCombination = TakeSecretCombination(numColumns);
            }

            return _secretCombination;
        }

        public string[] ComputerMove(int numColumns, string[] secretCombination)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  Computer's Turn!");
            Console.ForegroundColor = ConsoleColor.White;

            var rand = new Random();

            for (int i = 0; i < numColumns; i++)
            {
                int randIntColor = rand.Next(_colors.Length);
                string randColor = _colors.ElementAt(randIntColor).ToString().Substring(0, 1).ToLower();

                secretCombination.SetValue(randColor, i);
            }

            Console.WriteLine("  Combination Selected by Computer!");

            return secretCombination;
        }

        public int Initialization()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n\tWelcome to MasterMind Game by Umar Callachand!");
            Console.ForegroundColor = ConsoleColor.White;

            Console.Write("\n  Please Select Mode: Computer vs Player (0) or Player 1 vs player 2 (1): ");
            _modeInput = Console.ReadLine();

            string mode = _modeInput == "0" ? "Computer vs Player" : "Player 1 vs Player 2";

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n\t" + mode);
            Console.ForegroundColor = ConsoleColor.White;

            return Convert.ToInt32(_modeInput);
        }

        private void DisplayColors()
        {
            Console.WriteLine();
            foreach (var name in _colors)
            {
                Console.Write("  ");

                Console.BackgroundColor = name;
                Console.Write("  ");

                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write(name.ToString() + "(" + name.ToString().Substring(0, 1) + ")");
            }
            Console.WriteLine();
        }

        
    }
}

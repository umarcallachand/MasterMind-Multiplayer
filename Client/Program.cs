using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using MasterMind;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;

namespace Client
{
    class Program
    {
        private static Game _game = new Game();
        private static string _serverIp;
        private static int _serverPort;

        private static int _playerNum;
        private static int _numColumns;
        private static int _numAttempts;

        private static bool _canStartGame = false;
        private static bool _canTakeCol = false;
        private static bool _canTakeAttempt = false;
        private static bool _canPlyer2Move = false;
        private static bool _cantakeGuess = false;

        private static bool _canTakeNextGuess = false;

        private static string[] _secretCombination;

        private static bool _hasWon = false;

        static void Main(string[] args)
        {
            //These methods will be triggered when an incoming messgae of that specified title is received
            //the type of receipt is explicitly stated
            NetworkComms.AppendGlobalIncomingPacketHandler<int>("Player", GetPlayerNumber);
            NetworkComms.AppendGlobalIncomingPacketHandler<int>("NumColumns", GetColumnNumber);
            NetworkComms.AppendGlobalIncomingPacketHandler<int>("canTakeGuess", GetApprovalforGuess);
            NetworkComms.AppendGlobalIncomingPacketHandler<int[]>("CheckGuess", GetCheckGuess);

            //Connection.StartListening(ConnectionType.TCP, new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0));

            //Request server IP and port number
            TakeServerInfo();

            while(true)
            {
                if (_canStartGame)
                {
                    //starts game
                    _game = new Game();

                    int mode = _game.Initialization();
                    NetworkComms.SendObject("Mode", _serverIp, _serverPort, mode);
                    _canStartGame = false;
                }

                if (_canTakeCol)
                {
                    _numColumns = _game.TakeNumColumns();
                    NetworkComms.SendObject("numColumns", _serverIp, _serverPort, _numColumns);
                    _canTakeCol = false;
                    _canTakeAttempt = true;
                }

                if (_canTakeAttempt)
                {
                    _numAttempts = _game.TakeNumAttempts();
                    NetworkComms.SendObject("numAttempts", _serverIp, _serverPort, _numAttempts);
                    Console.WriteLine("Wait for Second Player to choose Secret Combination..");
                    _canTakeAttempt = false;
                }

                if (_canPlyer2Move)
                {
                    _secretCombination = _game.Player2Move(_numColumns);
                    NetworkComms.SendObject("secretCombination", _serverIp, _serverPort, _secretCombination);
                    _canPlyer2Move = false;
                }

                while (_cantakeGuess && _numAttempts > 0 && !_hasWon && _canTakeNextGuess)
                {
                    string[] guess = _game.TakeGuess(_numColumns);
                    NetworkComms.SendObject("guess", _serverIp, _serverPort, guess);

                    _numAttempts--;
                    _canTakeNextGuess = false;
                }

                if (_hasWon)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n\tYou Won!!!");
                    Console.WriteLine("\n\n\n\nPress any key to close");

                    break;
                }

                if (_numAttempts == 0 && _cantakeGuess)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\n\tYou Lost!!!");
                    Console.WriteLine("\n\n\n\nPress any key to close");
                    break;
                }

                //Console.WriteLine("Please Wait...");

            }

            //We have used comms so we make sure to call shutdown
            Console.Read();
            NetworkComms.Shutdown();
        }

        private static void TakeServerInfo()
        {
            try
            {
                Console.WriteLine("Please enter the server IP and port in the format 192.168.0.1:10000 and press return:");
                string serverInfo = Console.ReadLine();

                //Parse the necessary information out of the provided string
                _serverIp = serverInfo.Split(':').First();
                _serverPort = int.Parse(serverInfo.Split(':').Last());

                NetworkComms.SendObject("Connect", _serverIp, _serverPort, 1);
            }
            catch (Exception)
            {
                Console.WriteLine("\nInvalid!");
                TakeServerInfo();
            }
        }

        private static void GetCheckGuess(PacketHeader packetheader, Connection connection, int[] cp)
        {
            //this will print out the number of correct positions..
            int correctPlaces = _game.HandleGuesses(_numColumns, cp[0], cp[1]);

            //checks for wins
            if (correctPlaces == _numColumns)
            {
                _hasWon = true;
            }
            else
            {
                _canTakeNextGuess = true;
            }
        }

        private static void GetApprovalforGuess(PacketHeader packetheader, Connection connection, int isTrue)
        {
            _cantakeGuess = true;
            _canTakeNextGuess = true;
        }

        private static void GetColumnNumber(PacketHeader packetheader, Connection connection, int numColumnsClient)
        {
            if (_playerNum == 2)
            {
                _numColumns = numColumnsClient;
                _canPlyer2Move = true;
            }
            
        }

        private static void GetPlayerNumber(PacketHeader header, Connection connection, int playerNumber)
        {
            _playerNum = playerNumber;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  You are Player "+ playerNumber + "!");
            Console.ForegroundColor = ConsoleColor.White;

            _canStartGame = true;

            if (playerNumber == 1) _canTakeCol = true; //first player chooses columns
            else Console.WriteLine("Wait for First Player to choose Column Number..");
        }
    }
}

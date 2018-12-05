using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MasterMind;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;

/*
 * I have used the NetWorkComms library
 * The server will print out all available tcp connection
 */

namespace Server
{
    class Program
    {
        private static Game _game = new Game();

        private static Connection _connection1 = null;
        private static Connection _connection2 = null;

        private static int _ipLength;
        private static string _destinationIpAddress1;
        private static int _destinationPort1;
        private static string _destinationIpAddress2;
        private static int _destinationPort2;


        private static int _mode;
        private static int _numColumns;
        private static int _numAttempts;
        private static string[] _secretCombination;

        static void Main(string[] args)
        {
            //These methods will be triggered when an incoming messgae of that specified title is received
            //the type of receipt is explicitly stated
            NetworkComms.AppendGlobalIncomingPacketHandler<int>("Connect", GetConnect);
            NetworkComms.AppendGlobalIncomingPacketHandler<int>("Mode", GetMode);
            NetworkComms.AppendGlobalIncomingPacketHandler<int>("numColumns", GetColumns);
            NetworkComms.AppendGlobalIncomingPacketHandler<int>("numAttempts", GetAttempts);
            NetworkComms.AppendGlobalIncomingPacketHandler<string[]>("secretCombination", GetsecretCombination);
            NetworkComms.AppendGlobalIncomingPacketHandler<string[]>("guess", HandleGuess);

            //Start listening for incoming connections
            Connection.StartListening(ConnectionType.TCP, new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0));

            //Print out the IPs and ports we are now listening on
            Console.WriteLine("Server listening for TCP connection on:");
            foreach (System.Net.IPEndPoint localEndPoint in Connection.ExistingLocalListenEndPoints(ConnectionType.TCP))
                Console.WriteLine("{0}:{1}", localEndPoint.Address, localEndPoint.Port);

            //Let the user close the server
            Console.WriteLine("\nPress any key to close server.");
            Console.ReadKey(true);

            //I have used NetworkComms so we should ensure that we correctly call shutdown
            NetworkComms.Shutdown();
        }

        /// <summary>
        /// Handle the message receive
        /// </summary>
        /// <param name="packetheader">The packet header associated with the incoming message</param>
        /// <param name="connection">The connection used by the incoming message</param>
        /// <param name="incomingobject">The message received</param>
        private static void GetConnect(PacketHeader packetheader, Connection connection, int incomingobject)
        {
            //as soon as players are connected, they are assignd with their player number
            GetPlayers(connection);
        }

        private static void HandleGuess(PacketHeader packetheader, Connection connection, string[] guess)
        {
            //the server checks the guess from client and send correct positions to client (player 1)
            int[] cp = _game.CheckGuess(guess, _secretCombination, _numColumns);
            NetworkComms.SendObject("CheckGuess", _destinationIpAddress1, _destinationPort1, cp);
        }

        private static void GetsecretCombination(PacketHeader packetheader, Connection connection, string[] secretCombination)
        {
            //if player vs player, we take secret combination from player 2
            //and send can take guess to player 1 so as the latter can start guessing
            if(_mode == 1)
            {
                _secretCombination = secretCombination;
                NetworkComms.SendObject("canTakeGuess", _destinationIpAddress1, _destinationPort1, 1);
            }
        }

        private static void GetColumns(PacketHeader packetheader, Connection connection, int numColumnsClient)
        {
            _numColumns = numColumnsClient;

            //secret Combination has now a fixed length
            _secretCombination = new string[numColumnsClient];
        }

        private static void GetAttempts(PacketHeader packetheader, Connection connection, int numAttemptsClient)
        {
            _numAttempts = numAttemptsClient;

            //after choosing number of attempts, the secret combination can be created
            if (_mode == 0)
            {
                _secretCombination = _game.ComputerMove(_numColumns, _secretCombination);
                NetworkComms.SendObject("canTakeGuess", _destinationIpAddress1, _destinationPort1, 1);
            }
            else
            {
                //when player has selected number of columns and number of attempts
                //player 2 can chose secret Combination
                if (_connection2 != null) NetworkComms.SendObject("NumColumns", _destinationIpAddress2, _destinationPort2, _numColumns);
            }
        }

        
        private static void GetPlayers(Connection connection)
        {
            if (_connection1 == null)
            {
                _connection1 = connection;

                //gets ip address from string of connection
                _ipLength = connection.ToString().Length - 10 - 4 - 25 - 10 - 2;
                _destinationIpAddress1 = connection.ToString().Substring(10 + _ipLength / 2 + 6 + 4, _ipLength / 2);
                _destinationPort1 = Convert.ToInt32(connection.ToString().Substring(10 + _ipLength / 2 + 6 + 4 + _ipLength / 2 + 1, 5));

                //string connectionSubstring = connection.ToString().Substring(10);
                //_ipLength = connectionSubstring.IndexOf(":", StringComparison.Ordinal) * 2;
                //_destinationIpAddress1 = connectionSubstring.Substring(0, _ipLength/2);
                //_destinationPort1 = Convert.ToInt32(connectionSubstring.Substring(_ipLength/2 + 1, 5));

                NetworkComms.SendObject("Player", _destinationIpAddress1, _destinationPort1, 1);

            }
            else if (_connection1 != connection && _connection2 != connection)
            {
                _connection2 = connection;

                //gets ip address from string of connection
                _ipLength = connection.ToString().Length - 10 - 4 - 25 - 10 - 2;
                _destinationIpAddress2 = connection.ToString().Substring(10 + _ipLength / 2 + 6 + 4, _ipLength / 2);
                _destinationPort2 = Convert.ToInt32(connection.ToString().Substring(10 + _ipLength / 2 + 6 + 4 + _ipLength / 2 + 1, 5));

                NetworkComms.SendObject("Player", _destinationIpAddress2, _destinationPort2, 2);

                //stops listening when 2 players connected
                Connection.StopListening();
            }


            Console.WriteLine("Player 1 is : " + _connection1?.ToString());
            Console.WriteLine("Player 2 is : " + _connection2?.ToString());
        }

        private static void GetMode(PacketHeader header, Connection connection, int modeClient)
        {
            _mode = modeClient;
        }
    }
    
}

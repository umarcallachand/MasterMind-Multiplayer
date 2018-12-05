using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

/*
 * This is the startup project, if pressed RUN in visual studio, the game will be played without server and client.
 *
 * TO PLAY WITH CLIENT AND SERVER, YOU NEED TO EITHER CHANGE STARTUP PROJECT OR
 * LAUNCH THE SERVER.EXE AND CLIENT.EXE IN DEBUG FOLDER
 *
 * THE SERVER CAN LISTEN TO INCOMING REQUESTS FROM OTHER PCs
 * USE THE PROPER IP ADDRESS (DO NOT USE IPV6 ADDRESS)
 */

namespace MasterMind
{
    class Program
    {
        static void Main(string[] args)
        {
            //creates game and start playing it
            Game game = new Game();
            game.Play();
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpierenceBot
{

    public class WelcomeState
    {
        /// <summary>
        /// checks if user was welcomed
        /// </summary>
        /// <value>true/false.</value>
        public bool DidBotWelcomeUser { get; set; } = false;
        public string Username { get; set; } = "null";

    }

}
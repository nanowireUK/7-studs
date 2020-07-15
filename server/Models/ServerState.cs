using System;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace SevenStuds.Models
{
    public static class ServerState
    {
        // Enables a game object to be found from its ID 
        public static Hashtable GameList = new Hashtable();
    }
}
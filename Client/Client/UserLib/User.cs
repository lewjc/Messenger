﻿using System;

namespace Client.Client.User
{
    public class User
    {
        
        int number { get; set; }
        string username { get; set; }

        public User(int number, string username)
        {
            this.number = number;
            this.username = username;
        }


    }
}
using System;

namespace Client.UserLib
{
    public class User
    {
        public int number { get; set; }
        public string username { get; set; }
        public int permissions { get; set; }
        public string firstname{ get; set; }

        /// <summary>
        /// Defualt no args constructor
        /// </summary>
        public User()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Client.UserLib.User"/> class.
        /// </summary>
        /// <param name="number">The user's current number in the server session..</param>
        /// <param name="username">The user's username.</param>
        /// <param name="permissions">The user's permissions.</param>
        /// <param name="firstName">The user's first name</param>
        public User(int number, string username, string firstName, int permissions)
        {
            this.number = number;
            this.username = username;
            this.permissions = permissions;
            this.firstname = firstName;
        }

    }
}

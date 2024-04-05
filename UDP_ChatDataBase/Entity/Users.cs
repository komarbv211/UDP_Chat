using System.ComponentModel.DataAnnotations;
using UDP_ChatDataBase.Entity;

namespace UDP_ChatDataBase
{
    public class Users
    {
        public int Id { get; set; }
        public string FullName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public override string ToString()
        {
            return $"Id: {Id}, Full Name: {FullName}, Email: {Email}, Admin: {(IsAdmin ? "Yes" : "No")}";
        }
    }
}

using System;
using System.ComponentModel.DataAnnotations;

namespace UDP_ChatDataBase.Entity
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime Timestamp { get; set; }

        public int UsersId { get; set; }


        public Users Users { get; set; }

        public override string ToString()
        {
            string formattedTimestamp = Timestamp.ToString("dd-MM-yyyy HH:mm:ss");
            return $"<Message from {Users.FullName} {Users.Email}>\n<{formattedTimestamp}>\t {Content}";
        }
    }
}

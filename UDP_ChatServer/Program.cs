namespace UDP_ChatServer
{
    internal class Program
    {
        static void Main(string[] args)
        {

            UDP_ChatServer chatServer = new UDP_ChatServer();
            chatServer.ReceiveMessages();

        }
    }
}

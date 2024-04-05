using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UDP_ChatServer.Repositories;
using UDP_ChatServer.Services;

namespace UDP_ChatServer
{
    internal class UDP_ChatServer
    {
        private const short remotePort = 1234;

        HashSet<IPEndPoint> members = new HashSet<IPEndPoint>();



        public void ReceiveMessages()
        {
            try
            {
                using (var server = new UdpClient(new IPEndPoint(IPAddress.Any, remotePort)))
                {
                    while (true)
                    {
                        Console.WriteLine("Waiting for a message...");
                        IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                        byte[] data = server.Receive(ref clientEndPoint);
                        string message = Encoding.Unicode.GetString(data);

                        Console.WriteLine($"\tGot the {message} message");

                        // Розпізнаємо тип повідомлення та виконуємо відповідну дію
                        ProcessMessage(message, clientEndPoint, server);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task ProcessMessage(string message, IPEndPoint clientEndPoint, UdpClient server)
        {
            string[] messageParts = message.Split(':');
            string messageType = messageParts[0];

            switch (messageType)
            {
                case "<join>":
                    UserServices.ConnectUser(clientEndPoint, server);
                    members.Add(clientEndPoint);
                    Console.WriteLine($"Number of connections: {members.Count}");
                    break;
                case "<leave>":
                    await UserServices.RemoveMember(clientEndPoint, server);
                    members.Remove(clientEndPoint);
                    Console.WriteLine("Remove Member");
                    break;
                case "verification":
                    await UserServices.SendVerificationResponse(message, clientEndPoint, server);
                    break;
                case "registration":
                    await UserServices.RegisterUser(message, clientEndPoint, server);
                    break;
                case "<message>":
                    await UserServices.AddMessageToTheDataBase(message, server);
                    await UserServices.SendingNotificationsToUsers(message, server, members);
                    break;
                case "adminAction":
                    await ProcessAdminAction(messageParts, clientEndPoint, server);
                    break;
                case "getUserId":
                    await UserServices.SendUserIdResponse(messageParts, clientEndPoint, server);
                    break;
                default:
                    break;
            }
        }

        private async Task ProcessAdminAction(string[] messageParts, IPEndPoint clientEndPoint, UdpClient server)
        {
            if (messageParts.Length >= 3)
            {
                string action = messageParts[1];
                string userID =messageParts[2];
                switch (action)
                {
                    case "searchForUserMessages":
                        await UserServices.SearchUserMessages(int.Parse(userID), clientEndPoint, server);
                        break;
                    case "GetListAllUsers":
                        await UserServices.GetListAllUsers(clientEndPoint, server);
                        break;
                    case "grantAdmin":
                        await UserServices.GrantAdminRights(int.Parse(userID), clientEndPoint, server);
                        break;
                    case "removeAdmin":
                        await UserServices.RemoveAdminRights(int.Parse(userID), clientEndPoint, server);
                        break;
                    case "deleteUser":
                        await UserServices.DeleteUser(int.Parse(userID), clientEndPoint, server);
                        break;
                    default:
                        break;
                }
            }
        }

    }
}
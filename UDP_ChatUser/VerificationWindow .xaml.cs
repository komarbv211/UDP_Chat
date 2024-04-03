using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;

namespace UDP_ChatUser
{
    public partial class VerificationWindow : Window
    {
        public IPAddress? IpAddress { get; set; }
        public int Port { get; set; }
        public int Id { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        public VerificationWindow(IPAddress ipAddress, int port)
        {
            InitializeComponent();
            IpAddress = ipAddress;
            Port = port;
        }
        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            Email = EmailTextBox.Text.Trim();
            Password = PasswordTextBox.Password.Trim();

            // Відправка запиту на сервер для верифікації
            try
            {
                using (UdpClient client = new UdpClient())
                {
                    // Формування повідомлення для сервера
                    string message = $"verification:{Email}:{Password}";
                    // Відправлення повідомлення на сервер для верифікації
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    IPEndPoint serverEndPoint = new IPEndPoint(IpAddress, Port);
                    client.Send(data, data.Length, serverEndPoint);

                    // Очікування відповіді від сервера
                    byte[] response = client.Receive(ref serverEndPoint);
                    string responseMessage = Encoding.Unicode.GetString(response);

                    if (responseMessage == "Admin" || responseMessage == "RegularUser")
                    {
                        GetUserIdByEmail(Email);
                    }
                    // Обробка відповіді від сервера
                    HandleServerResponse(responseMessage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        public void GetUserIdByEmail(string email)
        {
            try
            {

                using (UdpClient client = new UdpClient())
                {
                    // Формування повідомлення для сервера
                    string message = $"getUserId:{email}";
                    // Відправлення повідомлення на сервер для отримання ID користувача
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    IPEndPoint serverEndPoint = new IPEndPoint(IpAddress, Port);
                    client.Send(data, data.Length, serverEndPoint);

                    // Очікування відповіді від сервера
                    byte[] response = client.Receive(ref serverEndPoint);
                    string responseMessage = Encoding.Unicode.GetString(response);

                    // Перевірка, чи успішно отримано ID користувача
                    int userId;
                    if (int.TryParse(responseMessage, out userId))
                    {
                        Id = userId;
                    }
                    else
                    {
                        MessageBox.Show("Error: Invalid response from server");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
        public void HandleServerResponse(string responseMessage)
        {
            switch (responseMessage)
            {
                case "Admin":
                    //MessageBox.Show("Welcome, Admin!");
                    this.IsEnabled = false;
                    this.Visibility = Visibility.Collapsed;

                    var adminWindow = new AdminWindow(Id, IpAddress, Port);
                    adminWindow.ShowDialog();
                    this.Close();
                    break;
                case "RegularUser":
                    //MessageBox.Show("Welcome, User!");
                    this.IsEnabled = false;
                    this.Visibility = Visibility.Collapsed;

                    var userWindow = new UserWindow(Id, IpAddress, Port);
                    userWindow.ShowDialog();
                    this.Close();
                    break;
                case "NotRegistered":
                    MessageBox.Show("You are not registered. Please register first.");
                    break;
                default:
                    break;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow registrationWindow = new RegistrationWindow(IpAddress, Port);
            registrationWindow.ShowDialog();
        }
    }
}

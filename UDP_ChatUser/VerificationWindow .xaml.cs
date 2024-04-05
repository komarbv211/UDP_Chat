using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.IO;

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

            LoadLastThreeEmails(); 
        }


        private void LoadLastThreeEmails()
        {
            try
            {
                string filePath = "EmailAddresses.txt";
                if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            EmailComboBox.Items.Add(line);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading email addresses from file: {ex.Message}");
            }
        }

        private void SaveLastThreeEmails(string newEmail)
        {
            try
            {
                string filePath = "EmailAddresses.txt";
                List<string> emails = new List<string>();

                emails.Add(newEmail);

                foreach (var item in EmailComboBox.Items)
                {
                    if (!emails.Contains(item.ToString()))
                    {
                        emails.Add(item.ToString());
                    }
                }

                if (emails.Count > 3)
                {
                    emails = emails.GetRange(emails.Count - 3, 3);
                }

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (var email in emails)
                    {
                        writer.WriteLine(email);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving email addresses: {ex.Message}");
            }
        }
        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            Email = EmailComboBox.Text.Trim();
            Password = PasswordTextBox.Password.Trim();


            try
            {
                using (UdpClient client = new UdpClient())
                {

                    string message = $"verification:{Email}:{Password}";


                    byte[] data = Encoding.Unicode.GetBytes(message);
                    IPEndPoint serverEndPoint = new IPEndPoint(IpAddress, Port);
                    client.Send(data, data.Length, serverEndPoint);


                    byte[] response = client.Receive(ref serverEndPoint);
                    string responseMessage = Encoding.Unicode.GetString(response);

                    if (responseMessage == "Admin" || responseMessage == "RegularUser")
                    {
                        GetUserIdByEmail(Email);
                        SaveLastThreeEmails(Email); 
                    }

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

                    string message = $"getUserId:{email}";

                    byte[] data = Encoding.Unicode.GetBytes(message);
                    IPEndPoint serverEndPoint = new IPEndPoint(IpAddress, Port);
                    client.Send(data, data.Length, serverEndPoint);


                    byte[] response = client.Receive(ref serverEndPoint);
                    string responseMessage = Encoding.Unicode.GetString(response);


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

                    this.IsEnabled = false;
                    this.Visibility = Visibility.Collapsed;

                    var adminWindow = new AdminWindow(Id, IpAddress, Port);
                    adminWindow.ShowDialog();

                    this.Close();
                    break;
                case "RegularUser":

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

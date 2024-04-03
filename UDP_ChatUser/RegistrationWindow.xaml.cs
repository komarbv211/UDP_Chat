using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;

namespace UDP_ChatUser
{
    public partial class RegistrationWindow : Window
    {
        public IPAddress? IpAddress { get; set; }
        public int Port { get; set; }

        public RegistrationWindow(IPAddress ipAddres, int port)
        {
            InitializeComponent();
            IpAddress = ipAddres;
            Port = port;
        }

        private async void RegistrationCommand(string command)
        {
            try
            {
                using (UdpClient client = new UdpClient())
                {
                    byte[] data = Encoding.Unicode.GetBytes(command);
                    IPEndPoint serverEndPoint = new IPEndPoint(IpAddress, Port);
                    await client.SendAsync(data, data.Length, serverEndPoint);

                    UdpReceiveResult result = await client.ReceiveAsync();
                    string msg = Encoding.Unicode.GetString(result.Buffer);

                    if (!string.IsNullOrEmpty(msg))
                    {
                        MessageBox.Show(msg);

                        if (msg == "Registration successful!")
                        {
                            this.Close();
                        }
                    }
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Socket Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string fullName = FullNameTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordTextBox.Password;

            // Перевірка валідності введених даних
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Будь ласка, заповніть усі поля.");
                return;
            }

            // Перевірка правильності формату електронної пошти
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Будь ласка, введіть правильну електронну адресу.");
                return;
            }

            // Виклик функції реєстрації з введеними даними
            RegistrationCommand($"registration:{fullName}:{email}:{password}");

            // Очистка полів вводу після реєстрації
            PanelClear();
        }

        private void PanelClear()
        {
            FullNameTextBox.Text = string.Empty;
            EmailTextBox.Text = string.Empty;
            PasswordTextBox.Password = string.Empty;
        }

        // Перевірка правильності формату електронної пошти
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}

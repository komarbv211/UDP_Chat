using System;
using System.Net;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UDP_ChatUser
{
    public partial class MainWindow : Window
    {
        public IPAddress? IpAddress { get; set; }
        public int Port { get; set; }

        public UdpClient client = new UdpClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IpTextBox.Text == "")
                IpTextBox.Text = "127.0.0.1";
            if (PortTextBox.Text == "")
                PortTextBox.Text = "1234";

            if (!IPAddress.TryParse(IpTextBox.Text, out IPAddress ipAddress))
            {
                MessageBox.Show("Invalid IPv4 address");
                return;
            }
            IpAddress = ipAddress;

            if (!int.TryParse(PortTextBox.Text.Trim(), out int port))
            {
                MessageBox.Show("Invalid port number");
                return;
            }
            Port = port;

            if (Port <= 0 || Port > 65535)
            {
                MessageBox.Show("Port must be between 0 - 65535");
                return;
            }
            Listen();
        }

        public async void Listen()
        {
            try
            {
                // Налаштування підключення до вказаної IP-адреси та порту
                client.Connect(IpAddress, Port);

                // Підтвердження підключення серверу
                byte[] confirmationMessage = Encoding.Unicode.GetBytes("<join>");
                client.Send(confirmationMessage, confirmationMessage.Length);

                // Очікування відповіді від сервера
                var result = await client.ReceiveAsync();
                string msg = Encoding.Unicode.GetString(result.Buffer);

                if (msg != "<join>")
                {
                    // Якщо не отримано підтвердження, виводимо повідомлення про невдале підключення
                    MessageBox.Show("Connection failed: Did not receive confirmation from the server.");
                    return;
                }

                this.IsEnabled = false;
                this.Visibility = Visibility.Collapsed;
                // Показ вікна верифікації
                VerificationWindow verificationWindow = new VerificationWindow(IpAddress, Port);
                verificationWindow.ShowDialog();
                this.Close();
            }
            catch (Exception ex)
            {
                // Обробка винятків
                MessageBox.Show("Error receiving message: " + ex.Message);
            }
        }
    }
}

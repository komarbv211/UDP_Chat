using System;
using System.Net;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;

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
            Port = 1234;
            DownloadFromFile();
        }
        private void DownloadFromFile()
        {
            string filePath = "IpAddress.txt";
            if (File.Exists(filePath) && new FileInfo(filePath).Length > 0)
            {
                try
                {
                    using (StreamReader reader = new StreamReader(filePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            IpComboBox.Items.Add(line);
                        }
                    }
                    // IpComboBox.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading IP addresses from file: {ex.Message}");
                }
            }
            else
            {
                IpComboBox.Items.Add("127.0.0.1");
                //IpComboBox.SelectedIndex = 0;
            }

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (IpComboBox.Text == "")
                IpComboBox.Text = "127.0.0.1";

            string ipAddressString = IpComboBox.Text;

            if (!IPAddress.TryParse(ipAddressString, out IPAddress ipAddress))
            {
                MessageBox.Show("Invalid IPv4 address");
                return;
            }

            // Встановлення значення для IpAddress
            IpAddress = ipAddress;

            string filePath = "IpAddress.txt";
            try
            {
                using (var writer = new System.IO.StreamWriter(filePath))
                {
                    writer.WriteLine(ipAddressString);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving IP address: {ex.Message}");
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

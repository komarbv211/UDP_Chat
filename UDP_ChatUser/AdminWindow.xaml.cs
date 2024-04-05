using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;

namespace UDP_ChatUser
{
    public partial class AdminWindow : Window
    {
        public IPAddress? IpAddress { get; set; }
        public int Port { get; set; }
        public int Id { get; set; }

        private bool isListening = false;

        IPEndPoint remoteEndPoint;

        UdpClient client = new UdpClient();

        public AdminWindow(int id, IPAddress ipAddres, int port)
        {
            InitializeComponent();
            Id = id;
            IpAddress = ipAddres;
            Port = port;
            remoteEndPoint = new IPEndPoint(ipAddres, port);
        }

        private async Task<string> SendAdminCommand(string command)
        {
            try
            {
                using (UdpClient client = new UdpClient())
                {
                    byte[] data = Encoding.Unicode.GetBytes(command);
                    IPEndPoint serverEndPoint = new IPEndPoint(IpAddress, Port);
                    await client.SendAsync(data, data.Length, serverEndPoint);

                    var result = await client.ReceiveAsync();
                    return Encoding.Unicode.GetString(result.Buffer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return null;
            }
        }
        private async void ListUserButton_Click(object sender, RoutedEventArgs e)
        {
            string response = await SendAdminCommand("adminAction:GetListAllUsers:");
            if (!string.IsNullOrEmpty(response))
            {
                UserDataListBox_Admin.Items.Add(response);
            }
        }
        private async void SearchByIdButton_Click_Admin(object sender, RoutedEventArgs e)
        {
            string userId = SearchIdTextBox_Admin.Text.Trim();
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out _))
            {
                string response = await SendAdminCommand("adminAction:searchForUserMessages:" + userId);
                if (!string.IsNullOrEmpty(response))
                {
                    UserDataListBox_Admin.Items.Add(response);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid user ID.");
            }
        }
        private async void GrantAdminRightsButton_Click(object sender, RoutedEventArgs e)
        {
            string userId = SearchIdTextBox_Admin.Text.Trim();
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out _))
            {
                string response = await SendAdminCommand("adminAction:grantAdmin:" + userId);
                if (!string.IsNullOrEmpty(response))
                {
                    UserDataListBox_Admin.Items.Add(response);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid user ID.");
            }
        }
        private async void RemoveAdministratorRights_Button_Click(object sender, RoutedEventArgs e)
        {
            string userId = SearchIdTextBox_Admin.Text.Trim();
            if (!string.IsNullOrEmpty(userId) && int.TryParse(userId, out _))
            {
                string response = await SendAdminCommand("adminAction:removeAdmin:" + userId);
                if (!string.IsNullOrEmpty(response))
                {
                    UserDataListBox_Admin.Items.Add(response);
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid user ID.");
            }
        }
        private async void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            await SendAdminCommand("adminAction:deleteUser:" + SearchIdTextBox_Admin.Text);
        }
        private void ClearAdminPanelButton_Click(object sender, RoutedEventArgs e)
        {
            SearchIdTextBox_Admin.Clear();
            UserDataListBox_Admin.Items.Clear();
        }
        private void JoinButtonClick(object sender, RoutedEventArgs e)
        {
            SendMessage("<join>");
            isListening = true;
            Listen();
        }
        private void LeaveButtonClick(object sender, RoutedEventArgs e)
        {
            SendMessage("<leave>");
            isListening = false;
        }
        private void SendMessageButtonClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBox.Text))
            {
                MessageBox.Show("Enter a message!");
                return;
            }
            if (!isListening)
            {
                MessageBox.Show("You are disconnected from the chat.");
                txtBox.Clear();
                return;
            }
            else
            {
                SendMessage(txtBox.Text);
                SendMessageDataBase(txtBox.Text);
            }
        }
        private async void Listen()
        {
            while (isListening)
            {
                var result = await client.ReceiveAsync();
                string msg = Encoding.Unicode.GetString(result.Buffer);
                list.Items.Add(msg);
                txtBox.Clear();
            }
        }
        private void SendMessage(string msg)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(msg);
            client.Send(bytes, bytes.Length, remoteEndPoint);
        }
        private void SendMessageDataBase(string msg)
        {
            string response = "<message>:" + msg + ":" + Id.ToString();

            byte[] bytes = Encoding.Unicode.GetBytes(response);
            client.Send(bytes, bytes.Length, remoteEndPoint);
        }

    }
}

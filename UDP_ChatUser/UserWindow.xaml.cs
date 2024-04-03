using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Windows;
using System.Threading.Tasks;

namespace UDP_ChatUser
{
    public partial class UserWindow : Window
    {
        public IPAddress? IpAddress { get; set; }
        public int Port { get; set; }
        public int Id { get; set; }

        IPEndPoint remoteEndPoint;

        UdpClient client = new UdpClient();

        private bool isListening = true;

        public UserWindow(int id, IPAddress ipAddress, int port)
        {
            InitializeComponent();
            remoteEndPoint = new IPEndPoint(ipAddress, port);
            Id = id;
        }
        private async void Listen()
        {
            while (isListening)
            {
                var result = await client.ReceiveAsync();
                string msg = Encoding.Unicode.GetString(result.Buffer);
                list.Items.Add(msg);
            }
        }

        public void SendMessageBtnClick(object sender, RoutedEventArgs e)
        {
            // validate message
            if (string.IsNullOrWhiteSpace(txtBox.Text))
            {
                MessageBox.Show("Enter a message!");
                return;
            }
            SendMessage(txtBox.Text);
            SendMessageDataBase(txtBox.Text);
        }
        public void JoinBtnClick(object sender, RoutedEventArgs e)
        {
            SendMessage("<join>");
            isListening = true;
            Listen();
        }
        public void LeaveBtnClick(object sender, RoutedEventArgs e)
        {
            SendMessage("<leave>");
            isListening = false;
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

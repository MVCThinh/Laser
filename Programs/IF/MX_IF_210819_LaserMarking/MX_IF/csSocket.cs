using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace MX_IF
{

    public class csSocketServer
    {
        Socket sServer;
        Socket sAccept;
        Task task;
        string ip;
        int port;
        public bool connectState = false;
        public bool canSend = false;
        System.Timers.Timer reconnectTimer = new System.Timers.Timer();
        public delegate void SocketEventHandler(object sender, DataEventArgs data);
        public event SocketEventHandler RcvData;

        public csSocketServer() { }
        public csSocketServer(string ip, int port) //ip랑 port받는 생성자.
        {
            this.ip = ip;
            this.port = port;
        }
        private void ReceiveData(string data)
        {
            if (this.RcvData != null) //이벤트 가입자 있는지 확인.
            {
                DataEventArgs dArgs = new DataEventArgs(data);
                RcvData(this, dArgs);
            }
        }
        public void ConnectServer(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            reconnectTimer.Elapsed += OnTimer;
            reconnectTimer.Interval = 3000;
            reconnectTimer.Start();
            Connect(ip, port);
        }
        private void Connect(string ip, int port)
        {
            sServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sServer.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            sServer.Listen(0);
            task = new Task(() => ReadData());
            task.Start();
        }
        public void ReadData()
        {
            reconnectTimer.Stop();
            connectState = true;
            sAccept = sServer.Accept();
            sServer.Close();
            canSend = true;
            while (connectState)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int rec = sAccept.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    if (rec <= 0)
                    {
                        throw new SocketException();
                    }
                    Array.Resize(ref buffer, rec);
                    string rcvdata = Encoding.UTF8.GetString(buffer);
                    ReceiveData(rcvdata);
                }
                catch
                {
                    sAccept.Close();
                    connectState = false;
                    canSend = false;
                    reconnectTimer.Start();
                    break;
                }
            }
        }
        public void SendData(string Data)
        {
            if (canSend) //try catch불가하여 canSend보고 메세지 보내야함.
            {
                byte[] data = Encoding.UTF8.GetBytes(Data);
                sAccept.Send(data, 0, data.Length, 0); //Send가 실패하면 프로그램 뻗음.
            }
            else
            {
            }

        }
        public void Close()
        {
            try
            {
                reconnectTimer.Stop();
                connectState = false;
                sAccept.Close();
                sServer.Close();
            }
            catch { }
        }
        private void OnTimer(object sender, EventArgs e)
        {
            if (!connectState)
            {
                Connect(ip, port);
            }
        }
    }

    public class csSocketClient
    {
        Socket sClient;
        Task task;
        string ip;
        int port;
        public bool connectState = false;
        System.Timers.Timer reconnectTimer = new System.Timers.Timer();
        public delegate void SocketEventHandler(object sender, DataEventArgs data);
        public event SocketEventHandler RcvData;

        public csSocketClient() { }
        public csSocketClient(string ip, int port) //ip랑 port받는 생성자.
        {
            this.ip = ip;
            this.port = port;
        }
        private void ReceiveData(string data)
        {
            if (this.RcvData != null) //이벤트 가입자 있는지 확인.
            {
                DataEventArgs dArgs = new DataEventArgs(data);
                RcvData(this, dArgs);
            }
        }
        public void ConnectClient(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
            reconnectTimer.Elapsed += OnTimer;
            reconnectTimer.Interval = 3000;
            reconnectTimer.Start();
            Connect(ip, port);
        }
        private void Connect(string ip, int port)
        {
            try
            {
                sClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sClient.Connect(new IPEndPoint(IPAddress.Parse(ip), port)); //클라이언트는 여기서 접속이 안되면 catch로 튕겨버림.
                connectState = true;
                reconnectTimer.Stop();
                task = new Task(() => ReadData());
                task.Start();
            }
            catch
            {
                sClient.Close();
                reconnectTimer.Start();
            }
        }
        public void ReadData()
        {
            while (connectState)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int rec = sClient.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                    if (rec <= 0)
                    {
                        throw new SocketException();
                    }
                    Array.Resize(ref buffer, rec);
                    string rcvdata = Encoding.UTF8.GetString(buffer);
                    ReceiveData(rcvdata);
                }
                catch
                {
                    connectState = false;
                    sClient.Close();
                    reconnectTimer.Start();
                    break;
                }
            }
        }
        public void SendData(string Data)
        {
            try
            {
                if (connectState)
                {
                    byte[] data = Encoding.UTF8.GetBytes(Data + "\r\n");
                    sClient.Send(data, 0, data.Length, SocketFlags.None);
                }
                else
                {
                    //CONST.LOG.ExceptionLogSave("Client SendData : " + Data); //실패시 로그 남기기위함.
                    //CONST.LOG.Save(csLog.LogKind.Socket, "Client SendData Fail: " + Data);
                }
            }
            catch
            {
                connectState = false;
                sClient.Close();
                reconnectTimer.Start();
            }
        }
        public void SendDataByte(Byte[] Data)
        {
            try
            {
                if (connectState)
                {
                    //byte[] data = Encoding.ASCII.GetBytes(Data);
                    sClient.Send(Data, 0, Data.Length, SocketFlags.None);
                }
                else
                {
                    StringBuilder log = new StringBuilder();
                    for (int i = 0; i < Data.Length; i++)
                    {
                        log.Append(Data[i] + " ");
                    }
                    //CONST.LOG.ExceptionLogSave("Client SendDataByte : " + log.ToString()); //실패시 로그 남기기위함.
                    //CONST.LOG.Save(csLog.LogKind.Socket, "Client SendDataByte Fail: " + log.ToString());
                }
            }
            catch
            {
                connectState = false;
                sClient.Close();
                reconnectTimer.Start();
            }
        }
        public void Close()
        {
            try
            {
                reconnectTimer.Stop();
                sClient.Close();
            }
            catch { }
        }
        private void OnTimer(object sender, EventArgs e)
        {
            if (!connectState)
            {
                Connect(ip, port);
            }
        }
    }

    public class DataEventArgs : EventArgs //sender가 아닌 e에 data를 보내기 위한 클래스.
    {
        public string data;
        public DataEventArgs(string _data)
        {
            this.data = _data;
        }
    }

}

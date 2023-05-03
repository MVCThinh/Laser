using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;

public class csAccura
{
    //public const string cUPSIP = "192.100.100.201";
    //public const string cGPSIP = "192.100.100.200";
    public string cUPSIP;
    public string cGPSIP;
    public const int cPort = 502;
    public const int cUPS = 0;
    public const int cGPS = 1;

    public const byte cUPSUnitID = 0x01;
    public const byte cGPSUnitID = 0x00;
    public const byte cFunctionCode = 0x03;
    public const byte cProtocalID_Upper = 0x00;
    public const byte cProtocalID_Lower = 0x00;
    public const byte cDataLength_Upper = 0x00;
    public const byte cDataLength_Lower = 0x06;

    //11101 Voltage Van Float32 PR A상의 상전압. 단위 [V] (2Word)
    //11103 Voltage Vbn Float32 PR B상의 상전압. 단위 [V] (2Word)
    //11105 Voltage Vcn Float32 PR C상의 상전압. 단위 [V] (2Word)
    //11107 Voltage Vavg_ln Float32 PR 삼상의 상전압 평균. 단위 [V] (2Word)
    //11109 Voltage Vab Float32 PR AB상의 선간전압. 단위 [V] (2Word)
    //11111 Voltage Vbc Float32 PR BC상의 선간전압. 단위 [V] (2Word)
    //11113 Voltage Vca Float32 PR CA상의 선간전압. 단위 [V] (2Word)
    //11115 Voltage Vavg_ll Float32 PR 삼상의 선간전압 평균. 단위 [V] (2Word)

    public const byte cVoltage_Request_UpperAddress = 0x2b;
    public const byte cVoltage_Request_LowerAddress = 0x5c;
    public const byte cVoltage_Request_UpperLength = 0x00;
    public const byte cVoltage_Request_LowerLength = 0x10;

    //초기 주소 11201 간격 150Word
    //60~61 Active Power Pa Float32 PR A상의 유효전력. 단위 [kW] (2Word)
    //62~63 Active Power Pb Float32 PR B상의 유효전력. 단위 [kW] (2Word)
    //64~65 Active Power Pc Float32 PR C상의 유효전력. 단위 [kW] (2Word)
    //66~67 Active Power Ptot Float32 PR 삼상의 유효전력 총합. 단위 [kW] (2Word)

    public const byte cGPSPower_Request_UpperAddress = 0x2b;
    public const byte cGPSPower_Request_LowerAddress = 0xfc;
    public const byte cGPSPower_Request_UpperLength = 0x00;
    //public const byte cGPSPower_Request_LowerLength = 0x08;
    public const byte cGPSPower_Request_LowerLength = 0x22;

    //public const byte cUPSPower_Request_UpperAddress = 0x2c;
    //public const byte cUPSPower_Request_LowerAddress = 0x92;
    //public const byte cUPSPower_Request_UpperLength = 0x00;
    ////public const byte cUPSPower_Request_LowerLength = 0x08;
    //public const byte cUPSPower_Request_LowerLength = 0x22;

    public const byte cUPSPower_Request_UpperAddress = 0x2b;
    public const byte cUPSPower_Request_LowerAddress = 0xfc;
    public const byte cUPSPower_Request_UpperLength = 0x00;
    public const byte cUPSPower_Request_LowerLength = 0x22;  //210128 cjm 0x08 -> 0x22 바꿈 : 데이터 값 25개 -> 75개 읽음

    //초기 주소 11201 간격 150Word
    //0~1 Current Ia Float32 PR A상 전류. 단위 [A] (2Word)
    //2~3 Current Ib Float32 PR B상 전류. 단위 [A] (2Word)
    //4~5 Current Ic Float32 PR C상 전류. 단위 [A] (2Word)
    //6~7 Current Iavg Float32 PR 삼상 전류 평균. 단위 [A] (2Word)

    public const byte cGPSCurrent_Request_UpperAddress = 0x2b;
    public const byte cGPSCurrent_Request_LowerAddress = 0xc0;
    public const byte cGPSCurrent_Request_UpperLength = 0x00;
    public const byte cGPSCurrent_Request_LowerLength = 0x08;

    //public const byte cUPSCurrent_Request_UpperAddress = 0x2c;
    //public const byte cUPSCurrent_Request_LowerAddress = 0x56;

    //public const byte cUPSCurrent_Request_UpperLength = 0x00;
    //public const byte cUPSCurrent_Request_LowerLength = 0x08;

    public const byte cUPSCurrent_Request_UpperAddress = 0x2b;
    public const byte cUPSCurrent_Request_LowerAddress = 0xc0;

    public const byte cUPSCurrent_Request_UpperLength = 0x00;
    public const byte cUPSCurrent_Request_LowerLength = 0x08;

    //사용자 정의(적산량 Address)
    //32001번지 부터 시작
    public const byte cUserDefine_Request_UpperAddress = 0x7d;
    public const byte cUserDefine_Request_LowerAddress = 0x00;

    public const byte cUserDefine_Request_UpperLength = 0x00;
    public const byte cUserDefine_Request_LowerLength = 0x64;

    //입력 전압 주파수 확인
    public const byte cFrequency_Request_UpperAddress = 0x2B;
    public const byte cFrequency_Request_LowerAddress = 0x8E;

    public const byte cFrequency_Request_UpperLength = 0x00;
    public const byte cFrequency_Request_LowerLength = 0x02;

    public TcpClient[] tcpClient = new TcpClient[2];
    public TcpListener[] tcpListener = new TcpListener[2];

    public StreamReader[] streamReader = new StreamReader[2];
    public StreamWriter[] streamWriter = new StreamWriter[2];
    public NetworkStream[] networkStream = new NetworkStream[2];

    public int iSendUPS = 0;
    public int iSendGPS = 0;

    public float[] getUPSVoltageValue = null;
    public float[] getUPSPowerValue = null;
    public float[] getUPSCurrentValue = null;
    public double[] getUPSUserDefineValue = null;

    public float[] getGPSVoltageValue = null;
    public float[] getGPSPowerValue = null;
    public float[] getGPSCurrentValue = null;

    public bool bCheckUPS = false;
    public bool bCheckGPS = false;


    public csAccura(string _UPSIP, string _GPSIP)
    {
        iSendUPS = 0;
        iSendGPS = 0;

        cUPSIP = _UPSIP;
        cGPSIP = _GPSIP;

        getUPSVoltageValue = new float[8];
        getUPSPowerValue = new float[4];
        //getUPSPowerValue = new float[15];

        getUPSCurrentValue = new float[4];
        getUPSUserDefineValue = new double[10];

        getGPSVoltageValue = new float[8];
        //getGPSPowerValue = new float[4];
        getGPSPowerValue = new float[15];
        getGPSCurrentValue = new float[4];

        if (_GPSIP != "")
        {
            if (run_GPSPingTest())
                bCheckGPS = init_GPSConnect();
        }
        if (_UPSIP != "")
        {
            if (run_UPSPingTest())
                bCheckUPS = init_UPSConnect();
        }
    }

    public bool run_GPSPingTest()
    {
        bool lcRet = false;

        Ping lcPing = new Ping();
        PingOptions lcPingOptions = new PingOptions();
        lcPingOptions.DontFragment = true;
        string lcSend = "aaaaaa";
        byte[] lcBuffer = ASCIIEncoding.ASCII.GetBytes(lcSend);
        int lcTimeOut = 120;
        PingReply lcPingReply = lcPing.Send(IPAddress.Parse(cGPSIP), lcTimeOut, lcBuffer, lcPingOptions);

        if (lcPingReply.Status == IPStatus.Success)
            lcRet = true;
        else
            lcRet = false;

        return lcRet;
    }

    public bool run_UPSPingTest()
    {
        bool lcRet = false;

        Ping lcPing = new Ping();
        PingOptions lcPingOptions = new PingOptions();
        lcPingOptions.DontFragment = true;
        string lcSend = "aaaaaa";
        byte[] lcBuffer = ASCIIEncoding.ASCII.GetBytes(lcSend);
        int lcTimeOut = 1200;
        PingReply lcPingReply = lcPing.Send(IPAddress.Parse(cUPSIP), lcTimeOut, lcBuffer, lcPingOptions);

        if (lcPingReply.Status == IPStatus.Success)
            lcRet = true;
        else
            lcRet = false;

        return lcRet;
    }

    public bool init_UPSConnect()
    {
        try
        {
            tcpClient[cUPS] = new TcpClient();
            tcpClient[cUPS].Connect(cUPSIP, cPort);
            networkStream[cUPS] = tcpClient[cUPS].GetStream();
            streamReader[cUPS] = new StreamReader(networkStream[cUPS]);
            streamWriter[cUPS] = new StreamWriter(networkStream[cUPS]);

            return tcpClient[cUPS].Connected;

        }
        catch
        {
            return false;
        }
    }

    public bool init_GPSConnect()
    {
        try
        {
            tcpClient[cGPS] = new TcpClient();
            tcpClient[cGPS].Connect(cGPSIP, cPort);
            networkStream[cGPS] = tcpClient[cGPS].GetStream();
            streamReader[cGPS] = new StreamReader(networkStream[cGPS]);
            streamWriter[cGPS] = new StreamWriter(networkStream[cGPS]);

            return tcpClient[cGPS].Connected;
        }
        catch
        {
            return false;
        }
    }

    public void rst_UPSConnect()
    {
        tcpClient[cUPS].Close();
    }

    public void rst_GPSConnect()
    {
        
        tcpClient[cGPS].Close();
    }

    #region //UPS 항목
    public float get_UPSFrequency(out bool lcResult)  //진동수
    {
        float lcRetFrequency = 0.0F;

        try
        {
            if (iSendUPS > 0xFFFF)
                iSendUPS = 0;

            byte[] lcSendTransaction = BitConverter.GetBytes(iSendUPS);

            byte[] lcSend = new byte[12];

            lcSend[0] = lcSendTransaction[1];
            lcSend[1] = lcSendTransaction[0];

            lcSend[2] = cProtocalID_Upper;
            lcSend[3] = cProtocalID_Lower;

            lcSend[4] = cDataLength_Upper;
            lcSend[5] = cDataLength_Lower;

            lcSend[6] = cUPSUnitID;

            lcSend[7] = cFunctionCode;

            lcSend[8] = cFrequency_Request_UpperAddress;
            lcSend[9] = cFrequency_Request_LowerAddress;

            lcSend[10] = cFrequency_Request_UpperLength;
            lcSend[11] = cFrequency_Request_LowerLength;

            iSendUPS++;

            networkStream[cUPS].Write(lcSend, 0, lcSend.Length);

            byte[] lcGetData = new byte[1024];
            int lcCount = 0;

            if (networkStream[cUPS].CanRead)
            {


                lcCount = networkStream[cUPS].Read(lcGetData, 0, lcGetData.Length);

                if (lcSend[0] == lcGetData[0] && lcSend[1] == lcGetData[1] && lcSend[7] == lcGetData[7])
                {


                    byte[] lcGet = new byte[lcCount];
                    string[] lcConvert = new string[lcCount];

                    Array.Copy(lcGetData, lcGet, lcCount);

                    for (int i = 0; i < lcCount; i++)
                        lcConvert[i] = Convert.ToString(lcGet[i], 16).PadLeft(2, '0');

                    string[] lcFormatString = new string[1];
                    int[] lcFormatInt = new int[1];

                    lcFormatString[0] = string.Format("0x{0}{1}{2}{3}", lcConvert[9], lcConvert[10], lcConvert[11], lcConvert[12]);
                    //lcFormatString[1] = string.Format("0x{0}{1}{2}{3}", lcConvert[13], lcConvert[14], lcConvert[15], lcConvert[16]);
                    //lcFormatString[2] = string.Format("0x{0}{1}{2}{3}", lcConvert[17], lcConvert[18], lcConvert[19], lcConvert[20]);
                    //lcFormatString[3] = string.Format("0x{0}{1}{2}{3}", lcConvert[21], lcConvert[22], lcConvert[23], lcConvert[24]);
                    //lcFormatString[4] = string.Format("0x{0}{1}{2}{3}", lcConvert[25], lcConvert[26], lcConvert[27], lcConvert[28]);
                    //lcFormatString[5] = string.Format("0x{0}{1}{2}{3}", lcConvert[29], lcConvert[30], lcConvert[31], lcConvert[32]);
                    //lcFormatString[6] = string.Format("0x{0}{1}{2}{3}", lcConvert[33], lcConvert[34], lcConvert[35], lcConvert[36]);
                    //lcFormatString[7] = string.Format("0x{0}{1}{2}{3}", lcConvert[37], lcConvert[38], lcConvert[39], lcConvert[40]);

                    lcFormatInt[0] = Convert.ToInt32(lcFormatString[0], 16);
                    //lcFormatInt[1] = Convert.ToInt32(lcFormatString[1], 16);
                    //lcFormatInt[2] = Convert.ToInt32(lcFormatString[2], 16);
                    //lcFormatInt[3] = Convert.ToInt32(lcFormatString[3], 16);
                    //lcFormatInt[4] = Convert.ToInt32(lcFormatString[4], 16);
                    //lcFormatInt[5] = Convert.ToInt32(lcFormatString[5], 16);
                    //lcFormatInt[6] = Convert.ToInt32(lcFormatString[6], 16);
                    //lcFormatInt[7] = Convert.ToInt32(lcFormatString[7], 16);

                    float[] lcGetResult = new float[1];

                    for (int i = 0; i < lcFormatInt.Length; i++)
                    {
                        byte[] lcByte = BitConverter.GetBytes(lcFormatInt[i]);
                        lcGetResult[i] = BitConverter.ToSingle(lcByte, 0);
                    }

                    for (int i = 0; i < getUPSVoltageValue.Length; i++)
                    {
                        lcRetFrequency = lcGetResult[i];
                    }
                }
                lcResult = true;
            }
            else
                lcResult = false;


            return lcRetFrequency;

        }
        catch
        {
            rst_UPSConnect();
            init_UPSConnect();

            lcResult = false;

            return lcRetFrequency;

        }
    }

    public float[] get_UPSVoltage(out bool lcResult) //전압
    {
        try
        {
            if (iSendUPS > 0xFFFF)
                iSendUPS = 0;

            byte[] lcSendTransaction = BitConverter.GetBytes(iSendUPS);

            byte[] lcSend = new byte[12];

            lcSend[0] = lcSendTransaction[1];
            lcSend[1] = lcSendTransaction[0];

            lcSend[2] = cProtocalID_Upper;
            lcSend[3] = cProtocalID_Lower;

            lcSend[4] = cDataLength_Upper;
            lcSend[5] = cDataLength_Lower;

            lcSend[6] = cUPSUnitID;

            lcSend[7] = cFunctionCode;

            lcSend[8] = cVoltage_Request_UpperAddress;
            lcSend[9] = cVoltage_Request_LowerAddress;

            lcSend[10] = cVoltage_Request_UpperLength;
            lcSend[11] = cVoltage_Request_LowerLength;

            iSendUPS++;

            networkStream[cUPS].Write(lcSend, 0, lcSend.Length);

            byte[] lcGetData = new byte[1024];
            int lcCount = 0;

            if (networkStream[cUPS].CanRead)
            {


                lcCount = networkStream[cUPS].Read(lcGetData, 0, lcGetData.Length);

                if (lcSend[0] == lcGetData[0] && lcSend[1] == lcGetData[1] && lcSend[7] == lcGetData[7])
                {


                    byte[] lcGet = new byte[lcCount];
                    string[] lcConvert = new string[lcCount];

                    Array.Copy(lcGetData, lcGet, lcCount);

                    for (int i = 0; i < lcCount; i++)
                        lcConvert[i] = Convert.ToString(lcGet[i], 16).PadLeft(2, '0');

                    string[] lcFormatString = new string[8];
                    int[] lcFormatInt = new int[8];

                    lcFormatString[0] = string.Format("0x{0}{1}{2}{3}", lcConvert[9], lcConvert[10], lcConvert[11], lcConvert[12]);  //A상 전압
                    lcFormatString[1] = string.Format("0x{0}{1}{2}{3}", lcConvert[13], lcConvert[14], lcConvert[15], lcConvert[16]); //B상 전압
                    lcFormatString[2] = string.Format("0x{0}{1}{2}{3}", lcConvert[17], lcConvert[18], lcConvert[19], lcConvert[20]); //C상 전압
                    lcFormatString[3] = string.Format("0x{0}{1}{2}{3}", lcConvert[21], lcConvert[22], lcConvert[23], lcConvert[24]); //평균 전압
                    lcFormatString[4] = string.Format("0x{0}{1}{2}{3}", lcConvert[25], lcConvert[26], lcConvert[27], lcConvert[28]);
                    lcFormatString[5] = string.Format("0x{0}{1}{2}{3}", lcConvert[29], lcConvert[30], lcConvert[31], lcConvert[32]);
                    lcFormatString[6] = string.Format("0x{0}{1}{2}{3}", lcConvert[33], lcConvert[34], lcConvert[35], lcConvert[36]);
                    lcFormatString[7] = string.Format("0x{0}{1}{2}{3}", lcConvert[37], lcConvert[38], lcConvert[39], lcConvert[40]);

                    lcFormatInt[0] = Convert.ToInt32(lcFormatString[0], 16);
                    lcFormatInt[1] = Convert.ToInt32(lcFormatString[1], 16);
                    lcFormatInt[2] = Convert.ToInt32(lcFormatString[2], 16);
                    lcFormatInt[3] = Convert.ToInt32(lcFormatString[3], 16);
                    lcFormatInt[4] = Convert.ToInt32(lcFormatString[4], 16);
                    lcFormatInt[5] = Convert.ToInt32(lcFormatString[5], 16);
                    lcFormatInt[6] = Convert.ToInt32(lcFormatString[6], 16);
                    lcFormatInt[7] = Convert.ToInt32(lcFormatString[7], 16);

                    float[] lcGetResult = new float[8];

                    for (int i = 0; i < lcFormatInt.Length; i++)
                    {
                        byte[] lcByte = BitConverter.GetBytes(lcFormatInt[i]);
                        lcGetResult[i] = BitConverter.ToSingle(lcByte, 0);
                    }

                    for (int i = 0; i < getUPSVoltageValue.Length; i++)
                        getUPSVoltageValue[i] = lcGetResult[i];
                }
                lcResult = true;
            }
            else
                lcResult = false;



            return getUPSVoltageValue;
        }
        catch
        {
            rst_UPSConnect();
            init_UPSConnect();

            lcResult = false;
            return getUPSVoltageValue;
        }
    }

    public void get_UPSPower(out bool lcResult, out float[] lcReturn01, out int[] lcReturn02)
    {
        float[] lcGetResult = new float[13];
        //float[] lcGetResult = new float[4];
        int[] lcGetResult01 = new int[4];

        try
        {
            if (iSendUPS > 0xFFFF)
                iSendUPS = 0;

            byte[] lcSendTransaction = BitConverter.GetBytes(iSendUPS);

            byte[] lcSend = new byte[12];

            lcSend[0] = lcSendTransaction[1];
            lcSend[1] = lcSendTransaction[0];

            lcSend[2] = cProtocalID_Upper;
            lcSend[3] = cProtocalID_Lower;

            lcSend[4] = cDataLength_Upper;
            lcSend[5] = cDataLength_Lower;

            lcSend[6] = cUPSUnitID;

            lcSend[7] = cFunctionCode;

            lcSend[8] = cUPSPower_Request_UpperAddress;
            lcSend[9] = cUPSPower_Request_LowerAddress;

            lcSend[10] = cUPSPower_Request_UpperLength;
            lcSend[11] = cUPSPower_Request_LowerLength;

            iSendUPS++;

            networkStream[cUPS].Write(lcSend, 0, lcSend.Length);

            byte[] lcGetData = new byte[1024];
            int lcCount = 0;

            if (networkStream[cUPS].CanRead)
            {
                lcCount = networkStream[cUPS].Read(lcGetData, 0, lcGetData.Length);

                if (lcSend[0] == lcGetData[0] && lcSend[1] == lcGetData[1] && lcSend[7] == lcGetData[7])
                {


                    byte[] lcGet = new byte[lcCount];
                    string[] lcConvert = new string[lcCount];

                    Array.Copy(lcGetData, lcGet, lcCount);

                    for (int i = 0; i < lcCount; i++)
                        lcConvert[i] = Convert.ToString(lcGet[i], 16).PadLeft(2, '0');

                    string[] lcFormatString = new string[17];
                    int[] lcFormatInt = new int[17];

                    //string[] lcFormatString = new string[4];
                    //int[] lcFormatInt = new int[4];

                    lcFormatString[0] = string.Format("0x{0}{1}{2}{3}", lcConvert[9], lcConvert[10], lcConvert[11], lcConvert[12]); //A상 유효 전력
                    lcFormatString[1] = string.Format("0x{0}{1}{2}{3}", lcConvert[13], lcConvert[14], lcConvert[15], lcConvert[16]); //B상 유효 전력
                    lcFormatString[2] = string.Format("0x{0}{1}{2}{3}", lcConvert[17], lcConvert[18], lcConvert[19], lcConvert[20]); //C상 유효 전력
                    lcFormatString[3] = string.Format("0x{0}{1}{2}{3}", lcConvert[21], lcConvert[22], lcConvert[23], lcConvert[24]); //삼상 유효 전력 총합
                    lcFormatString[4] = string.Format("0x{0}{1}{2}{3}", lcConvert[25], lcConvert[26], lcConvert[27], lcConvert[28]); //A상 무효 전력
                    lcFormatString[5] = string.Format("0x{0}{1}{2}{3}", lcConvert[29], lcConvert[30], lcConvert[31], lcConvert[32]); //B상 무효 전력
                    lcFormatString[6] = string.Format("0x{0}{1}{2}{3}", lcConvert[33], lcConvert[34], lcConvert[35], lcConvert[36]); //C상 무효 전력
                    lcFormatString[7] = string.Format("0x{0}{1}{2}{3}", lcConvert[37], lcConvert[38], lcConvert[39], lcConvert[40]); //삼상 무효 전력 총합
                    lcFormatString[8] = string.Format("0x{0}{1}{2}{3}", lcConvert[41], lcConvert[42], lcConvert[43], lcConvert[44]); //A상 피상 전력
                    lcFormatString[9] = string.Format("0x{0}{1}{2}{3}", lcConvert[45], lcConvert[46], lcConvert[47], lcConvert[48]); //B상 피상 전력
                    lcFormatString[10] = string.Format("0x{0}{1}{2}{3}", lcConvert[49], lcConvert[50], lcConvert[51], lcConvert[52]); //C상 피상 전력
                    lcFormatString[11] = string.Format("0x{0}{1}{2}{3}", lcConvert[53], lcConvert[54], lcConvert[55], lcConvert[56]); //삼상의 피상 전력 총합
                    lcFormatString[12] = string.Format("0x{0}{1}{2}{3}", lcConvert[57], lcConvert[58], lcConvert[59], lcConvert[60]); //ZCT 누설 전류 (미사용)
                    lcFormatString[13] = string.Format("0x{0}{1}{2}{3}", lcConvert[61], lcConvert[62], lcConvert[63], lcConvert[64]); //삼상 수전 유효 전력
                    lcFormatString[14] = string.Format("0x{0}{1}{2}{3}", lcConvert[65], lcConvert[66], lcConvert[67], lcConvert[68]); //삼상 송전 유효 전력(미사용)
                    lcFormatString[15] = string.Format("0x{0}{1}{2}{3}", lcConvert[69], lcConvert[70], lcConvert[71], lcConvert[72]); //삼상 송전 유효 전력합 -> (수전 + 송전) 
                    lcFormatString[16] = string.Format("0x{0}{1}{2}{3}", lcConvert[73], lcConvert[74], lcConvert[75], lcConvert[76]); //삼상 송전 유효 전력(미사용)



                    lcFormatInt[0] = Convert.ToInt32(lcFormatString[0], 16);
                    lcFormatInt[1] = Convert.ToInt32(lcFormatString[1], 16);
                    lcFormatInt[2] = Convert.ToInt32(lcFormatString[2], 16);
                    lcFormatInt[3] = Convert.ToInt32(lcFormatString[3], 16);
                    lcFormatInt[4] = Convert.ToInt32(lcFormatString[4], 16);
                    lcFormatInt[5] = Convert.ToInt32(lcFormatString[5], 16);
                    lcFormatInt[6] = Convert.ToInt32(lcFormatString[6], 16);
                    lcFormatInt[7] = Convert.ToInt32(lcFormatString[7], 16);
                    lcFormatInt[8] = Convert.ToInt32(lcFormatString[8], 16);
                    lcFormatInt[9] = Convert.ToInt32(lcFormatString[9], 16);
                    lcFormatInt[10] = Convert.ToInt32(lcFormatString[10], 16);
                    lcFormatInt[11] = Convert.ToInt32(lcFormatString[11], 16);
                    lcFormatInt[12] = Convert.ToInt32(lcFormatString[12], 16);
                    lcFormatInt[13] = Convert.ToInt32(lcFormatString[13], 16);
                    lcFormatInt[14] = Convert.ToInt32(lcFormatString[14], 16);
                    lcFormatInt[15] = Convert.ToInt32(lcFormatString[15], 16);
                    lcFormatInt[16] = Convert.ToInt32(lcFormatString[16], 16);






                    for (int i = 0; i < lcFormatInt.Length; i++)
                    {
                        byte[] lcByte = BitConverter.GetBytes(lcFormatInt[i]);

                        if (i < 13)
                            lcGetResult[i] = BitConverter.ToSingle(lcByte, 0);
                        else if (i == 13)
                            lcGetResult01[0] = BitConverter.ToInt32(lcByte, 0);
                        else if (i == 14)
                            lcGetResult01[1] = BitConverter.ToInt32(lcByte, 0);
                        else if (i == 15)
                            lcGetResult01[2] = BitConverter.ToInt32(lcByte, 0);
                        else if (i == 16)
                            lcGetResult01[3] = BitConverter.ToInt32(lcByte, 0);
                    }

                    //for (int i = 0; i < getUPSPowerValue.Length; i++)
                    //    getUPSPowerValue[i] = lcGetResult[i];
                }

                lcReturn01 = lcGetResult;
                lcReturn02 = lcGetResult01;

                lcResult = true;
            }
            else
            {
                lcReturn01 = lcGetResult;
                lcReturn02 = lcGetResult01;
                lcResult = false;
            }

           
        }
        catch
        {
            rst_UPSConnect();
            init_UPSConnect();

            lcReturn01 = lcGetResult;
            lcReturn02 = lcGetResult01;
            lcResult = false;
           
        }
    }

    public float[] get_UPSCurrent(out bool lcResult)
    {
        try
        {
            if (iSendUPS > 0xFFFF)
                iSendUPS = 0;

            byte[] lcSendTransaction = BitConverter.GetBytes(iSendUPS);

            byte[] lcSend = new byte[12];

            lcSend[0] = lcSendTransaction[1];
            lcSend[1] = lcSendTransaction[0];

            lcSend[2] = cProtocalID_Upper;
            lcSend[3] = cProtocalID_Lower;

            lcSend[4] = cDataLength_Upper;
            lcSend[5] = cDataLength_Lower;

            lcSend[6] = cUPSUnitID;

            lcSend[7] = cFunctionCode;

            lcSend[8] = cUPSCurrent_Request_UpperAddress;
            lcSend[9] = cUPSCurrent_Request_LowerAddress;

            lcSend[10] = cUPSCurrent_Request_UpperLength;
            lcSend[11] = cUPSCurrent_Request_LowerLength;

            iSendUPS++;

            networkStream[cUPS].Write(lcSend, 0, lcSend.Length);

            byte[] lcGetData = new byte[1024];
            int lcCount = 0;

            if (networkStream[cUPS].CanRead)
            {
                lcCount = networkStream[cUPS].Read(lcGetData, 0, lcGetData.Length);

                if (lcSend[0] == lcGetData[0] && lcSend[1] == lcGetData[1] && lcSend[7] == lcGetData[7])
                {
                    byte[] lcGet = new byte[lcCount];
                    string[] lcConvert = new string[lcCount];

                    Array.Copy(lcGetData, lcGet, lcCount);

                    for (int i = 0; i < lcCount; i++)
                        lcConvert[i] = Convert.ToString(lcGet[i], 16).PadLeft(2, '0');

                    string[] lcFormatString = new string[4];
                    int[] lcFormatInt = new int[4];

                    lcFormatString[0] = string.Format("0x{0}{1}{2}{3}", lcConvert[9], lcConvert[10], lcConvert[11], lcConvert[12]);
                    lcFormatString[1] = string.Format("0x{0}{1}{2}{3}", lcConvert[13], lcConvert[14], lcConvert[15], lcConvert[16]);
                    lcFormatString[2] = string.Format("0x{0}{1}{2}{3}", lcConvert[17], lcConvert[18], lcConvert[19], lcConvert[20]);
                    lcFormatString[3] = string.Format("0x{0}{1}{2}{3}", lcConvert[21], lcConvert[22], lcConvert[23], lcConvert[24]);


                    lcFormatInt[0] = Convert.ToInt32(lcFormatString[0], 16);
                    lcFormatInt[1] = Convert.ToInt32(lcFormatString[1], 16);
                    lcFormatInt[2] = Convert.ToInt32(lcFormatString[2], 16);
                    lcFormatInt[3] = Convert.ToInt32(lcFormatString[3], 16);


                    float[] lcGetResult = new float[4];

                    for (int i = 0; i < lcFormatInt.Length; i++)
                    {
                        byte[] lcByte = BitConverter.GetBytes(lcFormatInt[i]);
                        lcGetResult[i] = BitConverter.ToSingle(lcByte, 0);
                    }

                    for (int i = 0; i < getUPSCurrentValue.Length; i++)
                        getUPSCurrentValue[i] = lcGetResult[i];
                }

                lcResult = true;
            }
            else
                lcResult = false;

            return getUPSCurrentValue;
        }
        catch
        {
            rst_UPSConnect();
            init_UPSConnect();
            lcResult = false;
            return getUPSCurrentValue;
        }
    }

    public double[] get_UPSUserDefine(out bool lcResult)
    {
        try
        {
            if (iSendUPS > 0xFFFF)
                iSendUPS = 0;

            byte[] lcSendTransaction = BitConverter.GetBytes(iSendUPS);

            byte[] lcSend = new byte[12];

            lcSend[0] = lcSendTransaction[1];
            lcSend[1] = lcSendTransaction[0];

            lcSend[2] = cProtocalID_Upper;
            lcSend[3] = cProtocalID_Lower;

            lcSend[4] = cDataLength_Upper;
            lcSend[5] = cDataLength_Lower;

            lcSend[6] = cUPSUnitID;

            lcSend[7] = cFunctionCode;

            lcSend[8] = cUserDefine_Request_UpperAddress;
            lcSend[9] = cUserDefine_Request_LowerAddress;

            lcSend[10] = cUserDefine_Request_UpperLength;
            lcSend[11] = cUserDefine_Request_LowerLength;

            iSendUPS++;

            networkStream[cUPS].Write(lcSend, 0, lcSend.Length);

            byte[] lcGetData = new byte[2048];
            int lcCount = 0;

            if (networkStream[cUPS].CanRead)
            {
                lcCount = networkStream[cUPS].Read(lcGetData, 0, lcGetData.Length);

                if (lcSend[0] == lcGetData[0] && lcSend[1] == lcGetData[1] && lcSend[7] == lcGetData[7])
                {
                    byte[] lcGet = new byte[lcCount];
                    string[] lcConvert = new string[lcCount];

                    Array.Copy(lcGetData, lcGet, lcCount);

                    for (int i = 0; i < lcCount; i++)
                        lcConvert[i] = Convert.ToString(lcGet[i], 16).PadLeft(2, '0');


                    string[] lcFormatString = new string[10];

                    Int64[] lcFormatInt = new Int64[10];


                    lcFormatString[0] = string.Format("0x{7}{6}{5}{4}{3}{2}{1}{0}", lcConvert[28], lcConvert[27], lcConvert[26], lcConvert[25], lcConvert[24], lcConvert[23], lcConvert[22], lcConvert[21]);
                    lcFormatString[1] = string.Format("0x{7}{6}{5}{4}{3}{2}{1}{0}", lcConvert[38], lcConvert[37], lcConvert[36], lcConvert[35], lcConvert[34], lcConvert[33], lcConvert[32], lcConvert[31]);
                    lcFormatString[2] = string.Format("0x{7}{6}{5}{4}{3}{2}{1}{0}", lcConvert[48], lcConvert[47], lcConvert[46], lcConvert[45], lcConvert[44], lcConvert[43], lcConvert[42], lcConvert[41]);
                    lcFormatString[3] = string.Format("0x{7}{6}{5}{4}{3}{2}{1}{0}", lcConvert[58], lcConvert[57], lcConvert[56], lcConvert[55], lcConvert[54], lcConvert[53], lcConvert[52], lcConvert[51]);
                    lcFormatString[4] = string.Format("0x{7}{6}{5}{4}{3}{2}{1}{0}", lcConvert[68], lcConvert[67], lcConvert[66], lcConvert[65], lcConvert[64], lcConvert[63], lcConvert[62], lcConvert[61]);
                    lcFormatString[5] = string.Format("0x{7}{6}{5}{4}{3}{2}{1}{0}", lcConvert[78], lcConvert[77], lcConvert[76], lcConvert[75], lcConvert[74], lcConvert[73], lcConvert[72], lcConvert[71]);
                    lcFormatString[6] = string.Format("0x{7}{6}{5}{4}{3}{2}{1}{0}", lcConvert[88], lcConvert[87], lcConvert[86], lcConvert[85], lcConvert[84], lcConvert[83], lcConvert[82], lcConvert[81]);
                    lcFormatString[7] = string.Format("0x{7}{6}{5}{4}{3}{2}{1}{0}", lcConvert[98], lcConvert[97], lcConvert[96], lcConvert[95], lcConvert[94], lcConvert[93], lcConvert[92], lcConvert[91]);
                    lcFormatString[8] = string.Format("0x{7}{6}{5}{4}{3}{2}{1}{0}", lcConvert[108], lcConvert[107], lcConvert[106], lcConvert[105], lcConvert[104], lcConvert[103], lcConvert[102], lcConvert[101]);
                    lcFormatString[9] = string.Format("0x{7}{6}{5}{4}{3}{2}{1}{0}", lcConvert[118], lcConvert[117], lcConvert[116], lcConvert[115], lcConvert[114], lcConvert[113], lcConvert[112], lcConvert[111]);





                    lcFormatInt[0] = Convert.ToInt64(lcFormatString[0], 16);
                    lcFormatInt[1] = Convert.ToInt64(lcFormatString[1], 16);
                    lcFormatInt[2] = Convert.ToInt64(lcFormatString[2], 16);
                    lcFormatInt[3] = Convert.ToInt64(lcFormatString[3], 16);
                    lcFormatInt[4] = Convert.ToInt64(lcFormatString[4], 16);
                    lcFormatInt[5] = Convert.ToInt64(lcFormatString[5], 16);
                    lcFormatInt[6] = Convert.ToInt64(lcFormatString[6], 16);
                    lcFormatInt[7] = Convert.ToInt64(lcFormatString[7], 16);
                    lcFormatInt[8] = Convert.ToInt64(lcFormatString[8], 16);
                    lcFormatInt[9] = Convert.ToInt64(lcFormatString[9], 16);







                    double[] lcGetResult = new double[10];


                    lcGetResult[0] = BitConverter.Int64BitsToDouble(lcFormatInt[0]);
                    lcGetResult[1] = BitConverter.Int64BitsToDouble(lcFormatInt[1]);
                    lcGetResult[2] = BitConverter.Int64BitsToDouble(lcFormatInt[2]);
                    lcGetResult[3] = BitConverter.Int64BitsToDouble(lcFormatInt[3]);
                    lcGetResult[4] = BitConverter.Int64BitsToDouble(lcFormatInt[4]);
                    lcGetResult[5] = BitConverter.Int64BitsToDouble(lcFormatInt[5]);
                    lcGetResult[6] = BitConverter.Int64BitsToDouble(lcFormatInt[6]);
                    lcGetResult[7] = BitConverter.Int64BitsToDouble(lcFormatInt[7]);
                    lcGetResult[8] = BitConverter.Int64BitsToDouble(lcFormatInt[8]);
                    lcGetResult[9] = BitConverter.Int64BitsToDouble(lcFormatInt[9]);



                    for (int i = 0; i < getUPSUserDefineValue.Length; i++)
                        getUPSUserDefineValue[i] = lcGetResult[i];
                }

                lcResult = true;
            }
            else
                lcResult = false;

            return getUPSUserDefineValue;
        }
        catch
        {
            rst_UPSConnect();
            init_UPSConnect();
            lcResult = false;
            return getUPSUserDefineValue;
        }

    }

    #endregion

    #region //GPS 항목
    public float[] get_GPSVoltage(out bool lcResult)
    {
        try
        {
            if (iSendGPS > 0xFFFF)
                iSendGPS = 0;

            byte[] lcSendTransaction = BitConverter.GetBytes(iSendGPS);

            byte[] lcSend = new byte[12];

            lcSend[0] = lcSendTransaction[1];
            lcSend[1] = lcSendTransaction[0];

            lcSend[2] = cProtocalID_Upper;
            lcSend[3] = cProtocalID_Lower;

            lcSend[4] = cDataLength_Upper;
            lcSend[5] = cDataLength_Lower;

            lcSend[6] = cGPSUnitID;

            lcSend[7] = cFunctionCode;

            lcSend[8] = cVoltage_Request_UpperAddress;
            lcSend[9] = cVoltage_Request_LowerAddress;

            lcSend[10] = cVoltage_Request_UpperLength;
            lcSend[11] = cVoltage_Request_LowerLength;

            iSendGPS++;

            networkStream[cGPS].Write(lcSend, 0, lcSend.Length);

            byte[] lcGetData = new byte[1024];
            int lcCount = 0;

            if (networkStream[cGPS].CanRead)
            {
                lcCount = networkStream[cGPS].Read(lcGetData, 0, lcGetData.Length);

                if (lcSend[0] == lcGetData[0] && lcSend[1] == lcGetData[1] && lcSend[7] == lcGetData[7])
                {
                    byte[] lcGet = new byte[lcCount];
                    string[] lcConvert = new string[lcCount];

                    Array.Copy(lcGetData, lcGet, lcCount);

                    for (int i = 0; i < lcCount; i++)
                        lcConvert[i] = Convert.ToString(lcGet[i], 16).PadLeft(2, '0');

                    string[] lcFormatString = new string[8];
                    int[] lcFormatInt = new int[8];

                    lcFormatString[0] = string.Format("0x{0}{1}{2}{3}", lcConvert[9], lcConvert[10], lcConvert[11], lcConvert[12]);
                    lcFormatString[1] = string.Format("0x{0}{1}{2}{3}", lcConvert[13], lcConvert[14], lcConvert[15], lcConvert[16]);
                    lcFormatString[2] = string.Format("0x{0}{1}{2}{3}", lcConvert[17], lcConvert[18], lcConvert[19], lcConvert[20]);
                    lcFormatString[3] = string.Format("0x{0}{1}{2}{3}", lcConvert[21], lcConvert[22], lcConvert[23], lcConvert[24]);
                    lcFormatString[4] = string.Format("0x{0}{1}{2}{3}", lcConvert[25], lcConvert[26], lcConvert[27], lcConvert[28]);
                    lcFormatString[5] = string.Format("0x{0}{1}{2}{3}", lcConvert[29], lcConvert[30], lcConvert[31], lcConvert[32]);
                    lcFormatString[6] = string.Format("0x{0}{1}{2}{3}", lcConvert[33], lcConvert[34], lcConvert[35], lcConvert[36]);
                    lcFormatString[7] = string.Format("0x{0}{1}{2}{3}", lcConvert[37], lcConvert[38], lcConvert[39], lcConvert[40]);

                    lcFormatInt[0] = Convert.ToInt32(lcFormatString[0], 16);
                    lcFormatInt[1] = Convert.ToInt32(lcFormatString[1], 16);
                    lcFormatInt[2] = Convert.ToInt32(lcFormatString[2], 16);
                    lcFormatInt[3] = Convert.ToInt32(lcFormatString[3], 16);
                    lcFormatInt[4] = Convert.ToInt32(lcFormatString[4], 16);
                    lcFormatInt[5] = Convert.ToInt32(lcFormatString[5], 16);
                    lcFormatInt[6] = Convert.ToInt32(lcFormatString[6], 16);
                    lcFormatInt[7] = Convert.ToInt32(lcFormatString[7], 16);

                    float[] lcGetResult = new float[8];

                    for (int i = 0; i < lcFormatInt.Length; i++)
                    {
                        byte[] lcByte = BitConverter.GetBytes(lcFormatInt[i]);
                        lcGetResult[i] = BitConverter.ToSingle(lcByte, 0);
                    }

                    for (int i = 0; i < getGPSVoltageValue.Length; i++)
                        getGPSVoltageValue[i] = lcGetResult[i];
                }

                lcResult = true;
            }
            else
                lcResult = false;

            return getGPSVoltageValue;
        }
        catch
        {
            rst_GPSConnect();
            init_GPSConnect();

            lcResult = false;
            return getGPSVoltageValue;
        }
    }

    public void get_GPSPower(out bool lcResult, out float[] lcReturn01, out int[] lcReturn02)
    {
        float[] lcGetResult = new float[13];
        int[] lcGetResult01 = new int[4];
        try
        {
            if (iSendGPS > 0xFFFF)
                iSendGPS = 0;

            byte[] lcSendTransaction = BitConverter.GetBytes(iSendGPS);

            byte[] lcSend = new byte[12];

            lcSend[0] = lcSendTransaction[1];
            lcSend[1] = lcSendTransaction[0];

            lcSend[2] = cProtocalID_Upper;
            lcSend[3] = cProtocalID_Lower;

            lcSend[4] = cDataLength_Upper;
            lcSend[5] = cDataLength_Lower;

            lcSend[6] = cGPSUnitID;

            lcSend[7] = cFunctionCode;

            lcSend[8] = cGPSPower_Request_UpperAddress;
            lcSend[9] = cGPSPower_Request_LowerAddress;

            lcSend[10] = cGPSPower_Request_UpperLength;
            lcSend[11] = cGPSPower_Request_LowerLength;

            iSendGPS++;

            networkStream[cGPS].Write(lcSend, 0, lcSend.Length);

            byte[] lcGetData = new byte[1024];
            int lcCount = 0;

            if (networkStream[cGPS].CanRead)
            {
                lcCount = networkStream[cGPS].Read(lcGetData, 0, lcGetData.Length);

                if (lcSend[0] == lcGetData[0] && lcSend[1] == lcGetData[1] && lcSend[7] == lcGetData[7])
                {
                    byte[] lcGet = new byte[lcCount];
                    string[] lcConvert = new string[lcCount];

                    Array.Copy(lcGetData, lcGet, lcCount);

                    for (int i = 0; i < lcCount; i++)
                        lcConvert[i] = Convert.ToString(lcGet[i], 16).PadLeft(2, '0');

                    //string[] lcFormatString = new string[4];
                    //int[] lcFormatInt = new int[4];
                    string[] lcFormatString = new string[17];
                    int[] lcFormatInt = new int[17];

                    lcFormatString[0] = string.Format("0x{0}{1}{2}{3}", lcConvert[9], lcConvert[10], lcConvert[11], lcConvert[12]); //A상 유효 전력
                    lcFormatString[1] = string.Format("0x{0}{1}{2}{3}", lcConvert[13], lcConvert[14], lcConvert[15], lcConvert[16]); //B상 유효 전력
                    lcFormatString[2] = string.Format("0x{0}{1}{2}{3}", lcConvert[17], lcConvert[18], lcConvert[19], lcConvert[20]); //C상 유효 전력
                    lcFormatString[3] = string.Format("0x{0}{1}{2}{3}", lcConvert[21], lcConvert[22], lcConvert[23], lcConvert[24]); //삼상 유효 전력 총합
                    lcFormatString[4] = string.Format("0x{0}{1}{2}{3}", lcConvert[25], lcConvert[26], lcConvert[27], lcConvert[28]); //A상 무효 전력
                    lcFormatString[5] = string.Format("0x{0}{1}{2}{3}", lcConvert[29], lcConvert[30], lcConvert[31], lcConvert[32]); //B상 무효 전력
                    lcFormatString[6] = string.Format("0x{0}{1}{2}{3}", lcConvert[33], lcConvert[34], lcConvert[35], lcConvert[36]); //C상 무효 전력
                    lcFormatString[7] = string.Format("0x{0}{1}{2}{3}", lcConvert[37], lcConvert[38], lcConvert[39], lcConvert[40]); //삼상 무효 전력 총합
                    lcFormatString[8] = string.Format("0x{0}{1}{2}{3}", lcConvert[41], lcConvert[42], lcConvert[43], lcConvert[44]); //A상 피상 전력
                    lcFormatString[9] = string.Format("0x{0}{1}{2}{3}", lcConvert[45], lcConvert[46], lcConvert[47], lcConvert[48]); //B상 피상 전력
                    lcFormatString[10] = string.Format("0x{0}{1}{2}{3}", lcConvert[49], lcConvert[50], lcConvert[51], lcConvert[52]); //C상 피상 전력
                    lcFormatString[11] = string.Format("0x{0}{1}{2}{3}", lcConvert[53], lcConvert[54], lcConvert[55], lcConvert[56]); //삼상 피상 전력 총합
                    lcFormatString[12] = string.Format("0x{0}{1}{2}{3}", lcConvert[57], lcConvert[58], lcConvert[59], lcConvert[60]); //ZCT 누설 전류 (미사용)
                    lcFormatString[13] = string.Format("0x{0}{1}{2}{3}", lcConvert[61], lcConvert[62], lcConvert[63], lcConvert[64]); //삼상 수전 유효 전력(미사용)
                    lcFormatString[14] = string.Format("0x{0}{1}{2}{3}", lcConvert[65], lcConvert[66], lcConvert[67], lcConvert[68]); //삼상 송전 유효 전력(미사용)
                    lcFormatString[15] = string.Format("0x{0}{1}{2}{3}", lcConvert[69], lcConvert[70], lcConvert[71], lcConvert[72]); //수전 유효전력량 + 송전 유효전력량(미사용)
                    lcFormatString[16] = string.Format("0x{0}{1}{2}{3}", lcConvert[73], lcConvert[74], lcConvert[75], lcConvert[76]); //수전 유효전력량 - 송전 유효전력량 (사용)



                    lcFormatInt[0] = Convert.ToInt32(lcFormatString[0], 16);
                    lcFormatInt[1] = Convert.ToInt32(lcFormatString[1], 16);
                    lcFormatInt[2] = Convert.ToInt32(lcFormatString[2], 16);
                    lcFormatInt[3] = Convert.ToInt32(lcFormatString[3], 16);
                    lcFormatInt[4] = Convert.ToInt32(lcFormatString[4], 16);
                    lcFormatInt[5] = Convert.ToInt32(lcFormatString[5], 16);
                    lcFormatInt[6] = Convert.ToInt32(lcFormatString[6], 16);
                    lcFormatInt[7] = Convert.ToInt32(lcFormatString[7], 16);
                    lcFormatInt[8] = Convert.ToInt32(lcFormatString[8], 16);
                    lcFormatInt[9] = Convert.ToInt32(lcFormatString[9], 16);
                    lcFormatInt[10] = Convert.ToInt32(lcFormatString[10], 16);
                    lcFormatInt[11] = Convert.ToInt32(lcFormatString[11], 16);
                    lcFormatInt[12] = Convert.ToInt32(lcFormatString[12], 16);
                    lcFormatInt[13] = Convert.ToInt32(lcFormatString[13], 16);
                    lcFormatInt[14] = Convert.ToInt32(lcFormatString[14], 16);
                    lcFormatInt[15] = Convert.ToInt32(lcFormatString[15], 16);
                    lcFormatInt[16] = Convert.ToInt32(lcFormatString[16], 16);





                    for (int i = 0; i < lcFormatInt.Length; i++)
                    {
                        byte[] lcByte = BitConverter.GetBytes(lcFormatInt[i]);
                        if (i < 13)
                            lcGetResult[i] = BitConverter.ToSingle(lcByte, 0);
                        else if (i == 13)
                            lcGetResult01[0] = BitConverter.ToInt32(lcByte, 0);
                        else if (i == 14)
                            lcGetResult01[1] = BitConverter.ToInt32(lcByte, 0);
                        else if (i == 15)
                            lcGetResult01[2] = BitConverter.ToInt32(lcByte, 0);
                        else if (i == 16)
                            lcGetResult01[3] = BitConverter.ToInt32(lcByte, 0);

                    }





                    //for (int i = 0; i < getGPSPowerValue.Length; i++)
                    //    getGPSPowerValue[i] = lcGetResult[i];

                }

                lcReturn01 = lcGetResult;
                lcReturn02 = lcGetResult01;

                lcResult = true;
            }
            else
            {
                lcReturn01 = lcGetResult;
                lcReturn02 = lcGetResult01;
                lcResult = false;
            }
            

        }
        catch
        {
            rst_GPSConnect();
            init_GPSConnect();

            lcReturn01 = lcGetResult;
            lcReturn02 = lcGetResult01;

            lcResult = false;
            
        }
    }



    public float[] get_GPSCurrent(out bool lcResult)
    {
        try
        {
            if (iSendGPS > 0xFFFF)
                iSendGPS = 0;

            byte[] lcSendTransaction = BitConverter.GetBytes(iSendGPS);

            byte[] lcSend = new byte[12];

            lcSend[0] = lcSendTransaction[1];
            lcSend[1] = lcSendTransaction[0];

            lcSend[2] = cProtocalID_Upper;
            lcSend[3] = cProtocalID_Lower;

            lcSend[4] = cDataLength_Upper;
            lcSend[5] = cDataLength_Lower;

            lcSend[6] = cGPSUnitID;

            lcSend[7] = cFunctionCode;

            lcSend[8] = cGPSCurrent_Request_UpperAddress;
            lcSend[9] = cGPSCurrent_Request_LowerAddress;

            lcSend[10] = cGPSPower_Request_UpperLength;
            lcSend[11] = cGPSPower_Request_LowerLength;

            iSendGPS++;

            networkStream[cGPS].Write(lcSend, 0, lcSend.Length);

            byte[] lcGetData = new byte[1024];
            int lcCount = 0;

            if (networkStream[cGPS].CanRead)
            {
                lcCount = networkStream[cGPS].Read(lcGetData, 0, lcGetData.Length);

                if (lcSend[0] == lcGetData[0] && lcSend[1] == lcGetData[1] && lcSend[7] == lcGetData[7])
                {
                    byte[] lcGet = new byte[lcCount];
                    string[] lcConvert = new string[lcCount];

                    Array.Copy(lcGetData, lcGet, lcCount);

                    for (int i = 0; i < lcCount; i++)
                        lcConvert[i] = Convert.ToString(lcGet[i], 16).PadLeft(2, '0');

                    string[] lcFormatString = new string[4];
                    int[] lcFormatInt = new int[4];

                    lcFormatString[0] = string.Format("0x{0}{1}{2}{3}", lcConvert[9], lcConvert[10], lcConvert[11], lcConvert[12]);
                    lcFormatString[1] = string.Format("0x{0}{1}{2}{3}", lcConvert[13], lcConvert[14], lcConvert[15], lcConvert[16]);
                    lcFormatString[2] = string.Format("0x{0}{1}{2}{3}", lcConvert[17], lcConvert[18], lcConvert[19], lcConvert[20]);
                    lcFormatString[3] = string.Format("0x{0}{1}{2}{3}", lcConvert[21], lcConvert[22], lcConvert[23], lcConvert[24]);


                    lcFormatInt[0] = Convert.ToInt32(lcFormatString[0], 16);
                    lcFormatInt[1] = Convert.ToInt32(lcFormatString[1], 16);
                    lcFormatInt[2] = Convert.ToInt32(lcFormatString[2], 16);
                    lcFormatInt[3] = Convert.ToInt32(lcFormatString[3], 16);


                    float[] lcGetResult = new float[4];

                    for (int i = 0; i < lcFormatInt.Length; i++)
                    {
                        byte[] lcByte = BitConverter.GetBytes(lcFormatInt[i]);
                        lcGetResult[i] = BitConverter.ToSingle(lcByte, 0);
                    }

                    for (int i = 0; i < getGPSCurrentValue.Length; i++)
                        getGPSCurrentValue[i] = lcGetResult[i];
                }

                lcResult = true;
            }
            else lcResult = false;

            return getGPSCurrentValue;
        }
        catch
        {
            rst_GPSConnect();
            init_GPSConnect();

            lcResult = true;
            return getGPSCurrentValue;
        }
    }

    #endregion
}



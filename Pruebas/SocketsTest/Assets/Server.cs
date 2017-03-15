using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

public class Server : MonoBehaviour
{

    static Server singleton;

	private Socket m_Socket;
	private Socket m_SocketTick;

	ArrayList m_Connections = new ArrayList();

    ArrayList m_Buffer = new ArrayList();
    ArrayList m_ByteBuffer = new ArrayList();

    void Awake()
    {
		Debug.Log("Error");
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		m_SocketTick = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );

		//IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, Client.kPort);

        //m_Socket.Bind(ipLocal);

		//start listening...
		m_Socket.Listen( 10000 );
		m_SocketTick.Listen( 11000 );

		singleton = this;
    }

    void OnApplicationQuit()
    {
        Cleanup();
    }

    void Cleanup()
    {
        if (m_Socket != null)
            m_Socket.Close();
        m_Socket = null;

        foreach (Socket con in m_Connections)
            con.Close();
        m_Connections.Clear();
    }
    ~Server()
    {
        Cleanup();
    }

    void Update()
    {
        Debug.Log("Entered Update on Server.");
        // Accept any incoming connections!
        ArrayList listenList = new ArrayList();
        listenList.Add(m_Socket);
        Socket.Select(listenList, null, null, 1000);
        for (int i = 0; i < listenList.Count; i++)
        {
            Socket newSocket = ((Socket)listenList[i]).Accept();
            m_Connections.Add(newSocket);
            m_ByteBuffer.Add(new ArrayList());
            Debug.Log("Did connect");
        }

        // Read data from the connections!
        if (m_Connections.Count != 0)
        {
            Debug.Log("There's a connection with the server, processing on the server.");
            ArrayList connections = new ArrayList(m_Connections);
            Socket.Select(connections, null, null, 1000);
            // Go through all sockets that have data incoming!
            foreach (Socket socket in connections)
            {
                byte[] receivedbytes = new byte[512];

                ArrayList buffer = (ArrayList)m_ByteBuffer[m_Connections.IndexOf(socket)];
                int read = socket.Receive(receivedbytes);
                for (int i = 0; i < read; i++)
                    buffer.Add(receivedbytes[i]);

                while (true && buffer.Count > 0)
                {
                    int length = (byte)buffer[0];

                    if (length < buffer.Count)
                    {
                        ArrayList thismsgBytes = new ArrayList(buffer);
                        thismsgBytes.RemoveRange(length + 1, thismsgBytes.Count - (length + 1));
                        thismsgBytes.RemoveRange(0, 1);
                        if (thismsgBytes.Count != length)
                            Debug.Log("Bug");

                        buffer.RemoveRange(0, length + 1);
                        byte[] readbytes = (byte[])thismsgBytes.ToArray(typeof(byte));

                        MessageData readMsg = MessageData.FromByteArray(readbytes);
                        m_Buffer.Add(readMsg);

                        //Debug.Log(System.String.Format("Message {0}: {1}, {2}", readMsg.stringData, readMsg.mousex, readMsg.mousey));

                        if (singleton != this)
                            Debug.Log("Bug");
                    }
                    else
                        break;
                }

                // string output = Encoding.UTF8.GetString(bytes);
            }
        }
    }

    static public MessageData PopMessage()
    {
        Debug.Log("Entered PopMessage on Server class.");
        if (singleton.m_Buffer.Count == 0)
        {
            return null;
        }
        else
        {
            MessageData readMsg = (MessageData)singleton.m_Buffer[0];
            singleton.m_Buffer.RemoveAt(0);
            // Debug.Log(System.String.Format("Message {0}: {1}, {2}", readMsg.stringData, readMsg.mousex, readMsg.mousey));
            return readMsg;
        }
    }
}
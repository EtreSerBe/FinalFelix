using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;

public class Client : MonoBehaviour
{

	public string m_IPAdress = "10.0.5.212";//"127.0.0.1";
	public const int kPort = 10000;

	private static Client singleton;


	private Socket m_Socket;
	void Start( )
	{
		m_Socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );

		// System.Net.PHostEntry ipHostInfo = Dns.Resolve("host.contoso.com");
		// System.Net.IPAddress remoteIPAddress = ipHostInfo.AddressList[0];
		System.Net.IPAddress remoteIPAddress = System.Net.IPAddress.Parse(m_IPAdress);

		System.Net.IPEndPoint remoteEndPoint = new System.Net.IPEndPoint(remoteIPAddress, kPort);

		singleton = this;
		m_Socket.Connect( remoteEndPoint );
	}

	void OnApplicationQuit( )
	{
		m_Socket.Close();
		m_Socket = null;
	}

	static public void Send( MessageData msgData )
	{

		Debug.Log( "Entering Send on Client class." );

		if ( singleton.m_Socket == null )
			return;

		byte[] sendData = MessageData.ToByteArray(msgData);
		byte[] prefix = new byte[1];
		prefix[0] = (byte)sendData.Length;
		singleton.m_Socket.Send( prefix );
		singleton.m_Socket.Send( sendData );
	}
}

/*
using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

public class Client : MonoBehaviour
{

	public string m_IPAdress = "";

	private static Client m_CachedClient;


	private UdpClient m_UdpSocket = new UdpClient( CGlobals.m_iHostListenPort );

	void Start( )
	{
		m_UdpSocket.EnableBroadcast = true;
		Thread newThread = new Thread( fFindHost );
		
		//Esto no va
		m_CachedClient = this;
		m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Udp);
		System.Net.IPAddress remoteIPAddress = System.Net.IPAddress.Parse(m_IPAdress);
		System.Net.IPEndPoint remoteEndPoint = new System.Net.IPEndPoint(remoteIPAddress, kPort);
		m_Socket.Connect(remoteEndPoint);
	}

	void fFindHost()
	{
		IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, CGlobals.m_iHostAnswerPort );

		int count = 0;
		while ( count < 4 )
		{
			byte[] message = Encoding.ASCII.GetBytes("Looking for leader.");
			m_UdpSocket.Send( message, message.Length );

			byte[] receiveBytes = m_UdpSocket.Receive(ref RemoteIpEndPoint);
			string returnData = Encoding.ASCII.GetString(receiveBytes);
		}
	}

	void OnApplicationQuit( )
	{
		//m_Socket.Close();
		//m_Socket = null;
	}

	static public void Send( MessageData msgData )
	{
		//Esto no va
		Debug.Log("Entering Send on Client class.");

		 if (m_CachedClient.m_Socket == null)
			 return;

		 byte[] sendData = MessageData.ToByteArray(msgData);
		 byte[] prefix = new byte[1];
		 prefix[0] = (byte)sendData.Length;
		 m_CachedClient.m_Socket.Send(prefix);
		 m_CachedClient.m_Socket.Send(sendData);
	}
}
*/

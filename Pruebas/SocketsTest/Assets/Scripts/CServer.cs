using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;


public class CServer : MonoBehaviour
{
	/*< Cached server. */
	static CServer m_CachedServer;
	CThreadManager m_ThreadManager;

	Socket  m_Socket,
			m_SocketTick;

    UdpClient m_udpServer;

	ArrayList m_lstClients = new ArrayList();

	public static CServer CachedServer
	{
		get { return m_CachedServer; }
	}

	void Awake( )
	{
		m_ThreadManager = new CThreadManager();
		m_CachedServer = this;

		m_Socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
		//m_SocketTick = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );


		IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, 10000);
		m_Socket.ExclusiveAddressUse = false;
		//m_Socket.Blocking = false;

		m_Socket.Bind( ipLocal );
		m_Socket.Listen( 100 );
		//m_SocketTick.Listen( 11000 );
		Thread newThread = new Thread( openTCP );
		Thread newThread2 = new Thread( openUDP );

		newThread.Start();
		newThread2.Start();
	}

	void openTCP( )
	{
		try
		{
			Socket tmpSocket = m_Socket.Accept();
			if ( tmpSocket != null )
			{
				string ipAddress = ((IPEndPoint)tmpSocket.RemoteEndPoint).Address.ToString();
				Debug.Log( ipAddress );
				CClientHandler newClient = new CClientHandler( tmpSocket, ipAddress );
				Thread newThread = new Thread( newClient.run );
				m_lstClients.Add( newClient );
				m_ThreadManager.AddThread( newClient.run );
				newThread.Start();
			}
		}
		catch ( SocketException e )
		{
			//Debug.Log( e );
		}
	}

	void openUDP( )
	{

	}
}

public class CThreadManager
{
	ArrayList m_lstThreads = new ArrayList();

	public CThreadManager()
	{

	}

	public void AddThread( ThreadStart in_function )
	{
		Thread newThread = new Thread( in_function );
		m_lstThreads.Add( newThread );
	}

	public void EndThread( int in_ThreadNumber )
	{
		( (Thread)m_lstThreads[in_ThreadNumber] ).Interrupt();//If necessary use abort.
		m_lstThreads.RemoveAt( in_ThreadNumber );
	}
}


public class CClientHandler
{
	Socket m_Socket;
	string m_strIpAddress;

	public CClientHandler( Socket in_Socket, string in_strIpAddress )
	{
		m_Socket = in_Socket;
		m_strIpAddress = in_strIpAddress;
	}

	public void run( )
	{
		while ( true )
		{
			Debug.Log( "Update: " + m_strIpAddress );
		}
	}
}

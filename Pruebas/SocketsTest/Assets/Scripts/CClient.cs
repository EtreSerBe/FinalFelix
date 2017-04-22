using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System;

public class CClient : MonoBehaviour
{

    UdpClient m_udpClient;
    private float m_fTimeSinceLastResponse = 0.0f;  //This value must be reset when a response from the server is received.
    public float m_fMaxTimeSinceLastResponse = 5.0f;
    public bool m_bAutoReceiveMessages = true; //Deactivate this to stop this client from receiving its own messages.
    public int m_iMaxMinutesSinceLastResponse = 0; //NOTE: This values fro last response are used in the CServer class. Preferably, they'll be moved to globals.
    public int m_iMaxSecondsSinceLastResponse = 15;
    public float m_fHeartBeatMessageInterval = 2.5f;
    public string m_szPseudoBroadcastAddress = "223.0.0.0"; //some random value.

    bool bDisconnected = false;//This value must be modified when a first response of the server/host is received.
    public int m_iID = 0; //ID value used to identify the clients from the Server's perspective.
    public string m_szClientIP = "0.0.0.0";
    public int m_iServerID = -2;

    public string m_szServerIP = "0.0.0.0";

    List<Message> m_MessagesList = new List<Message>();
    //HashSet<ClientInfo> m_setKnownClients = new HashSet<ClientInfo>();
    Dictionary<string, ClientInfo> m_dicKnownClients = new Dictionary<string, ClientInfo>();

    CServer m_pServer = null; //null by default, only has a valid value when this client's machine is also executing the server.

    public string m_szMulticastIP = "223.0.0.0"; //set by default, the Server might change it for security purposes.
    public int m_iMulticastPort = 10000;



    //CServer m_ServerReference:

    // Use this for initialization
    void Start()
    {
        //Found this trick to get the initial IP Address, not sure if it works without connection. 
        //http://stackoverflow.com/questions/6803073/get-local-ip-address
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect("8.8.8.8", 65530);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            m_szClientIP = endPoint.Address.ToString();
            Debug.Log(m_szClientIP + " is this client's IP address");
        }

        //Client uses as receive udp client
        m_udpClient = new UdpClient( 10000);
        m_udpClient.EnableBroadcast = true;
        m_udpClient.MulticastLoopback = true; //Necessary so it receives its own messages in the multicast.
        //Debug.Log("The updClients ExclusiveAddressUse value is: " + m_udpClient.ExclusiveAddressUse.ToString());// it must false;
        m_pServer = null;
        try
        {
            m_fTimeSinceLastResponse = 0.0f;
            Debug.Log("Beggining to receive: ");
            m_udpClient.BeginReceive(new AsyncCallback(recv), null);
          
            SendUDPMessage('Y', "Begin_Con", "Empty", IPAddress.Broadcast.ToString(), 10000);
        }
        catch (Exception e)
        {
            Debug.Log("caught exception : " + e.ToString());
        }
    }

    //Unity predefined event called when the application is Quit.
    private void OnApplicationQuit()
    {
        //NOTE::: MAYBE IT COULD NOTIFY THE OTHERS BY ITSELF, WITH A SEND TO GROUP MESSAGE.
        Debug.Log("Quitting application, notifying the Server, so he notifies everyone else.");
        SendUDPMessage('Y', "User_Quit", "Empty", m_szServerIP, 10000);
    }

    //CallBack, it's automatically called when the socket receives anything.
    private void recv(IAsyncResult res)
    {
        Debug.Log("Entered recv function, Callback for the BeginReceive function.");
        //Also, reset the time since the last message was received.
        m_fTimeSinceLastResponse = 0.0f; //NOTE:::: Check if it has to be here...

        IPEndPoint RemoteIpEndPoint;
        //Receive messages especifically sent to this client's IP by the server.
        RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(m_szServerIP), 10000);
        byte[] received = m_udpClient.EndReceive(res, ref RemoteIpEndPoint);
        m_udpClient.BeginReceive(new AsyncCallback(recv), null);
        Debug.Log("The received data was: " + Encoding.UTF8.GetString(received));
        Message pReceivedMessage = new Message(received); //Construct the message with the special contructor which receives an array of bytes.
        Debug.Log("the Destination Address value received was: " + pReceivedMessage.m_szDestinationAddress);
        if (pReceivedMessage.m_szDestinationAddress == IPAddress.Broadcast.ToString())
        {
            //Then it is from a new user.
            if (m_pServer != null)
            {
                Debug.LogWarning("Warning, this SERVER: " + m_szClientIP + " has received a BROADCAST message. Adding it to the message list of the server");
                if (pReceivedMessage.m_cIsForServer == 'Y')
                    m_pServer.m_MessagesList.Add(pReceivedMessage);
                else
                    Debug.LogError("Error, a Broadcast message was received, but it was not Sent to the Server!");
            }
            //Otherwise, we just ignore it.
        }
        else if (pReceivedMessage.m_szDestinationAddress == m_szClientIP)
        {
            //Then, it was specifically to this IP.
            Debug.Log("A message to this SPECIFIC ADDRESS -" + m_szClientIP + " was received, its contents are: " + pReceivedMessage.ToString());
            if (pReceivedMessage.m_cIsForServer == 'Y')
            {
                Debug.Log("The specific message was to the server.");
                if (m_pServer == null)
                    Debug.LogError("ERROR, no server is available in this IP: " + m_szClientIP + " please check your data."); // NOTE: this happens when closing the application, but should be ok.
                else
                {
                    Debug.Log("Specific message added to the server's list correctly with contents: " + pReceivedMessage.ToString());
                    m_pServer.m_MessagesList.Add(pReceivedMessage);
                }
            }
            else
            {
                Debug.Log("The specific message was for the client, adding it to its message list.");
                m_MessagesList.Add(pReceivedMessage);
            }
        }
        else if (pReceivedMessage.m_szDestinationAddress == m_szMulticastIP)
        {
            //Then, it was sent to the multicast group.
            Debug.Log("MULTICAST message received, adding it to its message list.");
            m_MessagesList.Add(pReceivedMessage);
            //This is where we start the check for inactivity of the users, inside the Coroutine.
            if (m_pServer != null) //check if this Node is the same one as the server, if it is, then it must check the timeouts.
            {
                m_pServer.UpdateLastMessageFromAddress(pReceivedMessage.m_szTargetAddress);//check this values.
            }
        }
        else
        {
            Debug.LogWarning("This Client: " + m_szClientIP + " received a message without an specific PURPOSE. Beware of suspicious. The Destination address was: " + pReceivedMessage.m_szDestinationAddress);
        }
    }

    //This one is used to differentiate between messages to the server and messages received by someone who is trying to access the service.
    private bool  DispatchMessageToServer( Message in_pDispatchMessage )
    {
        //Decode the "time" or something like that so it helps filter old messages.
        //this is to be able to sort the messageList according to how old they are. ???
        //First, check if it is for the client or for the server (if this machine happens to be the server too).
        if (m_pServer != null && in_pDispatchMessage.m_cIsForServer == 'Y')
        {
            //Then, it means it is destined for the server part, not the client. And the server ir running on this machine.
            m_pServer.m_MessagesList.Add(in_pDispatchMessage);
            Debug.Log("The message just got passed to the SERVER, not to this client. Its content is: " + in_pDispatchMessage.ToString());
            return true;
        }
        return false;
    }

    private void sendCallback(IAsyncResult res)
    {
        //Debug.Log("Entered sendCallback function, Callback for the BeginSend function.");
        Debug.Log("Number of bytes sent by: " + m_szClientIP +  " were: " + m_udpClient.EndSend(res));//The call to "m_udpClient.EndSend(res)" is indispensable!
        //Debug.Log("Exit sendCallback function, Callback for the BeginSend function.");
    }

    //I still have my doubts about this one, maybe it should have other parameters as well.
    public void SendUDPMessage(char in_isForServer, string in_szTypeOfMessage, string in_szMessageContent, string in_szAddress, int in_iPort  )
    {
        Message NewMessage = new Message(in_isForServer, m_iID.ToString(), m_szClientIP, in_szTypeOfMessage, in_szAddress, in_szMessageContent);
        byte[] msgBytes = Encoding.UTF8.GetBytes(NewMessage.ToString());
        Debug.Log("Sending a message to Endpoint: " + in_szAddress + " and port: " + in_iPort + " from IP: " + m_szClientIP );
        //Send a message to the server.
        
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(in_szAddress), in_iPort);
        m_udpClient.BeginSend(msgBytes, msgBytes.Length, RemoteIpEndPoint, sendCallback, null);//Do the broadcast.
    }

    //facility to be a little more organized.
    public void SendUDPMessage(Message in_pMessage, string in_szAddress, int in_iPort)
    {
        SendUDPMessage(in_pMessage.m_cIsForServer, in_pMessage.m_szMessageType, in_pMessage.m_szMessageContent, in_szAddress, in_iPort);
    }

    //This one uses the defined Multicast Address and port to send, so it's less tedious to use.
    public void SendUDPMessageToGroup(char in_isForServer, string in_szTypeOfMessage, string in_szMessageContent )
    {
        Message NewMessage = new Message(in_isForServer, m_iID.ToString(), m_szClientIP, in_szTypeOfMessage, m_szMulticastIP, in_szMessageContent);
        byte[] msgBytes = Encoding.UTF8.GetBytes(NewMessage.ToString());

        Debug.Log("Sending a message to Multicast group address: " + m_szMulticastIP.ToString() + " and port: " + m_iMulticastPort + " from IP: " + m_szClientIP);
        //Send a message to the server.
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse( m_szMulticastIP), m_iMulticastPort);
        m_udpClient.BeginSend(msgBytes, msgBytes.Length, RemoteIpEndPoint, sendCallback, null);//Do the multicast.
    }

    void HeartBeatSend()
    {
        Debug.Log("Sending HeartBeat message to Server. This is invoked repeatedly.");
        SendUDPMessage('Y', "HeartBeat", "Empty", m_szServerIP , 10000); //NOTE:: THIS WOULD BE BETTER IF ONLY SENT TO SERVER.
    }

    // Update is called once per frame
    void Update()
    {
        //Check if a new message has arrived.
        //If it has, process it.
        m_fTimeSinceLastResponse += Time.deltaTime;

        if (m_fTimeSinceLastResponse >= m_fMaxTimeSinceLastResponse && bDisconnected == false)
        {
            bDisconnected = true;
            //Now, we intend to disconnect, and try to select a new leader.
            Debug.LogWarning("The time since a response last came has exceeded the limit, disconnecting.");

            Debug.LogWarning("Trying to select a new leader/host.");
            //here, this client must check if it meets the requirements to be the new leader, such as if it has a table of other candidates or something else.

            if (SelectNewServer() == m_iID ) 
            {
                if (m_pServer != null) //need to check for null, so it doesnt add more than one.
                {
                    Debug.LogError("ERROR, tried to start a server when there's already in this client's machine.");
                    return;//exit this function.
                }
                //Start a server in this machine.
                Debug.LogWarning("This machine will now host the server. Initializing.");
                gameObject.AddComponent<CServer>(); //Add a CServer component to this gameObject, so a new Server starts. 
                m_szServerIP = m_szClientIP; //Make the IP of the server the same as this client's IP.
                gameObject.GetComponent<CServer>().StartServer(this, m_dicKnownClients); //Send this client's set of known clients to the new server.
                m_pServer = gameObject.GetComponent<CServer>();
                SendUDPMessage('Y', "Begin_Con", "Empty", IPAddress.Broadcast.ToString(), 10000);
                //bDisconnected = false; //JUST FOR TESTING.
            }
            else
            {
                //Wait some time and then try to connect to the new leader.
                Debug.LogWarning("Another machine will host the server, it's ID is: " + m_szServerIP);
                //NOTE::: MAKE THE M_SZsERVERIP equal to the IP of the new server machine.
            }
        }
        else
        {
            ProcessMessage();
        }
    }

    //
    void ProcessMessage()
    {
        //First, check if it is a recent message, or if you can have a 
        //Check if the message belongs to you or the server.
        while (m_MessagesList.Count != 0)
        {
            Message pActualMessage = m_MessagesList[0]; //Get the first element opf the container.
            m_MessagesList.RemoveAt(0); //Then, remove it from the container.
            Debug.Log("Processing message with info: " + pActualMessage.ToString());

            //Check which type of message is.
            switch (pActualMessage.m_szMessageType)
            {
                case "Conn_Accepted": //Which is when the begin connection has been accepted by the leader.
                    {
                        Debug.Log("Entered Conn_Accepted case.");

                        string [] ConnAcceptedContent = pActualMessage.m_szMessageContent.Split('\t');
                        // 1 is the multicastIP ; 2 is the Multicast port; 3 is the newID for this client; 4 is the IP if the server.
                        if (ConnAcceptedContent.Length != 4) // see that there must be 4 parameters in the content of this message.
                        {
                            Debug.LogError("The -m_szMessageContent- of a Conn_Accepted message is not in the correct format. It was: " + pActualMessage.m_szMessageContent);
                        }
                        else
                        {
                            bDisconnected = false;
                            m_fTimeSinceLastResponse = 0.0f;
                            InvokeRepeating("HeartBeatSend", 0.0f, m_fHeartBeatMessageInterval); // start the HeartBeat message sends.
                            m_szMulticastIP = ConnAcceptedContent[0];
                            m_iMulticastPort = int.Parse(ConnAcceptedContent[1]);
                            m_iID = int.Parse(ConnAcceptedContent[2]);
                            //he multicast address range is 224.0.0.0 to 239.255.255.255. If you specify an address outside this range or if the router to which 
                            //the request is made is not multicast enabled, UdpClient will throw a SocketException.
                            Debug.LogWarning("A connection has been accepted. This client: " + m_szClientIP + " will join the multicast Group: " + m_szMulticastIP + " in port: " + m_iMulticastPort.ToString());
                            m_udpClient.JoinMulticastGroup(IPAddress.Parse(m_szMulticastIP)); //NOTE:: CHECK THAT THE PORT IS NOT NECESSARY?
                            ClientInfo tmpThisClientInfo = new ClientInfo( m_iID, m_szClientIP);
                            m_dicKnownClients.Add(m_szClientIP, tmpThisClientInfo);
                        }
                        Debug.Log("Exit Conn_Accepted case.");
                    }
                    break;
                case "New_User":
                    {
                        Debug.Log("Entered New_User case.");
                        string[] NewUserContent = pActualMessage.m_szMessageContent.Split('\t');
                        // 1 is the new user ID ; 2 is the new user IP.
                        if (NewUserContent.Length != 2) // see that there must be 3 parameters in the content of this message.
                        {
                            Debug.LogError("The -m_szMessageContent- of a New_User message is not in the correct format. It was: " + pActualMessage.m_szMessageContent);
                        }
                        else
                        {
                            ClientInfo tmpNewClient = new ClientInfo( int.Parse(NewUserContent[0]) , NewUserContent[1]);
                            Debug.Log("A New User has accessed the game, its ID is: " + tmpNewClient.m_iID.ToString() + " and its IP is: " + tmpNewClient.m_szIPAdress);
                            m_dicKnownClients.Add(tmpNewClient.m_szIPAdress, tmpNewClient); //we add it to the Dictionary of known clients.
                            Debug.Log("This client now knows: " + m_dicKnownClients.Count + " Clients.");
                        }
                        Debug.Log("Exit New_User case.");
                    }
                    break;
                case "HeartBeatACK":
                    {
                        Debug.Log("Entered HeartBeatACK case : This client's HeartBeat has been ACKNOWLEDGED.");
                        m_fTimeSinceLastResponse = 0.0f;
                        bDisconnected = false;
                    }
                    break;
                case "User_Disconnect":
                    {
                        Debug.Log("Entered User_Disconnect case.");
                        //1- IP address to disconnect. 2- Reason for disconnection.
                        string [] szMessageContent = pActualMessage.m_szMessageContent.Split('\t');
                        if (szMessageContent.Length != 2) // see that there must be 3 parameters in the content of this message.
                        {
                            Debug.LogError("The -m_szMessageContent- of a No_HeartBeat message is not in the correct format. It was: " + pActualMessage.m_szMessageContent);
                        }
                        else
                        {
                            if (m_dicKnownClients.ContainsKey(szMessageContent[0]))
                            {
                                Debug.Log("Removing from Known clients the IP: " +  szMessageContent[0] + "because it is: " + szMessageContent[1]); //print the reason for the disconnection.
                                m_dicKnownClients.Remove(szMessageContent[0]);
                            }
                            else
                            {
                                Debug.LogWarning("Warning, a Client NOT KNOWN to this user is trying to be deleted. Corroborate this, please. That IP was: " + szMessageContent[0]);
                            }
                            Debug.Log("This client now knows: " + m_dicKnownClients.Count + " Clients.");
                        }
                        Debug.Log("Exit User_Disconnect case.");
                    }
                    break;


            }

            //Begin connection message.

            //In-game message, such as action performed.
        }
    }

    int SelectNewServer()
    {
        //Check the conditions to become the new server.
        //If a server has been chosen, return that ID, else, return this client's ID.
        m_iServerID = GetLowestID();
        //m_szServerIP = TODO.
        return m_iServerID; //Default value.
    }

    private int GetLowestID()
    {
        int iLowest = int.MaxValue;//Start it on the highest possible value.
        if ( m_dicKnownClients.Count == 0 ) //if it's empty, then the client chooses itself by default.
        { iLowest = m_iID; }
        else
        {
            //Else, we iterate to check the lowest ID available in the dictionary.
            foreach (KeyValuePair<string, ClientInfo> cinfo in m_dicKnownClients)
            {
                iLowest = iLowest > cinfo.Value.m_iID ? cinfo.Value.m_iID : iLowest; //If in one line to update the actual iHighest.
            }
        }
       
        return iLowest;
    }
}
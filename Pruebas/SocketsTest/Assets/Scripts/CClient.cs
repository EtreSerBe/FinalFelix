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
    public float m_fMaxTimeSinceLastResponse = 15.0f;
    public bool m_bAutoReceiveMessages = true; //Deactivate this to stop this client from receiving its own messages.
    public int m_iMaxMinutesSinceLastResponse = 0; //NOTE: This values fro last response are used in the CServer class. Preferably, they'll be moved to globals.
    public int m_iMaxSecondsSinceLastResponse = 15;
    public float m_fHeartBeatMessageInterval = 2.5f;
    public string m_szPseudoBroadcastAddress = "223.0.0.0"; //some random value.

    bool bDisconnected = false;//This value must be modified when a first response of the server/host is received.
    //bool m_bConnectionStablished = false;
    public int m_iID = 0; //ID value used to identify the clients from the Server's perspective.
    public string m_szClientIP = "0.0.0.0";
    public int m_iServerID = -2;

    public string m_szServerIP = "0.0.0.0";

    List<Message> m_MessagesList = new List<Message>();
    //HashSet<ClientInfo> m_setKnownClients = new HashSet<ClientInfo>();
    Dictionary<string, ClientInfo> m_dicKnownClients = new Dictionary<string, ClientInfo>();
    DateTime m_dtBeginDateTime ;
    //Dictionary<string, int> m_dicPreServerKnownClients = new Dictionary<string, int>();

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
            m_dtBeginDateTime = DateTime.UtcNow;
            SendUDPMessage('Y', "Begin_Con", m_dtBeginDateTime.ToString(), IPAddress.Broadcast.ToString(), 10000);
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
        Debug.LogWarning("Quitting application, notifying the Server, so he notifies everyone else. Notifing the server: " + m_szServerIP);
        if (m_szServerIP != "0.0.0.0")
        {
            //SendUDPMessage('Y', "User_Quit", "Empty", m_szServerIP, 10000);
            Message NewMessage = new Message('Y', m_iID.ToString(), m_szClientIP, "User_Quit", m_szServerIP, "Empty");
            byte[] msgBytes = Encoding.UTF8.GetBytes(NewMessage.ToString());
            m_udpClient.Send(msgBytes, msgBytes.Length, m_szServerIP, 10000);

            //Now to the multicast group synchronously.
            Message NewGroupMessage = new Message('N', m_iID.ToString(), m_szClientIP, "User_Quit", m_szMulticastIP, "Empty");
            byte[] msgBytesGroup = Encoding.UTF8.GetBytes(NewGroupMessage.ToString());
            m_udpClient.Send(msgBytesGroup, msgBytesGroup.Length, m_szMulticastIP, 10000);
            //SendUDPMessageToGroup('N', "User_Quit", "Empty");
        }
        else
        {
            Debug.LogWarning("This client knew no IPAddress for a server, so it will send nothing.");
        }

        m_udpClient.Close(); // this might need a super flag or something to avoid doing anything else when this is activated.
    }

    //CallBack, it's automatically called when the socket receives anything.
    private void recv(IAsyncResult res)
    {
        //Debug.Log("Entered recv function, Callback for the BeginReceive function.");
       

        IPEndPoint RemoteIpEndPoint;
        //Receive messages especifically sent to this client's IP by the server.
        RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0); //WARNING :::::::::: SHOULD THIS BE "ANY" OR SHOULD WE JUST RECEIVE FROM SERVER AND MULTICAST GROUP?
        byte[] received = m_udpClient.EndReceive(res, ref RemoteIpEndPoint);
        
        Debug.Log("The received data was: " + Encoding.UTF8.GetString(received));
        Message pReceivedMessage = new Message(received); //Construct the message with the special contructor which receives an array of bytes.
        //Debug.Log("the Destination Address value received was: " + pReceivedMessage.m_szDestinationAddress);
        if (pReceivedMessage.m_szDestinationAddress == IPAddress.Broadcast.ToString())
        {
            Debug.LogWarning("Message to BROADCAST received from: " + pReceivedMessage.m_szTargetAddress);
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
            else
            {
                //NOTICE: The thing with the comparisons is the NANOSECONDS, which can cause a slight difference between two dates.
                DateTime tmpReceivedDate = DateTime.Parse(pReceivedMessage.m_szMessageContent);//UTC format is mandatory to do the comparison
                Debug.LogWarning("This clients begin date was: " + m_dtBeginDateTime.ToString() + " and the one received from the Broadcast was: " + tmpReceivedDate.ToString() );
                //Debug.LogWarning((tmpReceivedDate < m_dtBeginDateTime) + " y está en UTC?" + tmpReceivedDate.Kind.ToString()); //It is MANDATORY that they are both in the UTC format.
                //Debug.LogWarning("The start date has format: " + m_dtBeginDateTime.Ticks.ToString() + " and the other one is: " + tmpReceivedDate.Ticks.ToString() + " and the comparison result is: " + (tmpReceivedDate.Ticks < m_dtBeginDateTime.Ticks));
                //If the BeginTime is at least 10,000,000 nanoseconds greater, then it must wait for the other client to become server.
                if ((m_dtBeginDateTime.Ticks - tmpReceivedDate.Ticks) > 10000000) // Negative means tmpReceivedDate is prior to m_dtBeginDateTime.
                {  //WE GIVE THE 10,000,000 VALUE AS TOLERANCE FROM ITS OWN TIME, AS THE STRING IS NOT AS PRECISE AS THE DATETIME PER SE.
                    //Not necessarily the one received will become the new server, so we do not make any rushed assumptions, like to record its IP address as server or anything like that.
                    Debug.LogWarning("NOTICE: There's another client trying to become server or looking for one. He got active first, so this client will WAIT for it.");
                    m_fTimeSinceLastResponse = 0.0f;
                }
                //else //so, this client can continue its execution normally, the other one must be the one to wait N seconds longer.
            }
        }
        else if (pReceivedMessage.m_szDestinationAddress == m_szClientIP)
        {
            //Also, reset the time since the last message was received.
            m_fTimeSinceLastResponse = 0.0f; //NOTE:::: Check if it has to be here...
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
            //Also, reset the time since the last message was received.
            m_fTimeSinceLastResponse = 0.0f; //NOTE:::: Check if it has to be here...
            //Then, it was sent to the multicast group.
            Debug.Log("MULTICAST message received, adding it to its message list.");
            m_MessagesList.Add(pReceivedMessage);
            //This is where we start the check for inactivity of the users, inside the Coroutine.
            if (m_pServer != null) //check if this Node is the same one as the server, if it is, then it must check the timeouts.
            {
                //m_pServer.UpdateLastMessageFromAddress(pReceivedMessage.m_szTargetAddress);//check this values.
            }
        }
        else
        {
            Debug.LogWarning("This Client: " + m_szClientIP + " received a message without an specific PURPOSE. Beware of suspicious. The Destination address was: " + pReceivedMessage.m_szDestinationAddress);
        }
        //Finally, begin receiving again.
        m_udpClient.BeginReceive(new AsyncCallback(recv), null);
    }

    private void sendCallback(IAsyncResult res)
    {
        //Debug.Log("Entered sendCallback function, Callback for the BeginSend function.");
        m_udpClient.EndSend(res);
        //Debug.Log("Number of bytes sent by: " + m_szClientIP +  " were: " + m_udpClient.EndSend(res));//The call to "m_udpClient.EndSend(res)" is indispensable!
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
        //Debug.Log("Sending HeartBeat message to Server. This is invoked repeatedly.");
        SendUDPMessage('Y', "HeartBeat", "Empty", m_szServerIP , 10000); //NOTE:: THIS WOULD BE BETTER IF ONLY SENT TO SERVER.
    }

    // Update is called once per frame
    void Update()
    {
        //Check if a new message has arrived.
        //If it has, process it.
        m_fTimeSinceLastResponse += Time.deltaTime;

        if (m_fTimeSinceLastResponse >= m_fMaxTimeSinceLastResponse && bDisconnected == false )
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
                SendUDPMessage('Y', "Begin_Con", DateTime.UtcNow.ToString(), IPAddress.Broadcast.ToString(), 10000);
                //bDisconnected = false; //JUST FOR TESTING.
            }
            else
            {
                //Wait some time and then try to connect to the new leader.
                Debug.LogWarning("Another machine will host the server, it's ID is: " + m_szServerIP);
                //NOTE::: MAKE THE M_SZsERVERIP equal to the IP of the new server machine.
                bDisconnected = false; //??
                m_fTimeSinceLastResponse = 0.0f; // so it doesn't try many times while waiting for the other user to become the server.
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
        /*if (m_MessagesList.Count != 0)
        {
            Debug.LogWarning("There are: " + m_MessagesList.Count + " messages waiting to be processed in the client.");
            Debug.LogWarning("This client knows: " + m_dicKnownClients.Count + " clients at this time.");
        }*/

        //First, check if it is a recent message, or if you can have a 
        //Check if the message belongs to you or the server.
        while (m_MessagesList.Count != 0)
        {
            Message pActualMessage = m_MessagesList[0]; //Get the first element opf the container.
            m_MessagesList.RemoveAt(0); //Then, remove it from the container.
            Debug.Log("Processing message with info: " + pActualMessage.ToString());

            if (pActualMessage.m_szDestinationAddress == m_szMulticastIP && m_pServer != null)
            {
                m_pServer.UpdateLastMessageFromAddress(pActualMessage.m_szTargetAddress);//check this values.
            }

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
                            m_szServerIP = ConnAcceptedContent[3];
                            //he multicast address range is 224.0.0.0 to 239.255.255.255. If you specify an address outside this range or if the router to which 
                            //the request is made is not multicast enabled, UdpClient will throw a SocketException.
                            Debug.LogWarning("A connection has been accepted. This client: " + m_szClientIP + " will join the multicast Group: " + m_szMulticastIP + " in port: " + m_iMulticastPort.ToString() + " The IP of the server is: " + m_szServerIP);
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
                        if (NewUserContent.Length != 2) // see that there must be 2 parameters in the content of this message.
                        {
                            Debug.LogError("The -m_szMessageContent- of a New_User message is not in the correct format. It was: " + pActualMessage.m_szMessageContent);
                        }
                        else
                        {
                            ClientInfo tmpNewClient = new ClientInfo( int.Parse(NewUserContent[0]) , NewUserContent[1]);
                            Debug.LogWarning("A New User has accessed the game, its ID is: " + tmpNewClient.m_iID.ToString() + " and its IP is: " + tmpNewClient.m_szIPAdress);
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
                        Debug.LogWarning("Entered User_Disconnect case.");
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
                                Debug.LogWarning("Removing from Known clients the IP: " +  szMessageContent[0] + "because it is: " + szMessageContent[1]); //print the reason for the disconnection.
                                m_dicKnownClients.Remove(szMessageContent[0]);
                            }
                            else
                            {
                                Debug.LogWarning("Warning, a Client NOT KNOWN to this user is trying to be deleted. Corroborate this, please. That IP was: " + szMessageContent[0]);
                            }
                            Debug.Log("This client now knows: " + m_dicKnownClients.Count + " Clients.");
                        }
                        Debug.LogWarning("Exit User_Disconnect case.");
                    }
                    break;
                case "Known_User":
                    {
                        Debug.Log("Entered Known_User case. Trying to add it to the Known Clients dictionary.");
                        //1- ID of the client. 2- IP related to that ID.
                        string[] szMessageContent = pActualMessage.m_szMessageContent.Split('\t');
                        if (szMessageContent.Length != 2) // see that there must be 3 parameters in the content of this message.
                        {
                            Debug.LogError("The -m_szMessageContent- of a Known_User message is not in the correct format. It was: " + pActualMessage.m_szMessageContent);
                        }
                        else
                        {
                            if (m_dicKnownClients.ContainsKey(szMessageContent[1]))
                            {
                                Debug.Log("This client already knows the ID and IP: " + szMessageContent[0] + " : " + szMessageContent[1]); //Notify which one is "repeated"
                            }
                            else
                            {
                                m_dicKnownClients.Add(szMessageContent[1], new ClientInfo(int.Parse(szMessageContent[0]), szMessageContent[1])); // add the new info.
                            }
                            Debug.Log("This client now knows: " + m_dicKnownClients.Count + " Clients.");
                        }
                        Debug.Log("Exit Known_User case.");
                    }
                    break;
                case "User_Quit":
                    {
                        Debug.LogWarning("Entered User_Quit case on the client.");
                        if (m_dicKnownClients.ContainsKey(pActualMessage.m_szTargetAddress))
                        {
                            //Maybe show an IN-GAME notification about this would be good.
                            Debug.LogWarning("A user has Quit the application. Removing it from the known clients. Its IP was: " + pActualMessage.m_szTargetAddress);
                            m_dicKnownClients.Remove(pActualMessage.m_szTargetAddress);
                        }
                        Debug.LogWarning("Exit User_Quit case on the client.");
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
        m_iServerID = GetLowestID(); //Note that the ServerIP is also set in the GetLowestID function.
        return m_iServerID; //Default value.
    }

    //It also sets the m_szServerIP value.
    private int GetLowestID()
    {
        int iLowest = int.MaxValue;//Start it on the highest possible value.
        if ( m_dicKnownClients.Count == 0 ) //if it's empty, then the client chooses itself by default.
        { iLowest = m_iID; m_szServerIP = m_szClientIP; }
        else
        {
            //Else, we iterate to check the lowest ID available in the dictionary.
            foreach (KeyValuePair<string, ClientInfo> cinfo in m_dicKnownClients)
            {
                if (iLowest > cinfo.Value.m_iID)
                {
                    iLowest = cinfo.Value.m_iID;
                    m_szServerIP = cinfo.Value.m_szIPAdress;
                }
            }
        }
       
        return iLowest;
    }
}
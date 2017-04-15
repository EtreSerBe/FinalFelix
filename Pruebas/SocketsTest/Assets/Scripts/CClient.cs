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

    bool bDisconnected = false;//This value must be modified when a first response of the server/host is received.
    public int m_iID = 0; //ID value used to identify the clients from the Server's perspective.
    public string m_szClientIP = "0.0.0.0";
    public int m_iServerID = -2;

    public string m_szServerIP = "0.0.0.0";

    List<Message> m_MessagesList = new List<Message>();
    HashSet<ClientInfo> m_setKnownClients = new HashSet<ClientInfo>();

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
        m_udpClient = new UdpClient( 10000 );
       
        m_udpClient.EnableBroadcast = true;
        m_udpClient.MulticastLoopback = true; //Necessary so it receives its own messages in the multicast.
        

        try
        {
            m_fTimeSinceLastResponse = 0.0f;
            Debug.Log("Beggining to receive: ");
            m_udpClient.BeginReceive(new AsyncCallback(recv), null);
          
            SendUDPMessage('Y', "Begin_Con", "Empty", IPAddress.Broadcast, 10000);
           
        }
        catch (Exception e)
        {
            Debug.Log("caught exception : " + e.ToString());
        }
    }

    //This is a custom implementation to customize the parameters it uses to receive messages.
    public void BeginReceive(string in_szAddress, int in_iPort)
    {

    }

    //CallBack, it's automatically called when the socket receives anything.
    private void recv(IAsyncResult res)
    {
        Debug.Log("Entered recv function, Callback for the BeginReceive function.");
        IPEndPoint RemoteIpEndPoint;

        if (m_pServer != null || m_iID == 0) //it this one is also server OR it has not been added to the group
        {
            RemoteIpEndPoint = new IPEndPoint(IPAddress.Broadcast, 10000);
            byte[] received = m_udpClient.EndReceive(res, ref RemoteIpEndPoint);
            Debug.Log("The received data BY BROADCAST was: " + Encoding.UTF8.GetString(received));
            Message pReceivedMessage = new Message(received); //Construct the message with the special contructor which receives an array of bytes.
            DispatchMessageToServer(pReceivedMessage); //pass it to the server part.
        }
        if ( m_iID != 0 )
        {
            RemoteIpEndPoint = new IPEndPoint(IPAddress.Parse(m_szMulticastIP), m_iMulticastPort);
            byte[] received = m_udpClient.EndReceive(res, ref RemoteIpEndPoint);
            Debug.Log("The received data BY MULTICAST GROUP was: " + Encoding.UTF8.GetString(received));

            Message pReceivedMessage = new Message(received); //Construct the message with the special contructor which receives an array of bytes.
            //NOTE:::: CHECK IF A CLIENT SHOULD RECEIVE ITS OWN MESSAGES.
            if (pReceivedMessage.m_szSenderID != m_iID.ToString() || m_bAutoReceiveMessages)
            {
                //Now, store it on the message list buffer.
                m_MessagesList.Add(pReceivedMessage);
                Debug.Log("Now there are " + m_MessagesList.Count + " messages waiting to be processed.");
                //Also, reset the time since the last message was received.
                m_fTimeSinceLastResponse = 0.0f;
            }
        }

        //if (m_szClientIP == "0.0.0.0")
        //{
        //    m_szClientIP = ((IPEndPoint)m_udpClient.Client.LocalEndPoint).Address.ToString(); //only the IP, not the ":Port".
        //    Debug.Log("m_szClientIP is: " + m_szClientIP);
        //}

        //We need to begin receiving again, otherwise, it'd only receive once.
        m_udpClient.BeginReceive(new AsyncCallback(recv), null);
    }

    //This one is used to differentiate between messages to the server and messages received by someone who is trying to access the service.
    private void DispatchMessageToServer( Message in_pDispatchMessage )
    {
        //Decode the "time" or something like that so it helps filter old messages.
        //this is to be able to sort the messageList according to how old they are.
        //First, check if it is for the client or for the server (if this machine happens to be the server too).
        if (m_pServer != null && in_pDispatchMessage.m_cIsForServer == 'Y')
        {
            //Then, it means it is destined for the server part, not the client. And the server ir running on this machine.
            m_pServer.m_MessagesList.Add(in_pDispatchMessage);
            Debug.Log("The message just got passed to the SERVER, not to this client.");
        }
        else //else, just execute as any other client would.
        {
            //NOTE: CHECK IF A CLIENT SHOULD RECEIVE ITS OWN MESSAGES.
            if (in_pDispatchMessage.m_szSenderID != m_iID.ToString() || m_bAutoReceiveMessages)
            {
                //Now, store it on the message list buffer.
                m_MessagesList.Add(in_pDispatchMessage);
                Debug.Log("Now there are " + m_MessagesList.Count + " messages waiting to be processed.");
                //Also, reset the time since the last message was received.
                m_fTimeSinceLastResponse = 0.0f;
            }
        }
    }

    private void sendCallback(IAsyncResult res)
    {
        //Debug.Log("Entered sendCallback function, Callback for the BeginSend function.");
        Debug.Log("Number of bytes sent by: " + m_szClientIP +  " were: " + m_udpClient.EndSend(res));
        //Debug.Log("Exit sendCallback function, Callback for the BeginSend function.");
    }


    //I still have my doubts about this one, maybe it should have other parameters as well.
    public void SendUDPMessage(char in_isForServer, string in_szTypeOfMessage, string in_szMessageContent, IPAddress in_Address, int in_iPort  )
    {
        Message NewMessage = new Message(in_isForServer, m_iID.ToString(), m_szClientIP, in_szTypeOfMessage, in_szMessageContent);

        byte[] msgBytes = Encoding.UTF8.GetBytes(NewMessage.ToString());

        Debug.Log("Sending a message to Endpoint: " + in_Address.ToString() + " and port: " + in_iPort );
        //Send a message to the server.
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(in_Address, in_iPort);
        m_udpClient.BeginSend(msgBytes, msgBytes.Length, RemoteIpEndPoint, sendCallback, null);//Do the broadcast.
       
    }

    //facility to be a little more organized.
    public void SendUDPMessage(Message in_pMessage, IPAddress in_Address, int in_iPort)
    {
        SendUDPMessage(in_pMessage.m_cIsForServer, in_pMessage.m_szMessageType, in_pMessage.m_szMessageContent, in_Address, in_iPort);
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
                gameObject.GetComponent<CServer>().StartServer(this, m_setKnownClients); //Send this client's set of known clients to the new server.
                m_pServer = gameObject.GetComponent<CServer>();
                SendUDPMessage('Y', "Begin_Con", "Empty", IPAddress.Broadcast, 10000);
                //bDisconnected = false; //JUST FOR TESTING.
            }
            else
            {
                //Wait some time and then try to connect to the new leader.
                Debug.LogWarning("Another machine will host the server, it's ID is: " + m_szServerIP);

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
                        // 1 is the multicastIP ; 2 is the Multicast port; 3 is the newID for this client.
                        if (ConnAcceptedContent.Length != 3) // see that there must be 3 parameters in the content of this message.
                        {
                            Debug.LogError("The -m_szMessageContent- of a Conn_Accepted message is not in the correct format. It was: " + pActualMessage.m_szMessageContent);
                        }
                        else
                        {
                            m_szMulticastIP = ConnAcceptedContent[0];
                            m_iMulticastPort = int.Parse(ConnAcceptedContent[1]);
                            m_iID = int.Parse(ConnAcceptedContent[2]);
                            //he multicast address range is 224.0.0.0 to 239.255.255.255. If you specify an address outside this range or if the router to which 
                            //the request is made is not multicast enabled, UdpClient will throw a SocketException.
                            Debug.LogWarning("A connection has been accepted. This client: " + m_szClientIP + " will join the multicast Group: " + m_szMulticastIP + " in port: " + m_iMulticastPort.ToString());
                            m_udpClient.JoinMulticastGroup(IPAddress.Parse(m_szMulticastIP)); //NOTE:: CHECK THAT THE PORT IS NOT NECESSARY?
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
                            ClientInfo tmpNewClient = new ClientInfo();
                            tmpNewClient.m_iID = int.Parse(NewUserContent[0]);
                            tmpNewClient.m_szIPAdress = NewUserContent[1];
                            Debug.Log("A New User has accessed the game, its ID is: " + tmpNewClient.m_iID.ToString() + " and its IP is: " + tmpNewClient.m_szIPAdress);
                            m_setKnownClients.Add(tmpNewClient); //we add it to the set of known clients.
                            Debug.Log("This client now knows: " + m_setKnownClients.Count + " Clients.");
                        }
                        Debug.Log("Exit New_User case.");
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

        m_iServerID = m_iID;
        //m_szServerIP = TODO.
        return m_iID; //Default value.
    }

    
}

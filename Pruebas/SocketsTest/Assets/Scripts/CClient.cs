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

    bool bDisconnected = false;//This value must be modified when a first response of the server/host is received.
    public int m_iID = 0; //ID value used to identify the clients from the Server's perspective.
    public string m_szClientIP = "0.0.0.0";
    public int m_iServerID = -2;

    public string m_szServerIP = "0.0.0.0";

    List<Message> m_MessagesList = new List<Message>();
    HashSet<ClientInfo> m_setKnownClients = new HashSet<ClientInfo>();

    CServer m_pServer = null; //null by default, only has a valid value when this client's machine is also executing the server.

    public string m_szMulticastIP = "224.0.0.0"; //set by default, the Server might change it for security purposes.
    public int m_iMulticastPort = 10000;

    //CServer m_ServerReference:

    // Use this for initialization
    void Start()
    {
        //Client uses as receive udp client
        m_udpClient = new UdpClient( 10000 );
        m_szClientIP = m_udpClient.Client.LocalEndPoint.ToString();
        m_udpClient.EnableBroadcast = true;
        

        try
        {
            SendUDPMessage('Y', "Begin_Con", "Empty", IPAddress.Broadcast, 10000);
            
            m_fTimeSinceLastResponse = 0.0f;
            Debug.Log("Beggining to receive: ");
            m_udpClient.BeginReceive(new AsyncCallback(recv), null);
            Debug.Log("Passed the BeginReceive function, now what?");
        }
        catch (Exception e)
        {
            Debug.Log("caught exception : " + e.ToString());
        }
    }

    //CallBack, it's automatically called when the socket receives anything.
    private void recv(IAsyncResult res)
    {
        Debug.Log("Entered recv function, Callback for the BeginReceive function.");
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 10000);
        byte[] received = m_udpClient.EndReceive(res, ref RemoteIpEndPoint);

        

        //Process codes
        Debug.Log("The received data was: " + Encoding.UTF8.GetString(received));

        Message pReceivedMessage = new Message(received); //Construct the message with the special contructor which receives an array of bytes.

        //Decode the "time" or something like that so it helps filter old messages.
        //this is to be able to sort the messageList according to how old they are.

        //First, check if it is for the client or for the server (if this machine happens to be the server too).
        //PENDING****************
        if (m_pServer != null && pReceivedMessage.m_cIsForServer == 'Y')
        {
            //Then, it means it is destined for the server part, not the client. And the server ir running on this machine.
            m_pServer.m_MessagesList.Add(pReceivedMessage);
            Debug.Log("The message just got passed to the SERVER, not to this client.");
        }
        else //else, just execute as any other client would.
        {
            //NOTE: CHECK IF A CLIENT SHOULD RECEIVE ITS OWN MESSAGES.
            if (pReceivedMessage.m_szSenderID != m_iID.ToString())
            { 
                //Now, store it on the message list buffer.
                m_MessagesList.Add(pReceivedMessage);
                Debug.Log("Now there are " + m_MessagesList.Count + " messages waiting to be processed.");
                //Also, reset the time since the last message was received.
                m_fTimeSinceLastResponse = 0.0f;
            }
        }
        
        //We need to begin receiving again, otherwise, it'd only receive once.
        m_udpClient.BeginReceive(new AsyncCallback(recv), null);
    }

    private void sendCallback(IAsyncResult res)
    {
        Debug.Log("Entered sendCallback function, Callback for the BeginSend function.");
        IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 10000);
        Debug.Log("Number of bytes sent by: " + m_szClientIP +  " were: " + m_udpClient.EndSend(res));
        Debug.Log("Exit sendCallback function, Callback for the BeginSend function.");
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

            if (SelectNewServer() == m_iID)
            {
                //Start a server in this machine.
                Debug.LogWarning("This machine will now host the server. Initializing.");
                gameObject.AddComponent<CServer>(); //Add a CServer component to this gameObject, so a new Server starts. 
                gameObject.GetComponent<CServer>().StartServer( this, m_setKnownClients ); //Send this client's set of known clients to the new server.
                SendUDPMessage('Y', "Begin_Con", "Empty", IPAddress.Broadcast, 10000);
                //bDisconnected = false; //JUST FOR TESTING.
            }
            else
            {
                //Wait some time and then try to connect to the new leader.
                Debug.LogWarning("Another machine will host the server, it's ID is: " + m_szServerIP );
                
            }
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

            Debug.Log("Processing message with contents: " + pActualMessage.ToString());

            //Check which type of message is.
            switch (pActualMessage.m_szMessageType)
            {
                case "Begin_Con": //Which is begin connection.
                    {
                        //he multicast address range is 224.0.0.0 to 239.255.255.255. If you specify an address outside this range or if the router to which 
                        //the request is made is not multicast enabled, UdpClient will throw a SocketException.
                        Debug.Log("A new client has requested to begin connection to this server.");
                        ClientInfo tmpInfo = new ClientInfo();
                        tmpInfo.m_iID = int.Parse(pActualMessage.m_szSenderID); //Serves as a casting to int.

                        tmpInfo.m_szIPAdress = pActualMessage.m_szTargetAddress;  //The position of the bytes corresponding to the IP Address.

                        Debug.Log("That client's IP Address is : " + tmpInfo.m_szIPAdress);

                        //If it has not been registered as a connected client, then, add it to the list.
                        if (IsNewIPAddress(tmpInfo))
                        {
                            //This means it is new to this server. Check if it was already registered to another one. Do it by checking its ID.
                            if (tmpInfo.m_iID == 0)
                            {
                                //It means it is a completely new client, not registered before. So have to assign a new ID to it.
                                tmpInfo.m_iID = GetNewID();

                                //Now, send a message to that user, confirming its connection was successful. 
                                Debug.LogWarning("A new client is being connected. Notifying all other active users about this. Its ID will be: " + tmpInfo.m_iID);
                            }
                            else // else, it means that the client had an ID assigned by the previous server, but this machine didn't know about it. 
                            {
                                //So, we just add it to the set, without incrementing the Current ID.
                                //First, check if it is a lower ID than the Highest one this server knows.
                                if (tmpInfo.m_iID > m_iCurrentID)
                                    Debug.LogError("A client with an ID higher than the actual known highest ID has arrived, please corroborate this.");

                                Debug.Log("An old Client has been added to the hash.");
                                //In any case, add it.
                            }

                            //Add it to the Set of ClientsInfo.
                            m_setClientInfo.Add(tmpInfo);

                            //SEND TO EVERYONE ON THE GROUP.
                            /*********/
                            //TO DO 
                            Debug.Log("Sending to everyone else the info about the recently connected client.");
                            //PUT THE SEND COMMAND TO THE Multicast Group.

                        }
                        else
                        {
                            Debug.Log("Someone who is already connected tried to connect. Its address is: " + tmpInfo.m_szIPAdress);
                        }
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

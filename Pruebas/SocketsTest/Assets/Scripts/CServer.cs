using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System;


/*The actual structure standard for the messages to be passed to communicate in the application.*/
public struct Message
{
    public char m_cIsForServer;                    //Tipo de usuario (cliente o server)
    public string m_szSenderID;                    //ID de quien lo envía
    public string m_szTargetAddress;               //IP de a quién va dirigido ?
    public string m_szMessageType;                 //tipo de mensaje (iniciar conexión, dejar conexión, update, etc.)
    public string m_szDestinationAddress;          //IP Address to which the message is to be sent.
    public string m_szMessageContent;              //Contenido del mensaje

    public Message(byte[] in_receivedBytes)
    {
        string tmpString = Encoding.UTF8.GetString(in_receivedBytes);
        Debug.Log("Constructing a Message with: " + tmpString);
        string[] tmpValuesArray = tmpString.Split("\t".ToCharArray(),  6); //Gives us 5 parts so we can use each one as one of the variables of this object.
        if ( tmpValuesArray.Length != 6 )
        {
            //Then, the received bytes did not have the correct format (which is, containing 4 tabs '\t' to addecuately make the split).
            Debug.LogError("A message was constructed without the correct information. it was:  " + tmpString);
            m_cIsForServer = '0';
            m_szSenderID = null;
            m_szTargetAddress = null;
            m_szMessageType = null;
            m_szDestinationAddress = null;
            m_szMessageContent = null;
            return; //return to exit the constructor.
        }

        //Else, everything is formated correctly, so we can create our message object easily.
        m_cIsForServer = tmpValuesArray[0].ToCharArray()[0]; //By convention, this is where we will store this information.
        m_szSenderID = tmpValuesArray[1];
        m_szTargetAddress = tmpValuesArray[2];
        m_szMessageType = tmpValuesArray[3];
        m_szDestinationAddress = tmpValuesArray[4];
        m_szMessageContent = tmpValuesArray[5];

        Debug.Log("A message was created, and it has the values: " + ToString());//Debug to see what does it say.
    }

    public Message(char in_isForServer, string in_szSenderID, string in_szTargetIPAddress, string in_szTypeOfMessage, string in_szDestinationAddress, string in_szMessageContent)
    {
        m_cIsForServer = in_isForServer; //By convention, this is where we will store this information.
        m_szSenderID = in_szSenderID;
        m_szTargetAddress = in_szTargetIPAddress;
        m_szMessageType = in_szTypeOfMessage;
        m_szDestinationAddress = in_szDestinationAddress;
        m_szMessageContent = in_szMessageContent;
    }

    //Overrided method to obtain a string with exactly the format desired to make the functioning better and easier.
    public override string ToString()
    {
        string tmpString;
        tmpString = m_cIsForServer.ToString() + '\t' + m_szSenderID + '\t' + m_szTargetAddress + '\t' + m_szMessageType + '\t' + m_szDestinationAddress + '\t' + m_szMessageContent;
        return tmpString;
    }

};

public struct ClientInfo
{
    public int m_iID;
    public String m_szIPAdress;
   
    //Constructor which by defaults set the TimeSinceLastMessage value to DateTime.Now.
    public ClientInfo( int in_iID, string in_szIPAddress )
    {
        m_iID = in_iID;
        m_szIPAdress = in_szIPAddress;
        //m_dtTimeSinceLastMessage = DateTime.Now;
    }

    public override int GetHashCode()
    {
        return m_szIPAdress.GetHashCode();
    }
    //Need anything else?
};

public struct ClientTimers
{
    public DateTime m_dtTimeSinceLastMessage; //used to decide if that client must be considered INACTIVE.
    public DateTime m_dtTimeSinceLastHeartBeat; //used to decide if that client must be considered DISCONNECTED.

    public ClientTimers( bool bTrue = true )
    {
        m_dtTimeSinceLastMessage = DateTime.Now;
        m_dtTimeSinceLastHeartBeat = DateTime.Now;
    }

    public void SetTimeSinceLastMessage(DateTime in_Time)
    {
        m_dtTimeSinceLastMessage = in_Time;
    }
    public void SetTimeSinceLastHeartBeat(DateTime in_Time)
    {
        m_dtTimeSinceLastHeartBeat = in_Time;
    }
};

public class CServer : MonoBehaviour
{
    public string m_szMulticastIP = "223.0.0.0"; //default INVALID Multicast IP for this program.
    public int m_iMulticastPort = 10000;
    public float m_fMaxTimeSinceLastHeartBeat = 10.0f;// Value which, when exceeded, will cause the server to disconnect the given user.

    public CClient m_pClientRef = null;
    Dictionary<string, ClientInfo> m_dicKnownClients = new Dictionary<string, ClientInfo>();
    public Dictionary<string, ClientTimers> m_dicClientTimers = new Dictionary<string, ClientTimers>(); //This one is exclusive to the server, so it is separated from ClientInfo, which was previously joint.
    public List<Message> m_MessagesList = new List<Message>();

    private int m_iCurrentID = 1;

    private int GetNewID()
    {
        m_iCurrentID++;
        return m_iCurrentID;
    }

    private int GetHighestID()
    {
        int iHighest = 0;
        foreach( KeyValuePair<string,  ClientInfo> cinfo in m_dicKnownClients)
        {
            iHighest = iHighest < cinfo.Value.m_iID ? cinfo.Value.m_iID : iHighest; //If in one line to update the actual iHighest.
        }

        return iHighest;
    }

    //This coroutine is used by the Server Machine to decide if a user has gone DISCONNECTED.
    public IEnumerator CheckHeartBeatCoroutine(string in_szIPAddress)
    {
        yield return new WaitForSeconds(m_fMaxTimeSinceLastHeartBeat);
        //Now, we dow our checking.
        ClientTimers OutClientInfo;
        if (!m_dicClientTimers.TryGetValue(in_szIPAddress, out OutClientInfo))
        {
            Debug.LogWarning("Warning, checking the Timeout HeartBeat for a client which is not registered on the KnownClients Timers Dictionary.");
        }

        System.TimeSpan TimeDiff = OutClientInfo.m_dtTimeSinceLastMessage.Subtract(System.DateTime.Now);
        if ((TimeDiff.Seconds >= m_fMaxTimeSinceLastHeartBeat) //if more
            || TimeDiff.Minutes > 0) //if more than a minute has elapsed since last heartbeat, disconnect that user.
        {
            //TO DO: Send the notification to all users.
            //Then, this user is to be considered Inactive, and therefore removed from the group. //also remove it from the Timers info.
            Debug.LogWarning("Warning, the user with IP: " + in_szIPAddress + " is NON-RESPONSIVE TO HEARTBEAT and will be removed from the group.");
            // NOTE::: VERIFY IF IT IS NECESSARY TO REMOVE IT FROM THE SERVER'S m_dicKnownClients, or only on the client's one. I HAVE MY DOUBTS ABOUT THIS.
            if (m_dicKnownClients.ContainsKey(in_szIPAddress))
            {
                m_dicKnownClients.Remove(in_szIPAddress);
            }
            else
            {
                Debug.LogWarning("Warning, tried to remove an IP which is not Known to the Server. IP was: " + in_szIPAddress);
            }
            //In this point, we've already checked if this IP is in the Timers Dictionary.
            m_dicClientTimers.Remove(in_szIPAddress);
            Debug.Log("The removal of the client was: SUCCESSFUL.");

            //Then, communicate the removal to the Multicast Group.
            m_pClientRef.SendUDPMessageToGroup('N', "User_Disconnect", in_szIPAddress + '\t' + "No_HeartBeat"); //The content is the address of the user to be removed.
        }
    }

    //This coroutine is used by the Server Machine to decide if a user has gone Inactive.
    public IEnumerator CheckTimeSinceLastMessageCoroutine(string in_szIPAddress)
    {
        yield return new WaitForSeconds(m_pClientRef.m_iMaxMinutesSinceLastResponse * 60.0f + (float)m_pClientRef.m_iMaxSecondsSinceLastResponse);
        //Now, we dow our checking.
        ClientTimers OutClientInfo;
        if (!m_dicClientTimers.TryGetValue(in_szIPAddress, out OutClientInfo))
        {
            Debug.LogWarning("Warning, checking the Timeout for a client which is not registered on the KnownClients Dictionary.");
        }

        System.TimeSpan TimeDiff = OutClientInfo.m_dtTimeSinceLastMessage.Subtract(System.DateTime.Now);
        if ((TimeDiff.Minutes == m_pClientRef.m_iMaxMinutesSinceLastResponse && TimeDiff.Seconds >= m_pClientRef.m_iMaxSecondsSinceLastResponse) //if more
            || TimeDiff.Minutes > m_pClientRef.m_iMaxMinutesSinceLastResponse)
        {
            //TO DO: Send the notification to all users.
            //Then, this user is to be considered Inactive, and therefore removed from the group.
            Debug.LogWarning("Warning, the user with IP: " + in_szIPAddress + " is INACTIVE IN GAME and will be removed from the group.");
            // NOTE::: VERIFY IF IT IS NECESSARY TO REMOVE IT FROM THE SERVER'S m_dicKnownClients, or only on the client's one. I HAVE MY DOUBTS ABOUT THIS.
            if (m_dicKnownClients.ContainsKey(in_szIPAddress))
            {
                m_dicKnownClients.Remove(in_szIPAddress);
            }
            else
            {
                Debug.LogWarning("Warning, tried to remove an IP which is not Known to the Server. IP was: " + in_szIPAddress);
            }
            //In this point, we've already checked if this IP is in the Timers Dictionary.
            m_dicClientTimers.Remove(in_szIPAddress);
            Debug.Log("The removal of the client was: SUCCESSFUL.");

            //Then, communicate the removal to the Multicast Group.
            m_pClientRef.SendUDPMessageToGroup('N', "User_Disconnect", in_szIPAddress + '\t' + "Inactive"); //The content is the address of the user to be removed.
        }
    }

    public void UpdateLastMessageFromAddress(string in_szAddress)
    {
        //NOTE::: CHECK THAT THE ADDRESS IS CORRECT, MAYBE WE HAVE TO RETRIEVE IT FROM THE DICTIONARY!"!!!!!
        Debug.Log("Server is resseting the INACTIVITY timeout for client with IP: " + in_szAddress);
        //Stop the actual coroutine Timeoutn for this address, so we can start a new one.
        StopCoroutine(CheckTimeSinceLastMessageCoroutine(in_szAddress));
        m_dicClientTimers[in_szAddress].SetTimeSinceLastMessage(DateTime.Now); //A function was needed, as Dictionary can be a real Dick about it.
          //Then, we manage the times since this user last sent a message.
        StartCoroutine(CheckTimeSinceLastMessageCoroutine(in_szAddress));
    }

    public void UpdateLastHeartBeatFromAddress(string in_szAddress)
    {
        //NOTE::: CHECK THAT THE ADDRESS IS CORRECT, MAYBE WE HAVE TO RETRIEVE IT FROM THE DICTIONARY!"!!!!!
        Debug.Log("Server is resseting the HEARTBEAT timeout for client with IP: " + in_szAddress);
        //Stop the actual coroutine Timeoutn for this address, so we can start a new one.
        StopCoroutine(CheckHeartBeatCoroutine(in_szAddress));
        m_dicClientTimers[in_szAddress].SetTimeSinceLastHeartBeat(DateTime.Now); //A function was needed, as Dictionary can be a real Dick about it.
        //Then, we manage the times since this user last sent a message.
        StartCoroutine(CheckHeartBeatCoroutine(in_szAddress));
    }

    //Starts this component as the active server.
    public void StartServer( CClient in_pClientRef,  Dictionary<string , ClientInfo> in_refKnownClients)
    {
        Debug.Log("Entered StartServer function.");
        m_pClientRef = in_pClientRef;//Copy the reference, so it can be later used to use its socket to send and receive.

        if (m_pClientRef.m_szMulticastIP == "223.0.0.0") //if it is equal to thge default Multicast address, then it's a complete new server-
        {
            //We generate a new one, so they don't use a "pre-known" IP address, so we can protect the system a little more.
            int[] iRand = new int[4];
            iRand[0] = UnityEngine.Random.Range(224, 239);
            iRand[1] = UnityEngine.Random.Range(0, 255);
            iRand[2] = UnityEngine.Random.Range(0, 255);
            iRand[3] = UnityEngine.Random.Range(0, 255);
            m_szMulticastIP = iRand[0].ToString() + "." + iRand[1].ToString() + "." + iRand[2].ToString() + "." + iRand[3].ToString(); //new composed address.
            m_iMulticastPort = 10000;// UnityEngine.Random.Range(10000, 11000);//some range of possible ports.
        }
        else //else, we adopt the previously stablished Address and port of the multicast group.
        {
            m_szMulticastIP = in_pClientRef.m_szMulticastIP;
            m_iMulticastPort = 10000;//NOTE: MUST BE THE SAME AS WHEN YOU BIND/CREATE THE SOCKETS// in_pClientRef.m_iMulticastPort;
        }

        Debug.LogWarning("The IP Address and port for the Multicast group are:  " + m_szMulticastIP + " : " + m_iMulticastPort.ToString());
        //m_dicKnownClients.Clear();//Clear it from any possible trash from other executions o something else.//Shouldn't be necessary.

        m_dicKnownClients = new Dictionary<string, ClientInfo>(in_refKnownClients); //Done this way so it copies the elements of that set into its own container.
        m_iCurrentID = GetHighestID(); //The highest  ID given in the elements of "m_setClientInfo". So we know which is the next ID to generate.
        Debug.Log("Exit of StartServer function.");
    }

    void Update()
    {
        ProcessMessages(); //Just call ProcessMessages for now, if there is anything else, put it here.
    } 

    private void ProcessMessages()
    {
        
        while (m_MessagesList.Count != 0)
        {
            Message pActualMessage = m_MessagesList[0]; //Get the first element opf the container.
            m_MessagesList.RemoveAt(0); //Then, remove it from the container.

            Debug.Log("Processing message with contents: " + pActualMessage.ToString());
            ClientInfo tmpInfo = new ClientInfo();
            tmpInfo.m_iID = int.Parse(pActualMessage.m_szSenderID); //Serves as a casting to int.
            tmpInfo.m_szIPAdress = pActualMessage.m_szTargetAddress;  //The position of the bytes corresponding to the IP Address.

            Debug.Log("SERVER SAYS: That client's IP Address is : " + tmpInfo.m_szIPAdress);

            //Check which type of message is.
            switch (pActualMessage.m_szMessageType)
            {
                case "Begin_Con": //Which is begin connection.
                    {
                        //he multicast address range is 224.0.0.0 to 239.255.255.255. If you specify an address outside this range or if the router to which 
                        //the request is made is not multicast enabled, UdpClient will throw a SocketException.
                        Debug.Log("Entered Begin_Con case. A new client has requested to begin connection to this server.");

                        //If it has not been registered as a connected client, then, add it to the list.
                        if (IsNewIPAddress(tmpInfo))
                        {
                            //This means it is new to this server. Check if it was already registered to another one. Do it by checking its ID.
                            if (tmpInfo.m_iID == 0)
                            {
                                //It means it is a completely new client, not registered before. So have to assign a new ID to it.
                                tmpInfo.m_iID = GetNewID();
                                Debug.LogWarning("The new ID generated is: " + tmpInfo.m_iID + " for the client with IP" + pActualMessage.m_szTargetAddress);

                                //Send the IP of the multicast group to the new client, so it can join. Also, its new ID for inside the group.
                                //NOTE:::: CHECK IF IT IS NECESSARY TO PASS THE SERVER IP too.
                                Message MulticastAddressMsg = new Message('N', m_pClientRef.m_iID.ToString(),  pActualMessage.m_szTargetAddress, "Conn_Accepted", m_pClientRef.m_szClientIP, (m_szMulticastIP + "\t" + m_iMulticastPort.ToString() + "\t" + tmpInfo.m_iID.ToString() + "\t" + m_pClientRef.m_szServerIP));
                                m_pClientRef.SendUDPMessage(MulticastAddressMsg, /*IPAddress.Parse(pActualMessage.m_szTargetAddress)*/ pActualMessage.m_szTargetAddress, 10000); //send it by the default port: 10000, TO THE SPECIFIC CLIENT.
                                //Also, send all the info about the other clients to this one.
                                SendKnownClientsDic(pActualMessage.m_szTargetAddress);

                                //Now, send a message to that user, confirming its connection was successful. 
                                Debug.LogWarning("A new client is being connected. Notifying all other active users about this. Its ID will be: " + tmpInfo.m_iID);
                                m_pClientRef.SendUDPMessage('N', "New_User", tmpInfo.m_iID.ToString() + "\t" + pActualMessage.m_szTargetAddress, m_szMulticastIP, m_iMulticastPort); //send it to the multicast group.

                            }
                            else // else, it means that the client had an ID assigned by the previous server, but this machine didn't know about it. 
                            {
                                //So, we just add it to the set, without incrementing the Current ID.
                                //First, check if it is a lower ID than the Highest one this server knows.
                                if(tmpInfo.m_iID > m_iCurrentID)
                                    Debug.LogError("A client with an ID higher than the actual known highest ID has arrived, please corroborate this.");

                                Debug.Log("An old Client has been added to the hash.");
                                //In any case, add it.
                            }

                            //Add it to the Set of ClientsInfo.
                            //NOTE:::: Maybe it's not necessary anymore. 15 / april 2017
                            //NOTE:::: WE NEED TO SEND THE WHOLE INFO TO THE NEW CLIENT! 16/4/2017
                            m_dicKnownClients.Add(tmpInfo.m_szIPAdress, tmpInfo);
                            m_dicClientTimers.Add(tmpInfo.m_szIPAdress, new ClientTimers());

                            //SEND TO EVERYONE ON THE GROUP.
                            /*********/
                            //TO DO 
                            Debug.Log("TODO-----Sending to everyone else the info about the recently connected client.");
                            //PUT THE SEND COMMAND TO THE Multicast Group.
                        }
                        else
                        {
                            Debug.Log("Someone who is already connected tried to connect. Its address is: " + tmpInfo.m_szIPAdress);
                        }
                        Debug.Log("Exit Begin_Con case in the server.");
                    }
                    break;
                case "HeartBeat":
                    {
                        Debug.Log("Entered HeartBeat case on the server");
                        if (IsNewIPAddress(tmpInfo))
                        {
                            Debug.LogError("Error, a new user is trying to send a HeartBeat message, it should Begin_Con first.");
                        }
                        else
                        {
                            UpdateLastHeartBeatFromAddress(tmpInfo.m_szIPAdress);//Update the time since we received the last Heartbeat from this address.
                            m_pClientRef.SendUDPMessage('N', "HeartBeatACK", "OK" , tmpInfo.m_szIPAdress /*IPAddress.Parse(m_szMulticastIP)*/, 10000);//10000 port by default
                        }
                        Debug.Log("Exit HeartBeat case on the server");
                    }
                    break;
                default:
                    {
                        Debug.LogError("ERROR: The server received a pActualMessage.m_szMessageType with value: " + pActualMessage.m_szMessageType);
                    }
                    break;
            }

            //In-game message, such as action performed.
            
        }
    }

    //Send many messages to the specified in_szAddress, each one contains the information about one client known to the server.
    private void SendKnownClientsDic( string in_szAddress )
    {
        //NOTE:: This might get Heavy when there are a lot of users.
        Debug.Log("Sending the info about known clients to the address: " + in_szAddress);
        foreach ( KeyValuePair<string , ClientInfo> cl in m_dicKnownClients )
        {
            m_pClientRef.SendUDPMessage('N', "Known_User", cl.Value.m_iID + '\t' + cl.Value.m_szIPAdress, in_szAddress, 10000);
        }
    }

    private bool IsNewIPAddress(ClientInfo in_AddressToCheck)
    {
        if (m_dicKnownClients.ContainsKey(in_AddressToCheck.m_szIPAdress))
        {
            return false;
        }
        //If no address like that has been registered, return true.
        return true;
    }

}
using UnityEngine;
using System.Collections;
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
    public int m_iID = -1; //ID value used to identify the clients from the Server's perspective.
    public String m_szClientIP = "0.0.0.0";
    public int m_iServerID = -2;

    public String m_szServerIP = "0.0.0.0";

    ArrayList m_MessagesList = new ArrayList();


    //CServer m_ServerReference:

    // Use this for initialization
    void Start()
    {
        //Client uses as receive udp client
        m_udpClient = new UdpClient(10000);
        //AddressFamily.InterNetwork
        m_szClientIP = m_udpClient.Client.LocalEndPoint.ToString();

        try
        {
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

        //Decode the "time" or something like that so it helps filter old messages.
        //this is to be able to sort the messageList according to how old they are.


        //Now, store it on the message list buffer.
        m_MessagesList.Add(  received);
        Debug.Log("Now there are " + m_MessagesList.Count + " messages waiting to be processed.");

        //We need to begin receiving again, otherwise, it'd only receive once.
        m_udpClient.BeginReceive(new AsyncCallback(recv), null);
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

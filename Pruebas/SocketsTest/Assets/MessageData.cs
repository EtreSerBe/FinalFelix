using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[System.Serializable]
public class MessageData
{

    public string stringData = "";
    public float mousex = 0;
    public float mousey = 0;
    public int type = 0;

    public static MessageData FromByteArray(byte[] input)
    {
        // Create a memory stream, and serialize.
        MemoryStream stream = new MemoryStream(input);
        // Create a binary formatter.
        BinaryFormatter formatter = new BinaryFormatter();

        MessageData data = new MessageData();
        data.stringData = (string)formatter.Deserialize(stream);
        data.mousex = (float)formatter.Deserialize(stream);
        data.mousey = (float)formatter.Deserialize(stream);
        data.type = (int)formatter.Deserialize(stream);

        return data;
    }

    public static byte[] ToByteArray(MessageData msg)
    {
        // Create a memory stream, and serialize.
        MemoryStream stream = new MemoryStream();
        // Create a binary formatter.
        BinaryFormatter formatter = new BinaryFormatter();

        // Serialize.
        formatter.Serialize(stream, msg.stringData);
        formatter.Serialize(stream, msg.mousex);
        formatter.Serialize(stream, msg.mousey);
        formatter.Serialize(stream, msg.type);

        // Now return the array.
        return stream.ToArray();
    }
}
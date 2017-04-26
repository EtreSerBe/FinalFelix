using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class CGlobals : MonoBehaviour
{
	public const int m_iHostListenPort = 10000;
	public const int m_iHostAnswerPort = 11000;
	public const bool m_bIsEncrypted = true;

	public static byte[] CesarCipher( byte[] in_byteArray )
	{
		byte tmp = 10;
		Debug.Log( "The NON-encrypted data is: " + Encoding.UTF8.GetString( in_byteArray ) );
		for ( int i = 0; i < in_byteArray.Length; i++ )
		{
			in_byteArray[i] = (byte)( in_byteArray[i] + tmp );
		}
		Debug.Log( "The encrypted data is: " + Encoding.UTF8.GetString( in_byteArray ) );
		return in_byteArray;
	}

	public static byte[] CesarCipherDecrypt( byte[] in_byteArray )
	{
		byte tmp = 10;
		Debug.Log( "The encrypted data to Decrypt is: " + Encoding.UTF8.GetString( in_byteArray ) );
		for ( int i = 0; i < in_byteArray.Length; i++ )
		{
			in_byteArray[i] = (byte)( in_byteArray[i] - tmp );
		}
		Debug.Log( "The Decrypted data is: " + Encoding.UTF8.GetString( in_byteArray ) );
		return in_byteArray;
	}

}

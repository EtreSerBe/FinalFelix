using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.IO;
using System.Globalization;

public class CGlobals : MonoBehaviour
{
	public const int m_iHostListenPort = 10000;
	public const int m_iHostAnswerPort = 11000;
	public const bool m_bIsEncrypted = true;
	public static DateTime m_dtGlobalTime;
	public static TimeSpan m_tsDifferenceFromLocalToGlobalTime;

	public static byte[] CesarCipher( byte[] in_byteArray )
	{
		byte tmp = 10;
		//Debug.Log( "The NON-encrypted data is: " + Encoding.UTF8.GetString( in_byteArray ) );
		for ( int i = 0; i < in_byteArray.Length; i++ )
		{
			in_byteArray[i] = (byte)( in_byteArray[i] + tmp );
		}
		//Debug.Log( "The encrypted data is: " + Encoding.UTF8.GetString( in_byteArray ) );
		return in_byteArray;
	}

	public static byte[] CesarCipherDecrypt( byte[] in_byteArray )
	{
		byte tmp = 10;
		//Debug.Log( "The encrypted data to Decrypt is: " + Encoding.UTF8.GetString( in_byteArray ) );
		for ( int i = 0; i < in_byteArray.Length; i++ )
		{
			in_byteArray[i] = (byte)( in_byteArray[i] - tmp );
		}
		//Debug.Log( "The Decrypted data is: " + Encoding.UTF8.GetString( in_byteArray ) );
		return in_byteArray;
	}


	public static string Vec3ToString( Vector3 in_vToParse )
	{
		string szValues = in_vToParse.x.ToString() + '\t' + in_vToParse.y.ToString() + '\t' + in_vToParse.z.ToString();
		return szValues;
	}

	public static DateTime GetNistTime( )
	{

		var myHttpWebRequest = (HttpWebRequest)WebRequest.Create("http://www.google.com");
		var response = myHttpWebRequest.GetResponse();
		string todaysDates = response.Headers["date"];
		m_dtGlobalTime = DateTime.Parse(todaysDates).AddMilliseconds(DateTime.UtcNow.Millisecond);
		m_tsDifferenceFromLocalToGlobalTime = m_dtGlobalTime.Subtract(DateTime.UtcNow);
		return m_dtGlobalTime;
		//return DateTime.ParseExact( todaysDates,
		//						   "ddd, dd MMM yyyy HH:mm:ss.fff 'GMT'",
		//						   CultureInfo.InvariantCulture.DateTimeFormat,
		//						   DateTimeStyles.AssumeUniversal );
	}

	public static DateTime GetGlobalTime( )
	{
		return DateTime.UtcNow.Add( CGlobals.m_tsDifferenceFromLocalToGlobalTime );
	}

	public static string GetGlobalTimeString( )
	{
		return DateTime.UtcNow.Add( CGlobals.m_tsDifferenceFromLocalToGlobalTime ).ToString( "MM/dd/yyyy hh:mm:ss.fff" );
	}



	//public static DateTime GetNISTTime( )
	//{
	//	Debug.LogWarning("Entered to check global time.");
	//	TcpClient client = new TcpClient("time.nist.gov", 13);
	//	using ( StreamReader streamReader = new StreamReader( client.GetStream() ) )
	//	{
	//		string response = streamReader.ReadToEnd();
	//		Debug.LogWarning(response + " the string received from the net was that");

	//		var utcDateTimeString = response.Substring(7, 17);
	//		client.Close();

	//		DateTime localDateTime = DateTime.Parse(utcDateTimeString);
	//		Debug.LogWarning("The parsed date from the network was: " + localDateTime.ToString( "MM/dd/yyyy hh:mm:ss.fff" ) );
	//		return localDateTime;
	//	}


	//	//return DateTime.MinValue;
	//}

	//public static Vector3 ParseToVec3( string in_szToParse )
	//{

	//	return Vector3.zero;
	//}
}

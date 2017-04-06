using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum eGyroMove { NONE, UP, DOWN, LEFT, RIGHT, UP_DOWN, RIGHT_LEFT };

public class Test : MonoBehaviour
{
    // Faces for 6 sides of the cube
    private GameObject[] quads = new GameObject[6];
    
    // Textures for each quad, should be +X, +Y etc
    // with appropriate colors, red, green, blue, etc
    public Texture[] labels;
    public bool m_bDebug;
    public List<eGyroMove> m_lstGyroEvents = new List<eGyroMove>();

    void Start( )
    {
        Input.gyro.enabled = true;
        // make camera solid colour and based at the origin
        if ( m_bDebug )
        {
            GetComponent<Camera>().backgroundColor = new Color( 49.0f / 255.0f, 77.0f / 255.0f, 121.0f / 255.0f );
            GetComponent<Camera>().transform.position = new Vector3( 0, 0, 0 );
            GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;

            // create the six quads forming the sides of a cube
            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);

            quads[0] = createQuad( quad, new Vector3( 1, 0, 0 ), new Vector3( 0, 90, 0 ), "plus x",
                    new Color( 0.90f, 0.10f, 0.10f, 1 ), labels[0] );
            quads[1] = createQuad( quad, new Vector3( 0, 1, 0 ), new Vector3( -90, 0, 0 ), "plus y",
                    new Color( 0.10f, 0.90f, 0.10f, 1 ), labels[1] );
            quads[2] = createQuad( quad, new Vector3( 0, 0, 1 ), new Vector3( 0, 0, 0 ), "plus z",
                    new Color( 0.10f, 0.10f, 0.90f, 1 ), labels[2] );
            quads[3] = createQuad( quad, new Vector3( -1, 0, 0 ), new Vector3( 0, -90, 0 ), "neg x",
                    new Color( 0.90f, 0.50f, 0.50f, 1 ), labels[3] );
            quads[4] = createQuad( quad, new Vector3( 0, -1, 0 ), new Vector3( 90, 0, 0 ), "neg y",
                    new Color( 0.50f, 0.90f, 0.50f, 1 ), labels[4] );
            quads[5] = createQuad( quad, new Vector3( 0, 0, -1 ), new Vector3( 0, 180, 0 ), "neg z",
                    new Color( 0.50f, 0.50f, 0.90f, 1 ), labels[5] );

            GameObject.Destroy( quad );
        }
    }

    // make a quad for one side of the cube
    GameObject createQuad( GameObject quad, Vector3 pos, Vector3 rot, string name, Color col, Texture t )
    {
        Quaternion quat = Quaternion.Euler(rot);
        GameObject GO = Instantiate(quad, pos, quat);
        GO.name = name;
        GO.GetComponent<Renderer>().material.color = col;
        GO.GetComponent<Renderer>().material.mainTexture = t;
        GO.transform.localScale += new Vector3( 0.25f, 0.25f, 0.25f );
        return GO;
    }

    void Update( )
    {
        Quaternion qResult  = GyroToUnity( Input.gyro.attitude );
        Vector3 vResult     = quatToVec(qResult);
        Debug.Log( vResult );
        transform.rotation  = qResult;
    }

    void OnGUI( )
    {
        GUI.skin.label.fontSize = Screen.width / 40;

        GUILayout.Label( "Orientation: " + Screen.orientation );
        GUILayout.Label( "input.gyro.attitude: " + Input.gyro.attitude );
    }

    static Quaternion GyroToUnity( Quaternion q )
    {
        return new Quaternion( q.x, q.y, -q.z, -q.w );
    }

    static Vector3 quatToVec( Quaternion q )
    {
        return new Vector3( q.x, q.y, q.z );
    }
}
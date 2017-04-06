using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public enum eGyroEvents { UP, LEFT, DOWN, RIGHT, UP_DOWN, DOWN_UP, LEFT_RIGHT, RIGHT_LEFT };

public class EventManager : MonoBehaviour
{
    /*< Dictionary of events */
    private Dictionary <eGyroEvents, UnityEvent> m_DictionaryEvents;
    /*< instance of the event manager */
    private static EventManager m_CachedEventManager;

    public static EventManager instance
    {
        get
        {
            if ( !m_CachedEventManager )
            {
                m_CachedEventManager = FindObjectOfType( typeof( EventManager ) ) as EventManager;

                if ( !m_CachedEventManager )
                {
                    Debug.LogError( "There needs to be one active EventManger script on a GameObject in your scene." );
                }
                else
                {
                    m_CachedEventManager.Init();
                }
            }

            return m_CachedEventManager;
        }
    }

    void Init( )
    {
        if ( m_DictionaryEvents == null )
            m_DictionaryEvents = new Dictionary<eGyroEvents, UnityEvent>();
    }

	/*
	 * @Param in_Event is the event type wanted to add a listener.
	 * @Param in_Listener is the action wanted to execute when the event occurs.
	 */
	public static void StartListening( eGyroEvents in_Event, UnityAction in_Listener )
    {
        UnityEvent thisEvent = null;
        if ( instance.m_DictionaryEvents.TryGetValue( in_Event, out thisEvent ) )
        {
            thisEvent.AddListener( in_Listener );
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener( in_Listener );
            instance.m_DictionaryEvents.Add( in_Event, thisEvent );
        }
    }

	/*
	 * @Param in_Event is the event type wanted to stop a listening event.
	 * @Param in_Listener is the action wanted remove of the event listener.
	 */
	public static void StopListening( eGyroEvents in_Event, UnityAction in_Listener )
    {
        if ( m_CachedEventManager == null ) return;
        UnityEvent thisEvent = null;

        if ( instance.m_DictionaryEvents.TryGetValue( in_Event, out thisEvent ) )
            thisEvent.RemoveListener( in_Listener );
    }

	/*
	 * @Param in_Event is the event occurred that's going to dispatch.
	 */
    public static void TriggerEvent( eGyroEvents in_Event )
    {
        UnityEvent thisEvent = null;
        if ( instance.m_DictionaryEvents.TryGetValue( in_Event, out thisEvent ) )
            thisEvent.Invoke();
    }
}
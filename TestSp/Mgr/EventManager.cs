// 事件管理器
using Sproto;
using System;
using System.Collections.Generic;
using System.Text;



public class EventManager
{
    public delegate void EventFunc(object param);

    public static EventManager instance = new EventManager();

    


    private Dictionary<EVENTKEY, List<EventFunc>> m_eventDict = new Dictionary<EVENTKEY, List<EventFunc>>();
    public void AddListener(EVENTKEY key,EventFunc func) {
        List<EventFunc> l;
        m_eventDict.TryGetValue(key,out l);
        if (l == null) {
            l = new List<EventFunc>();
            m_eventDict.Add(key, l);
        }
        l.Add(func);
    }

    public void Dispatch(EVENTKEY key,object param) {
        List<EventFunc> l;
        m_eventDict.TryGetValue(key, out l);
        if (l != null) {
            foreach (EventFunc func in l)
            {
                func(param);
            }
        }
    }

    public void RemoveListener(EVENTKEY key, EventFunc func) {
        List<EventFunc> l;
        m_eventDict.TryGetValue(key, out l);
        if (l != null) {
            l.Remove(func);
        } 
    }

    public delegate void EventNet(SprotoTypeBase sp);

    private event EventNet eventNet;

    private Dictionary<EVENTKEY, EventNet> m_dict = new Dictionary<EVENTKEY, EventNet>();

    public EventManager() {
        RegisterEvent();
    }

    private void RegisterEvent() {
        m_dict.Add(EVENTKEY.net_Recv, eventNet);
    }

    public void FireEvent(EVENTKEY key,SprotoTypeBase sp) {
        m_dict[key]?.Invoke(sp);
    }

    public void AddHandler(EVENTKEY key, EventNet handler) {
        EventNet evt = m_dict[key];
        if (evt != null) {
            evt += handler;
        }
    }
}


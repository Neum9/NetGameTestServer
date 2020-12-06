// 事件管理器
using Sproto;
using System;
using System.Collections.Generic;
using System.Text;



public class EventManager
{

    public static EventManager instance = new EventManager();

    public delegate void EventNet(SprotoTypeBase sp, long session, int tag);

    private event EventNet eventNet;

    private Dictionary<EVENTKEY, EventNet> m_dict = new Dictionary<EVENTKEY, EventNet>();

    public EventManager() {
        RegisterEvent();
    }

    private void RegisterEvent() {
        m_dict.Add(EVENTKEY.net_Recv, eventNet);
    }

    public void FireEvent(EVENTKEY key, SprotoTypeBase sp, long session, int tag) {
        m_dict[key]?.Invoke(sp, session, tag);
    }

    public void AddHandler(EVENTKEY key, EventNet handler) {
        m_dict[key] += handler;
    }
}


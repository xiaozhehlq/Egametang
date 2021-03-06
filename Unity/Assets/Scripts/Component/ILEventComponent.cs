﻿using System;
using System.Collections.Generic;
using Base;

namespace Model
{
	/// <summary>
	/// 事件分发,可以将事件分发到IL和Mono层,性能较EventComponent要差
	/// </summary>
	[EntityEvent(EntityEventId.ILEventComponent)]
	public class ILEventComponent : Component
	{
		private Dictionary<int, List<IInstanceMethod>> allEvents;

		private void Awake()
		{
			this.Load();
		}

		private void Load()
		{
			this.allEvents = new Dictionary<int, List<IInstanceMethod>>();

			Type[] types = DllHelper.GetBaseTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);

				foreach (object attr in attrs)
				{
					EventAttribute aEventAttribute = (EventAttribute)attr;
					IInstanceMethod method = new MonoInstanceMethod(type, "Run");
					if (!this.allEvents.ContainsKey(aEventAttribute.Type))
					{
						this.allEvents.Add(aEventAttribute.Type, new List<IInstanceMethod>());
					}
					this.allEvents[aEventAttribute.Type].Add(method);
				}
			}

			types = DllHelper.GetHotfixTypes();
			foreach (Type type in types)
			{
				object[] attrs = type.GetCustomAttributes(typeof(EventAttribute), false);

				foreach (object attr in attrs)
				{
					EventAttribute aEventAttribute = (EventAttribute)attr;
					IInstanceMethod method = new ILInstanceMethod(type, "Run");
					if (!this.allEvents.ContainsKey(aEventAttribute.Type))
					{
						this.allEvents.Add(aEventAttribute.Type, new List<IInstanceMethod>());
					}
					this.allEvents[aEventAttribute.Type].Add(method);
				}
			}
		}

		public void Run(int type, params object[] param)
		{
			List<IInstanceMethod> iEvents = null;
			if (!this.allEvents.TryGetValue(type, out iEvents))
			{
				return;
			}

			foreach (IInstanceMethod obj in iEvents)
			{
				try
				{
					obj.Run(param);
				}
				catch (Exception err)
				{
					Log.Error(err.ToString());
				}
			}
		}
	}
}
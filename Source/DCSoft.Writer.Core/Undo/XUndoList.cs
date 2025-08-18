﻿using System;

namespace DCSoft.Writer.Undo
{
	/// <summary>
	/// 撤销操作列表记录类型
	/// </summary>
	/// <remarks>本类型是DCSoft内部使用,应用程序不适合使用本类型</remarks>
	[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = false)]
	public class XUndoList:IDisposable
	{
		public virtual void Dispose() 
		{
			if(this._Values != null )
			{
				foreach(var item in this._Values)
				{
					item.Dispose();
				}
				this._Values.Clear();
				this._Values = null;
			}
			if(this.myLogItems != null )
			{
				foreach( var item in this.myLogItems)
				{
					item.Dispose();
				}
				this.myLogItems.Clear();
				this.myLogItems = null;
			}
		}
		private System.Collections.Generic.List<IUndoable> _Values = new System.Collections.Generic.List<IUndoable>();
        [System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
        public int Count
        {
			get
            {
				return this._Values.Count;
            }
        }
		/// <summary>
		/// 列表中最大成员个数
		/// </summary>
		protected int intMaxCount = 100;
		/// <summary>
		/// 列表中最大成员个数
		/// </summary>
		public int MaxCount
		{
			get { return intMaxCount; }
			set { intMaxCount = value; }
		}

		/// <summary>
		/// 当前操作对象的序号
		/// </summary>
		protected int intPosition = 0;
        /// <summary>
        /// 返回指定序号的对象
        /// </summary>
        public IUndoable this[int index]
		{
			get
			{
				return this._Values[index];
			}
		}
		/// <summary>
		/// 当前对象
		/// </summary>
		public IUndoable Current()
		{
			if (intPosition >= 0 && intPosition < this.Count)
				return this._Values[intPosition];
			else
				return null;
		}

		/// <summary>
		/// 列表状态发生改变事件,应用系统可以响应这个事件来更新用户界面
		/// </summary>
		public System.EventHandler StateChanged = null;

		/// <summary>
		/// 触发列表状态发生改变事件
		/// </summary>
		protected virtual void OnStateChanged()
		{
			if (StateChanged != null)
			{
				StateChanged(this, null);
			}
		}

		/// <summary>
		/// 执行一次撤销操作
		/// </summary>
		/// <remarks>是否执行了操作</remarks>
		public bool Undo(XUndoEventArgs args)
		{
			if (intPosition < 0)
			{
				return false;
			}
			bool back = bolLock;
			this.bolLock = true;
			try
			{
				IUndoable undo = this.Current();
				intPosition--;
				if (undo != null)
				{
					undo.Undo(args);
					OnStateChanged();
					return true;
				}
			}
			finally
			{
				bolLock = back;
			}
			return false;
		}

		/// <summary>
		/// 执行一次重复操作
		/// </summary>
		public bool Redo(XUndoEventArgs args)
		{
			if (intPosition >= this.Count)
			{
				return false;
			}
			bool back = bolLock;
			bolLock = true;
			try
			{
				intPosition++;
				IUndoable undo = this.Current();
				if (undo != null)
				{
					undo.Redo(args);
					OnStateChanged();
					return true;
				}
			}
			finally
			{
				bolLock = back;
			}
			return false;
		}

		/// <summary>
		/// 能否执行撤销操作
		/// </summary>
		public bool CanUndo()
		{
			return this.Current() != null;
		}

		/// <summary>
		/// 能否执行重复操作
		/// </summary>
		public bool CanRedo()
		{
			return intPosition + 1 >= 0 && intPosition + 1 < this.Count;
		}
        [System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
        public virtual void Clear()
		{
			if (this._Values != null)
			{
				this._Values.Clear();
			}
			intPosition = 0;
			bolLock = false;
			myLogItems = null;
			OnStateChanged();
		}

		private bool bolLock = false;
		/// <summary>
		/// 锁定列表标记
		/// </summary>
		public bool Lock
		{
			get { return bolLock; }
			set { bolLock = value; }
		}

		private System.Collections.Generic.List<IUndoable> myLogItems = null;
		/// <summary>
		/// 撤销操作信息列表
		/// </summary>
		protected System.Collections.Generic.List<IUndoable> LogItems()
		{
			return myLogItems;
		}

		/// <summary>
		/// 开始登记撤销记录
		/// </summary>
		public virtual bool BeginLog()
		{
			if (bolLock)
			{
				myLogItems = null;
				return false;
			}
			else
			{
				myLogItems = new System.Collections.Generic.List<IUndoable>();
				return true;
			}
		}

		/// <summary>
		/// 取消当前撤销记录
		/// </summary>
		public virtual void CancelLog()
		{
			myLogItems = null;
		}
		/// <summary>
		/// 创建撤销操作信息组对象
		/// </summary>
		/// <returns>创建的对象</returns>
		protected virtual XUndoGroup CreateGroup()
		{
			return new XUndoGroup();
		}

		/// <summary>
		/// 是否强制使用撤销记录组
		/// </summary>
		protected virtual bool ForceUseGroup()
		{
			return false;
		}
		public void RemoveAt(int index )
        {
			this._Values.RemoveAt(index);
        }
		/// <summary>
		/// 完成登记撤销记录
		/// </summary>
		public virtual bool EndLog()
		{
			if (bolLock)
			{
				myLogItems = null;
				return false;
			}
			bool flag = false;
			if (myLogItems != null && myLogItems.Count > 0)
			{
				flag = true;
				for (int iCount = this.Count - 1; iCount > intPosition && iCount >= 0; iCount--)
				{
					this.RemoveAt(iCount);
				}
				if (myLogItems.Count == 1 && this.ForceUseGroup() == false)
				{
					this._Values.Add((IUndoable)myLogItems[0]);
				}
				else
				{
					XUndoGroup group = this.CreateGroup();
					foreach (IUndoable undo in myLogItems)
					{
						group.Add(undo);
					}
					//group.Items.AddRange( myLogItems );
					this._Values.Add(group);
				}
				intPosition = this.Count - 1;
			}
			myLogItems = null;
			if (this.intMaxCount > 0)
			{
				while (this.Count > intMaxCount)
				{
					flag = true;
					this._Values.RemoveAt(0);
					intPosition = this.Count - 1;
				}
			}
			if (flag)
			{
				this.OnStateChanged();
			}
			return flag;
		}

		/// <summary>
		/// 添加一个撤销动作对象到列表中
		/// </summary>
		/// <param name="undo">要添加的撤销动作对象</param>
		[System.Reflection.Obfuscation(Exclude = true, ApplyToMembers = true)]
		public void Add(IUndoable undo)
		{
			if (undo == null || myLogItems == null || myLogItems.Contains(undo))
			{
				return;
			}
			if (myLogItems != null)
			{
				myLogItems.Add(undo);
			}
		}

		/// <summary>
		/// 列表是否处于记录动作的状态,若返回 true 则 Add 方法有效,
		/// 可以记录撤销操作信息,若返回 false 则Add方法无效,不能向列表添加撤销记录
		/// </summary>
		public bool CanLog()
		{
			return bolLock == false && myLogItems != null;
		}
	}//public class XUndoList : System.Collections.CollectionBase
}
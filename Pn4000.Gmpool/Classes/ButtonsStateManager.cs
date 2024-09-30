using System;
using System.Collections.Generic;

namespace WiCAM.Pn4000.Gmpool.Classes
{
	internal class ButtonsStateManager
	{
		private readonly bool _isFromPnControl;

		internal readonly static string BtnCopy;

		internal readonly static string BtnEdit;

		internal readonly static string BtnDelete;

		internal readonly static string BtnSettings;

		internal readonly static string BtnGmpool;

		internal readonly static string BtnExcelXml;

		private readonly Dictionary<string, bool> _dictionary = new Dictionary<string, bool>();

		static ButtonsStateManager()
		{
			ButtonsStateManager.BtnCopy = "BtnCopy";
			ButtonsStateManager.BtnEdit = "BtnEdit";
			ButtonsStateManager.BtnDelete = "BtnDelete";
			ButtonsStateManager.BtnSettings = "BtnSettings";
			ButtonsStateManager.BtnGmpool = "BtnGmpool";
			ButtonsStateManager.BtnExcelXml = "BtnExcelXml";
		}

		public ButtonsStateManager(string state, bool isFromPnControl)
		{
			this._isFromPnControl = isFromPnControl;
			if (string.IsNullOrEmpty(state))
			{
				return;
			}
			int num = 0;
			int num1 = num + 1;
			this.Add(state, ButtonsStateManager.BtnCopy, num);
			int num2 = num1;
			num1 = num2 + 1;
			this.Add(state, ButtonsStateManager.BtnEdit, num2);
			int num3 = num1;
			num1 = num3 + 1;
			this.Add(state, ButtonsStateManager.BtnDelete, num3);
			int num4 = num1;
			num1 = num4 + 1;
			this.Add(state, ButtonsStateManager.BtnSettings, num4);
			int num5 = num1;
			num1 = num5 + 1;
			this.Add(state, ButtonsStateManager.BtnGmpool, num5);
			int num6 = num1;
			num1 = num6 + 1;
			this.Add(state, ButtonsStateManager.BtnExcelXml, num6);
		}

		private void Add(string state, string key, int index)
		{
			if (index >= state.Length)
			{
				return;
			}
			this._dictionary.Add(key, state[index] == '1');
		}

		internal bool FindState(string buttonKey)
		{
			if (!this._isFromPnControl)
			{
				return true;
			}
			if (!this._dictionary.ContainsKey(buttonKey))
			{
				return true;
			}
			return this._dictionary[buttonKey];
		}
	}
}
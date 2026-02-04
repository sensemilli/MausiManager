using System;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

public interface IToolSetupMemory
{
	public class Memory
	{
		public string SavedName { get; set; }

		public string Setup { get; }

		public DateTime Time { get; }

		public string DocName { get; }

		public int MachineNumber { get; }

		public int Version { get; } = 1;

		public Memory(string setup, DateTime time, string docName, int machineNumber)
		{
			Setup = setup;
			Time = time;
			DocName = docName;
			MachineNumber = machineNumber;
		}
	}

	Memory? Load(int machineNumber);

	void Save(int machineNumber, string setup, string docName);

	void AutoSave(int machineNumber, string setup, string docName);
}

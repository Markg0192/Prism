using System;
using System.Collections;

namespace Tekla.Structures
{
	public sealed class ConfigurationSet
	{
		private readonly BitArray storage = new BitArray(Enum.GetValues(typeof(Configuration)).Length, defaultValue: false);

		public static ConfigurationSet Empty => new ConfigurationSet();

		public static ConfigurationSet Full => new ConfigurationSet(Configuration.ConstructionManagement, Configuration.Developer, Configuration.Drafter, Configuration.Educational, Configuration.Full, Configuration.PrecastConcreteDetailing, Configuration.ProjectManagement, Configuration.ReinforcedConcreteDetailing, Configuration.Engineering, Configuration.SteelDetailing, Configuration.Primary, Configuration.Viewer);

		private bool this[Configuration configuration]
		{
			get
			{
				return storage[(int)configuration];
			}
			set
			{
				storage[(int)configuration] = value;
			}
		}

		public ConfigurationSet()
		{
		}

		public ConfigurationSet(params Configuration[] configurations)
		{
			Add(configurations);
		}

		public void Add(Configuration configuration)
		{
			this[configuration] = true;
		}

		public void Add(params Configuration[] configurations)
		{
			foreach (Configuration configuration in configurations)
			{
				this[configuration] = true;
			}
		}

		public bool Contains(Configuration configuration)
		{
			return this[configuration];
		}

		public bool ContainsAll(params Configuration[] configurations)
		{
			foreach (Configuration configuration in configurations)
			{
				if (!Contains(configuration))
				{
					return false;
				}
			}
			return true;
		}

		public bool ContainsAny(params Configuration[] configurations)
		{
			foreach (Configuration configuration in configurations)
			{
				if (Contains(configuration))
				{
					return true;
				}
			}
			return false;
		}

		public void Remove(Configuration configuration)
		{
			this[configuration] = false;
		}

		public void Remove(params Configuration[] configurations)
		{
			foreach (Configuration configuration in configurations)
			{
				this[configuration] = false;
			}
		}
	}
}

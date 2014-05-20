using System;
using Caliburn.Micro;

namespace Meow.Client
{
	public class PropertyChangeDemo : PropertyChangedBase
	{
		public string Name { get; set; }

		public PropertyChangeDemo()
		{
			var disposable = this.RegisterPropertyChanged(x => x.Name, () => System.Console.WriteLine(Name));
			this.RegisterPropertyChanged(x => x.Name, source => System.Console.WriteLine(source.Name));

			// Unregister
			disposable.Dispose();
		}
	}
}
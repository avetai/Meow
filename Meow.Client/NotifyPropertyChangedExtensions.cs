using System;
using System.ComponentModel;
using System.Linq.Expressions;
using Meow.Core;
using Caliburn.Micro;
using Action = System.Action;

namespace Meow.Client
{
	public static class NotifyPropertyChangedExtensions
	{
		 public static IDisposable RegisterPropertyChanged<TNotifyPropertyChanged>(this TNotifyPropertyChanged self, Expression<Func<TNotifyPropertyChanged, object>> selector, Action onPropertChanged, bool notifyWhenEmpty = true)
			 where TNotifyPropertyChanged : class, INotifyPropertyChanged
		 {
			 return self.RegisterPropertyChanged(selector, _ => onPropertChanged(), notifyWhenEmpty);
		 }

		 public static IDisposable RegisterPropertyChanged<TNotifyPropertyChanged>(this TNotifyPropertyChanged self, Expression<Func<TNotifyPropertyChanged, object>> selector, Action<TNotifyPropertyChanged> onPropertChanged, bool notifyWhenEmpty = true)
			 where TNotifyPropertyChanged : class, INotifyPropertyChanged
		 {
			 if (self == null) throw new ArgumentNullException("self");
			 
			 var memberInfo = selector.GetMemberInfo();
			 var propertyName = memberInfo.Name;

			 return self.RegisterPropertyChanged(propertyName, onPropertChanged, notifyWhenEmpty);
		 }

		 public static IDisposable RegisterPropertyChanged<TNotifyPropertyChanged>(this TNotifyPropertyChanged self, string propertyName, Action onPropertChanged, bool notifyWhenEmpty = true)
			 where TNotifyPropertyChanged : class, INotifyPropertyChanged
		 {
			 return self.RegisterPropertyChanged(propertyName, _ => onPropertChanged(), notifyWhenEmpty);
		 }

		 public static IDisposable RegisterPropertyChanged<TNotifyPropertyChanged>(this TNotifyPropertyChanged self, string propertyName, Action<TNotifyPropertyChanged> onPropertChanged, bool notifyWhenEmpty = true)
			 where TNotifyPropertyChanged : class, INotifyPropertyChanged
		 {
			 if (self == null) throw new ArgumentNullException("self");
			 if (propertyName == null) throw new ArgumentNullException("propertyName");
			 if (onPropertChanged == null) throw new ArgumentNullException("onPropertChanged");
			 
			 PropertyChangedEventHandler selfOnPropertyChanged = (sender, args) =>
				 {
					 if (args.PropertyName == propertyName || (notifyWhenEmpty && string.IsNullOrEmpty(args.PropertyName)))
					 {
						 onPropertChanged(self);
					 }
				 };
			 self.PropertyChanged += selfOnPropertyChanged;

			 return new DisposeNotifier(() => self.PropertyChanged -= selfOnPropertyChanged);
		 }
	}
}
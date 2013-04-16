using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Windows;

namespace AssRef.ViewModels
{
	/// <summary>
	/// Base class for all ViewModel classes.
	/// It provides support for property change notifications 
	/// </summary>
	public abstract class ViewModelBase : INotifyPropertyChanged
	{
		#region INotifyPropertyChanged Members

		/// <summary>
		/// Raised when a property on this object has a new value.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Raises this object's PropertyChanged event for all properties
		/// </summary>
		protected virtual void OnPropertyChanged()
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(string.Empty);
				handler(this, e);
			}
		}
		/// <summary>
		/// Raises this object's PropertyChanged event.
		/// </summary>
		/// <param name="propertyName">The property that has a new value. If propertyName is Null of string.Empty, than all properties of a class will be updated.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
#if DEBUG
			if (!string.IsNullOrEmpty(propertyName))
			{
				VerifyPropertyName(propertyName);
			}
#endif

			PropertyChangedEventHandler handler = PropertyChanged;
			if (handler != null)
			{
				var e = new PropertyChangedEventArgs(propertyName);
				handler(this, e);
			}
		}

		/// <summary>
		/// Raises this object's PropertyChanged event.
		/// </summary>
		/// <param name="propertyExpression">The property that has a new value.</param>
		protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
		{
			if (propertyExpression == null)
				throw new ArgumentNullException("propertyExpression");
			if (PropertyChanged == null)
				return;
			var memberExpression = propertyExpression.Body as MemberExpression;
			if (memberExpression == null)
				throw new ArgumentNullException("propertyExpression");
			OnPropertyChanged(memberExpression.Member.Name);
		}

		#endregion

		#region Debugging Aides

		/// <summary>
		/// Warns the developer if this object does not have
		/// a public property with the specified name. This 
		/// method does not exist in a Release build.
		/// </summary>
		[Conditional("DEBUG")]
		[DebuggerStepThrough]
		public void VerifyPropertyName(string propertyName)
		{
            // This is special scenario, when we want to raise change event on all properties
		    if ( string.IsNullOrEmpty( propertyName ) )
		        return;

			// Verify that the property name matches a real,  
			// public, instance property on this object.
			if (TypeDescriptor.GetProperties(this)[propertyName] == null)
			{
				string msg = "Invalid property name: " + propertyName;
				throw new ArgumentException(msg, propertyName);
			}
		}

		#endregion

		#region Design time

		private static bool? _isInDesignMode;

		/// <summary>
		/// Gets a value indicating whether the control is in design mode (running in Blend
		/// or Visual Studio).
		/// </summary>
		public static bool IsInDesignModeStatic
		{
			get
			{
				if (!_isInDesignMode.HasValue)
				{
#if SILVERLIGHT
					_isInDesignMode = DesignerProperties.IsInDesignTool;
#else
					var prop = DesignerProperties.IsInDesignModeProperty;
					_isInDesignMode = (bool)DependencyPropertyDescriptor
						.FromProperty(prop, typeof(FrameworkElement))
						.Metadata.DefaultValue;
#endif
				}

				return _isInDesignMode.Value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the control is in design mode (running under Blend
		/// or Visual Studio).
		/// </summary>
		[SuppressMessage(
			"Microsoft.Performance",
			"CA1822:MarkMembersAsStatic",
			Justification = "Non static member needed for data binding")]
		public bool IsInDesignMode
		{
			get
			{
				return IsInDesignModeStatic;
			}
		}

		#endregion
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Meow.Client
{
	public class SimpleGrid
	{
		#region Simple Row & Column Properties

		public static readonly DependencyProperty RowProperty = DependencyProperty.RegisterAttached("Row", typeof (int), typeof (SimpleGrid), new PropertyMetadata(0, OnRowAttachedPropertyChanged), IsIntValueNotNegative);

		public static void SetRow(UIElement element, int value)
		{
			element.SetValue(RowProperty, value);
		}

		public static int GetRow(UIElement element)
		{
			return (int) element.GetValue(RowProperty);
		}

		private static void OnRowAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var uiElement = (UIElement) d;

			var grid = FindGrid(uiElement);
			if (grid == null)
			{
				return;
			}

			var row = (int) e.NewValue;
			EnsureAndSetRow(grid, row, uiElement);
		}

		private static RowDefinition EnsureAndSetRow(Grid grid, int row, UIElement uiElement)
		{
			EnsureRow(grid, row);

			Grid.SetRow(uiElement, row);

			return grid.RowDefinitions[row];
		}

		private static void EnsureRow(Grid grid, int row)
		{
			while (grid.RowDefinitions.Count <= row)
			{
				grid.RowDefinitions.Add(new RowDefinition());
			}
		}


		public static readonly DependencyProperty ColumnProperty = DependencyProperty.RegisterAttached("Column", typeof(int), typeof(SimpleGrid), new PropertyMetadata(0, OnColumnAttachedPropertyChanged), IsIntValueNotNegative);

		public static void SetColumn(UIElement element, int value)
		{
			element.SetValue(ColumnProperty, value);
		}

		public static int GetColumn(UIElement element)
		{
			return (int) element.GetValue(ColumnProperty);
		}

		private static void OnColumnAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var uiElement = (UIElement) d;

			var grid = FindGrid(uiElement);
			if (grid == null)
			{
				return;
			}

			var column = (int) e.NewValue;
			EnsureAndSetColumn(grid, column, uiElement);
		}

		private static ColumnDefinition EnsureAndSetColumn(Grid grid, int column, UIElement uiElement)
		{
			EnsureColumn(grid, column);

			Grid.SetColumn(uiElement, column);

			return grid.ColumnDefinitions[column];
		}

		private static void EnsureColumn(Grid grid, int column)
		{
			if (column < grid.ColumnDefinitions.Count)
			{
				return;
			}

			while (grid.ColumnDefinitions.Count <= column)
			{
				grid.ColumnDefinitions.Add(new ColumnDefinition());
			}

			foreach (var child in grid.Children.OfType<UIElement>().Where(x => GetCellRole(x) == CellRole.SpanToEnd))
			{
				var currnetChildColumn = Grid.GetColumn(child);
				Grid.SetColumnSpan(child, grid.ColumnDefinitions.Count - currnetChildColumn);
			}
		}

		#endregion

		#region CellRole Property

		public static readonly DependencyProperty CellRoleProperty = DependencyProperty.RegisterAttached("CellRole", typeof(CellRole), typeof(SimpleGrid), new PropertyMetadata(default(CellRole), OnCellRoleAttachedPropertyChanged));

		public static void SetCellRole(UIElement element, CellRole value)
		{
			element.SetValue(CellRoleProperty, value);
		}

		public static CellRole GetCellRole(UIElement element)
		{
			return (CellRole) element.GetValue(CellRoleProperty);
		}

		private static void OnCellRoleAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var cellRole = (CellRole)e.NewValue;
			if (cellRole == CellRole.None)
			{
				return;
			}

			var uiElement = (UIElement)d;

			var grid = FindGrid(uiElement);
			if (grid == null)
			{
				return;
			}

			var shouldFit = cellRole == CellRole.FitNext || cellRole == CellRole.FitOmega;

			var currentRow = GetCurrentRow(grid);
			var currentColumn = GetCurrentColumn(grid);

			EnsureAndSetRow(grid, currentRow, uiElement);
			var columnDefinition = EnsureAndSetColumn(grid, currentColumn, uiElement);

			if (shouldFit)
			{
				columnDefinition.Width = GridLength.Auto;
			}

			switch (cellRole)
			{
				case CellRole.FitNext:
				case CellRole.Next:
					SetCurrentColumn(grid, currentColumn + 1);
					break;
				case CellRole.FitOmega:
				case CellRole.Omega:
				case CellRole.SpanToEnd:
					if (cellRole == CellRole.SpanToEnd)
					{
						Grid.SetColumnSpan(uiElement, grid.ColumnDefinitions.Count - currentColumn);
					}

					SetCurrentRow(grid, currentRow + 1);
					SetCurrentColumn(grid, 0);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			var columnSpan = GetColumnSpan(uiElement);
			if (columnSpan > 1)
			{
				ApplyColumnSpan(uiElement, columnSpan);
			}
		}


		public static readonly DependencyProperty ColumnSpanProperty = DependencyProperty.RegisterAttached("ColumnSpan", typeof(int), typeof(SimpleGrid), new PropertyMetadata(1, OnColumnSpanAttachedPropertyChanged), IsIntValueGreaterThanZero);

		public static void SetColumnSpan(UIElement element, int value)
		{
			element.SetValue(ColumnSpanProperty, value);
		}

		public static int GetColumnSpan(UIElement element)
		{
			return (int) element.GetValue(ColumnSpanProperty);
		}

		private static void OnColumnSpanAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var uiElement = (UIElement)d;

			var columnSpan = (int)e.NewValue;	
			var cellRole = GetCellRole(uiElement);

			if (cellRole == CellRole.None || columnSpan == 1)
			{
				return;
			}
			
			ApplyColumnSpan(uiElement, columnSpan);
		}


		private static void ApplyColumnSpan(UIElement uiElement, int columnSpan)
		{
			var grid = FindGrid(uiElement);
			if (grid == null)
			{
				return;
			}

			var cellRole = GetCellRole(uiElement);
			if (cellRole != CellRole.FitNext && cellRole != CellRole.Next)
			{
				throw new ArgumentException(
					"Column span can be set only on Next / FitNext cells roles. Use SpanToEnd on it's own instead.");
			}

			Grid.SetColumnSpan(uiElement, columnSpan);

			var currentColumn = GetCurrentColumn(grid);
			SetCurrentColumn(grid, currentColumn + columnSpan - 1);
		}

		#endregion
		
		#region Grid Helper Properties

		public static readonly DependencyProperty CurrentRowProperty = DependencyProperty.RegisterAttached("CurrentRow", typeof (int), typeof (SimpleGrid), new PropertyMetadata(default(int)));

		public static void SetCurrentRow(Grid element, int value)
		{
			element.SetValue(CurrentRowProperty, value);
		}

		public static int GetCurrentRow(Grid element)
		{
			return (int) element.GetValue(CurrentRowProperty);
		}


		public static readonly DependencyProperty CurrentColumnProperty = DependencyProperty.RegisterAttached("CurrentColumn", typeof (int), typeof (SimpleGrid), new PropertyMetadata(default(int)));

		public static void SetCurrentColumn(Grid element, int value)
		{
			element.SetValue(CurrentColumnProperty, value);
		}

		public static int GetCurrentColumn(Grid element)
		{
			return (int) element.GetValue(CurrentColumnProperty);
		}

		#endregion

		#region Simple Row & Column Properties

		public static readonly DependencyProperty RowDefinitionsProperty = DependencyProperty.RegisterAttached("RowDefinitions", typeof(GridLengthCollection), typeof(SimpleGrid), new PropertyMetadata(null, OnRowDefinitionsAttachedPropertyChanged));

		public static void SetRowDefinitions(Grid element, GridLengthCollection value)
		{
			element.SetValue(RowDefinitionsProperty, value);
		}

		public static int GetRowDefinitions(Grid element)
		{
			return (int)element.GetValue(RowDefinitionsProperty);
		}

		private static void OnRowDefinitionsAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var grid = (Grid)d;

			var gridLengthCollection = (GridLengthCollection)e.NewValue;
			
			EnsureRow(grid, gridLengthCollection.Count - 1);
			
			for (int i = 0; i < gridLengthCollection.Count; i++)
			{
				grid.RowDefinitions[i].Height = gridLengthCollection[i];
			}
		}


		public static readonly DependencyProperty ColumnDefinitionsProperty = DependencyProperty.RegisterAttached("ColumnDefinitions", typeof(GridLengthCollection), typeof(SimpleGrid), new PropertyMetadata(null, OnColumnDefinitionsAttachedPropertyChanged));

		public static void SetColumnDefinitions(Grid element, GridLengthCollection value)
		{
			element.SetValue(ColumnDefinitionsProperty, value);
		}

		public static int GetColumnDefinitions(Grid element)
		{
			return (int)element.GetValue(ColumnDefinitionsProperty);
		}

		private static void OnColumnDefinitionsAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var grid = (Grid)d;

			var gridLengthCollection = (GridLengthCollection)e.NewValue;

			EnsureColumn(grid, gridLengthCollection.Count - 1);

			for (int i = 0; i < gridLengthCollection.Count; i++)
			{
				grid.ColumnDefinitions[i].Width = gridLengthCollection[i];
			}
		}

		#endregion

		#region Helpers

		private static Grid FindGrid(DependencyObject d)
		{
			var visual = d as Visual;
			if (visual == null)
			{
				return null;
			}

			var grid = VisualTreeHelper.GetParent(visual) as Grid;
			return grid;
		}


		private static bool IsIntValueNotNegative(object value)
		{
			return (int) value >= 0;
		}

		private static bool IsIntValueGreaterThanZero(object value)
		{
			return (int) value > 0;
		}

		#endregion
	}

	public enum CellRole
	{
		None,
		FitNext,
		Next,
		Omega,
		FitOmega,
		SpanToEnd
	}

	[TypeConverter(typeof(GridLengthCollectionConverter))]
	public class GridLengthCollection : ReadOnlyCollection<GridLength>
	{
		public GridLengthCollection(IList<GridLength> lengths)
			: base(lengths)
		{
		}
	}

	public class GridLengthCollectionConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
			{
				return true;
			}

			return base.CanConvertFrom(context, sourceType);
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType == typeof (string))
			{
				return true;
			}

			return base.CanConvertTo(context, destinationType);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var stringValue = value as string;
			if (stringValue != null)
			{
				return ParseString(stringValue);
			}

			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			var gridLengthCollection = value as GridLengthCollection;
			if (destinationType == typeof (string) && gridLengthCollection != null)
			{
				return ToString(gridLengthCollection);
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}

		private static string ToString(IEnumerable<GridLength> gridLengths)
		{
			var converter = new GridLengthConverter();
			return string.Join(",", gridLengths.Select(v => converter.ConvertToString(v)));
		}

		private static GridLengthCollection ParseString(string stringValue)
		{
			var converter = new GridLengthConverter();
			var gridLengths = stringValue.Split(',').Select(p => (GridLength)converter.ConvertFromString(p.Trim()));
			return new GridLengthCollection(gridLengths.ToArray());
		}
	}
}
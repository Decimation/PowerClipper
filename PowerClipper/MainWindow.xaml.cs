global using Clipboard2 = Novus.Win32.Clipboard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PowerClipper;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
	public MainWindow()
	{
		ClipboardEntries = new ObservableCollection<ClipboardEntry>();
		DataContext      = this;
		InitializeComponent();

		m_cbDispatcher = new DispatcherTimer(DispatcherPriority.Normal)
		{
			Interval  = TimeSpan.FromSeconds(1),
			IsEnabled = true
		};
		m_cbDispatcher.Tick += ClipboardTick;
	}

	private ClipboardEntry m_current;

	public ClipboardEntry Current
	{
		get => m_current;
		set
		{
			if (Equals(value, m_current)) return;
			m_current = value;
			OnPropertyChanged();
		}
	}

	public ObservableCollection<ClipboardEntry> ClipboardEntries { get; private set; }

	private int m_sequence;

	public int Sequence
	{
		get => m_sequence;
		set
		{
			if (value == m_sequence) return;
			m_sequence = value;
			OnPropertyChanged();
		}
	}

	private int m_prevSeq;

	private void ClipboardTick(object? sender, EventArgs e)
	{
		var csq = Clipboard2.SequenceNumber;

		if (m_prevSeq != csq) {
			Sequence = csq;
			var n = ClipboardEntry.Get();

			foreach (var ce in n) {
				if (!ClipboardEntries.Contains(ce)) {
					ClipboardEntries.Add(ce);
				}
			}

			m_prevSeq = csq;
		}
	}

	private readonly DispatcherTimer m_cbDispatcher;

	private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
	{
		e.Handled = true;
	}

	private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
	{
		
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
	{
		if (EqualityComparer<T>.Default.Equals(field, value)) return false;
		field = value;
		OnPropertyChanged(propertyName);
		return true;
	}
}
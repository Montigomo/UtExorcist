using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Threading;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace utexorcist
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{

		#region Constructor & Properties

		private readonly DispatcherTimer dispatcherTimer = new DispatcherTimer();

		public MainWindow()
		{
			InitializeComponent();
			Initialize();

			Resources["Drinks"] = UteSettings.Instance.Values;
			UiDispatcher = Dispatcher.CurrentDispatcher;
			dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
			dispatcherTimer.Interval = TimeSpan.FromMilliseconds(5000);
			dispatcherTimer.Start();
		}

		public Dispatcher UiDispatcher { get; private set; }

		private async void CheckUtorrent()
		{
			await Utorrent.InitializeTask().ContinueWith((t) => UiDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => this.IsEnabled = Utorrent.IsConnected)));
		}

		private void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			CheckUtorrent();
			//CommandManager.InvalidateRequerySuggested();
		}

		//private bool IsLocked { get; set; }

		private void Initialize()
		{
			this.IsEnabled = false;
			if (UteSettings.Instance.Values.Count == 0)
			{
				UteSettings.Instance.Values = Utorrent.InitValues;
				UteSettings.Instance.Save();
			}
		}

		#endregion

		public Task LockUI(bool _flag)
		{
			return Task.Run(() =>
			{
				UiDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => this.IsEnabled = _flag));
			});
		}

		private void buttonWrite_Click(object sender, RoutedEventArgs e)
		{
			WriteValues();
		}

		private void buttonRead_Click(object sender, RoutedEventArgs e)
		{
			ReadValues();
		}

		private async void WriteValues()
		{
			if(Utorrent.IsConnected)
				await LockUI(false).ContinueWith((t1) => Utorrent.WriteValues().ContinueWith((t2) => LockUI(true)));
		}

		private async void ReadValues()
		{
			if (Utorrent.IsConnected)
				await LockUI(false).ContinueWith((t1) => Utorrent.ReadValues().ContinueWith((t2) => LockUI(true)));
		}

		#region Window events

		private void wndMain_Loaded(object sender, RoutedEventArgs e)
		{
			//CheckUtorrent();
		}

		
   // private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
   // {
   //   if (msg == Win32.WM_COPYDATA)
   //   {
			//	// Get the COPYDATASTRUCT struct from lParam.
			//	Win32.COPYDATASTRUCT cds = (Win32.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.COPYDATASTRUCT));

			//	// If the size matches
			//	if (cds.cbData == Marshal.SizeOf(typeof(MsgStruct)))
			//	{
			//		// Marshal the data from the unmanaged memory block to a 
			//		// MsgStruct managed struct.
			//		MsgStruct myStruct = (MsgStruct)Marshal.PtrToStructure(cds.lpData,
			//				typeof(MsgStruct));

			//		// Display the MsgStruct data members.
			//		if (myStruct.Message == "Show Up")
			//		{
			//			this.ShowTop();
			//		}
			//	}
			//}
   //   return IntPtr.Zero;
   // }
		#endregion

		private void wndMain_SourceInitialized(object sender, EventArgs e)
		{
			//IntPtr windowHandle = (new WindowInteropHelper(this)).Handle;
			//HwndSource src = HwndSource.FromHwnd(windowHandle);
			//src.AddHook(new HwndSourceHook(WndProc));

		}

		private void wndMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			//IntPtr windowHandle = (new WindowInteropHelper(this)).Handle;
			//HwndSource src = HwndSource.FromHwnd(windowHandle);
			//src.RemoveHook(new HwndSourceHook(this.WndProc));
		}
	}
}

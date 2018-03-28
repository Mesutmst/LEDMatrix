using System;
using System.Windows;
using System.Windows.Threading;

namespace LedMatrix.Client
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      private MainViewModel _ViewModel;
      private DispatcherTimer _Timer;

      public MainWindow()
      {
         InitializeComponent();
         _ViewModel = new MainViewModel();
         _Timer = new DispatcherTimer();
         _Timer.Interval = TimeSpan.FromMilliseconds(60);
         _Timer.Tick += Timer_Tick;
         DataContext = _ViewModel;
      }

      private void Timer_Tick(object sender, EventArgs e)
      {
         _ViewModel.NextTimerTick();
      }

      private void OpenPort_Click(object sender, RoutedEventArgs e)
      {
         try
         {
            _ViewModel.ToggleSerialPort();
            _ViewModel.ClearPrograms();
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.Message);
         }
      }

      private void SendButton_Click(object sender, RoutedEventArgs e)
      {
         _ViewModel.SendInputText();
      }

      private void RandomButton_Click(object sender, RoutedEventArgs e)
      {
         _ViewModel.InitialiseRandomProgram();
         _Timer.IsEnabled = true;
      }

      private void SinWaveButton_Click(object sender, RoutedEventArgs e)
      {
         _ViewModel.InitialiseSinWaveProgram();
         _Timer.IsEnabled = true;
      }

      private void CountButton_Click(object sender, RoutedEventArgs e)
      {
         _ViewModel.InitialiseCountProgram();
         _Timer.IsEnabled = true;
      }

      private void StopButton_Click(object sender, RoutedEventArgs e)
      {
         _ViewModel.ClearPrograms();
         _Timer.IsEnabled = false;
      }
   }
}

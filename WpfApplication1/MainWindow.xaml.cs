using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private static ConsoleContent dc = new ConsoleContent();
        private static AxKHOpenAPILib.AxKHOpenAPI axKHOA = new AxKHOpenAPILib.AxKHOpenAPI();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = dc;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the interop host control.
            System.Windows.Forms.Integration.WindowsFormsHost host =
                new System.Windows.Forms.Integration.WindowsFormsHost();

            // Assign the ActiveX control as the host control's child.
            host.Child = axKHOA;

            // Add the interop host control to the Grid
            // control's collection of child controls.
            this.grid1.Children.Add(host);

            InputBlock.KeyDown += InputBlock_KeyDown;
            InputBlock.Focus();
        }
        void InputBlock_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                dc.ConsoleInput = InputBlock.Text;
                dc.RunCommand();
                InputBlock.Focus();
                Scroller.ScrollToBottom();
            }
        }
        /// <summary>
        /// getKHOA is a method to reference axKHOA without messing up anything else
        /// </summary>
        /// <returns></returns>
        public static AxKHOpenAPILib.AxKHOpenAPI getKHOA()
        {
            return axKHOA;
        }
        public static ConsoleContent getDc()
        {
            return dc;
        }
    }
    public class ConsoleContent : INotifyPropertyChanged
    {
        string consoleInput = string.Empty;
        ObservableCollection<string> consoleOutput = new ObservableCollection<string>()
            {constants.START};
        public string ConsoleInput
        {
            get
            {
                return consoleInput;
            }
            set
            {
                consoleInput = value;
                OnPropertyChanged("ConsoleInput");
            }
        }

        public ObservableCollection<string> ConsoleOutput
        {
            get
            {
                return consoleOutput;
            }
            set
            {
                consoleOutput = value;
                OnPropertyChanged("ConsoleOutput");
            }
        }

        public void RunCommand()
        {
            if (ConsoleInput != "\n")
                ConsoleOutput.Add("$" + ConsoleInput);

            // do your stuff here.
            searchCommand(ConsoleInput);
            ConsoleInput = String.Empty;
        }
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        public void searchCommand(string input)/////////command search engine////////////////
        {//////////////////////////////must add the command here in order to be operational//
            commands command = new commands();

            switch (ConsoleInput)
            {
                case ("login"):
                    command.commandLoginComm(); // Request communication with Kiwoom HTS by logging in
                    break;
                case ("userinfo"):
                    command.commandUserInfo(); // Show info of logged user
                    break;
                case ("connection"):
                    command.commandConnectionState(); // Show connection state
                    break;
                case ("search"):
                    command.commandStartSearch(); // perform a product search using KNN algorithm
                    break;
                case ("trade"):
                    command.commandTrade();
                    break;
                case ("help"):
                    command.commandHelp();
                    break;

                default: break;
            }
        }
    }
}
static class constants
{
    public static string VERSION="2.1.2 pre-Alpha";
    public static string START = "Red Stock Portal\nversion " + VERSION + "\nTo start program, enter <login> and login to your Kiwoom account";
    public static string UNCONNECTED = "You are not connected. Please login or check your connection";
    public static string CONDITION = "";
}
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
        ConsoleContent dc = new ConsoleContent();
        // Make a OpenAPI instance, couldn't find how to make this global. had to make it public
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
        public static AxKHOpenAPILib.AxKHOpenAPI getKHOA()
        {
            return axKHOA;
        }
    }
    public class ConsoleContent : INotifyPropertyChanged
    {
        string consoleInput = string.Empty;
        ObservableCollection<string> consoleOutput = new ObservableCollection<string>()
            {"To start program, enter <login> and login to your Kiwoom account" };

        //fetching OpenAPI from MainWindow. As I said, couldn't find a way to pass it otherwise
        private static AxKHOpenAPILib.AxKHOpenAPI _axKHOA = MainWindow.getKHOA();

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
            ConsoleOutput.Add(ConsoleInput);

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

        private class commands
        {
            public static void LoginComm() //Login Request communication with the hts.
            {
                long Result;
                Result = _axKHOA.CommConnect();
                if (Result != 0)
                    MessageBox.Show("Login창 열림 Fail");
            }
        }
        public void searchCommand(string input)
        {
            if (ConsoleInput == "login")
                commands.LoginComm();
        }
    }
}
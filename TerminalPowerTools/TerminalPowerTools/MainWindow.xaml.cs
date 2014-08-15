using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace TerminalPowerTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Process _process;

        private string _input;
        private string _output;
        private string _error;

        public MainWindow()
        {
            InitializeComponent();

            var information = new ProcessStartInfo("cmd");
            information.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            information.RedirectStandardError = true;
            information.RedirectStandardInput = true;
            information.RedirectStandardOutput = true;

            information.UseShellExecute = false;
            information.CreateNoWindow = !Debugger.IsAttached;

            var process = new Process();
            process.StartInfo = information;

            process.Start();
            _process = process;

            StartProxy();

            PreviewKeyUp += MainWindow_PreviewKeyUp;
        }

        void StartProxy()
        {
            StartErrorProxy();
            StartOutputProxy();
        }

        //TODO: turn this into a class.
        private async void StartOutputProxy()
        {
            await Task.Run(async delegate()
            {
                while (true)
                {
                    var character = (char)_process.StandardOutput.Read();
                    _output += character;

                    await LogOutput(_output);
                }
            });
        }

        //TODO: turn this into a class.
        private async void StartErrorProxy()
        {
            await Task.Run(async delegate()
            {
                while (true)
                {
                    var character = (char)_process.StandardError.Read();
                    _error += character;

                    await LogError(_error);
                }
            });
        }

        async void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back)
            {
                if (!string.IsNullOrEmpty(_input))
                {
                    _input = _input.Substring(0, _input.Length - 1);
                    await LogOutput(_output + _input);
                }
            }
            else
            {

                char character;
                switch (e.Key)
                {
                    case Key.OemPeriod:
                        character = '.';
                        break;

                    default:
                        character = (char)KeyInterop.VirtualKeyFromKey(e.Key);
                        break;
                }

                var isUpper = (Keyboard.IsKeyToggled(Key.CapsLock) && !Keyboard.IsKeyDown(Key.LeftShift) &&
                               !Keyboard.IsKeyDown(Key.RightShift)) ||
                              (!Keyboard.IsKeyToggled(Key.CapsLock) &&
                               (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)));
                if (isUpper)
                {
                    character = char.ToUpper(character);
                }
                else
                {
                    character = char.ToLower(character);
                }

                if (e.Key == Key.Enter)
                {
                    lock (this) _process.StandardInput.WriteLine(_input);
                    _input = string.Empty;
                }
                else
                {
                    _input += character;
                }

                await LogOutput(_output + _input);

            }

        }

        async Task LogOutput(string text)
        {
            await Dispatcher.InvokeAsync(delegate
            {
                ConsoleOutput.Text = text;
            });
        }

        async Task LogError(string text)
        {
            await Dispatcher.InvokeAsync(delegate
            {
                ConsoleError.Text = text;
            });
        }
    }
}

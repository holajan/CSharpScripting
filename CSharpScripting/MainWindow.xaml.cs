using System;
using System.Windows;
using System.Windows.Controls;
using System.Text;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Scripting;

namespace CSharpScripting
{
    #region ControlWriter
    public class ControlWriter : TextWriter
    {
        private readonly TextBox textbox;

        public ControlWriter(TextBox textbox)
        {
            this.textbox = textbox;
        }

        public override void Write(char value)
        {
            textbox.Text += value;
        }

        public override void Write(string value)
        {
            textbox.Text += value;
        }

        public override Encoding Encoding
        {
            get { return Encoding.ASCII; }
        }
    }
    #endregion

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Console.SetOut(new ControlWriter(txtConsole));

            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Input.Keyboard.Focus(txtScript);
        }

        private async void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            ButtonRun.IsEnabled = false;
            this.Cursor = System.Windows.Input.Cursors.Wait;

            try
            {
                txtConsole.Text = "";

                var scriptRunner = CSharpScript.Create(txtScript.Text, GetScriptOptions()).CreateDelegate();
                await scriptRunner.Invoke();
            }
            catch (Microsoft.CodeAnalysis.Scripting.CompilationErrorException ex)
            {
                txtConsole.Text = txtConsole.Text + (txtConsole.Text.Length == 0 ? "" : Environment.NewLine) + string.Join(Environment.NewLine, ex.Diagnostics);
            }
            catch (Exception ex)
            {
                txtConsole.Text = txtConsole.Text + (txtConsole.Text.Length == 0 ? "" : Environment.NewLine) + string.Join(Environment.NewLine, ex.ToString());
            }
            finally
            {
                ButtonRun.IsEnabled = true;
                this.Cursor = null;
            }
        }

        private Microsoft.CodeAnalysis.Scripting.ScriptOptions GetScriptOptions()
        {
            return Microsoft.CodeAnalysis.Scripting.ScriptOptions.Default
                .WithReferences(typeof(System.Diagnostics.Process).Assembly,            //System Assembly
                                typeof(System.Dynamic.DynamicObject).Assembly,          //System.Core Assembly
                                typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly) //Microsoft.CSharp Assembly
                .WithImports(new[] {
                    "System",
                    "System.IO",
                    "System.Collections.Generic",
                    "System.Diagnostics",
                    "System.Dynamic",
                    "System.Linq",
                    "System.Linq.Expressions",
                    "System.Text",
                    "System.Threading.Tasks"
                });
        }
    }
}
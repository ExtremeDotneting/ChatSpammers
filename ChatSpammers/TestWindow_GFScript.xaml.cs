using System;
using System.Threading.Tasks;
using System.Windows;
using GFScript;

namespace ChatSpammers
{
    /// <summary>
    /// Логика взаимодействия для TestWindow_GFScript.xaml
    /// </summary>
    public partial class TestWindow_GFScript : Window
    {
        string script = "Obj();\n"+
            "Plus(1, 23.071);\n" +
            "Min(30, 200);\n" +
            "Show(\"msg1\");\n" +
            "Show(\"msg2\",\"msg3\",\"msg4\");";
        GFScriptInterpreter gfsInterp = new GFScriptInterpreter();

        public TestWindow_GFScript()
        {
            InitializeComponent();
            textBox.Text = script;
            BindMethods();
        }
        

        void BindMethods()
        {
            gfsInterp.BindMethod(
                "Obj", (obj, args) =>
                {
                    MessageBox.Show(Convert.ToString(obj ?? "null"));
                }
                );
            gfsInterp.BindMethod(
                "Show", (obj, args) =>
                {
                    
                    foreach (var item in args)
                    {
                        MessageBox.Show(Convert.ToString(item));
                    }
                }
                );
            gfsInterp.BindMethod(
                "Plus", (obj, args) =>
                {
                    double res = Convert.ToDouble(args[0]) + Convert.ToDouble(args[1]);
                    MessageBox.Show(Convert.ToString(res));
                }
                );
            gfsInterp.BindMethod(
                "Min", (obj, args) =>
                {
                    double res = Convert.ToDouble(args[0]) - Convert.ToDouble(args[1]);
                    MessageBox.Show(Convert.ToString(res));
                }
                );
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            string textBoxText = textBox.Text.Trim();
            if (textBoxText != "")
                script = textBoxText;
            Task.Run(() =>
            {
                var act=gfsInterp.ParseScript(textBoxText,true);
                act("global object");
            });
        }
    }
}

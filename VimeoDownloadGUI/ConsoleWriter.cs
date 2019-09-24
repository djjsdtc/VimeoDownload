namespace VimeoDownload.GUI
{
    using System.IO;
    using System.Text;
    using System.Windows.Controls;
    using System.Windows.Threading;

    public class ConsoleWriter : TextWriter
    {
        private TextBox textbox;
        private Dispatcher dispatcher;

        public ConsoleWriter(TextBox textbox, Dispatcher dispatcher)
        {
            this.textbox = textbox;
            this.dispatcher = dispatcher;
        }

        public override void Write(char value)
        {
            this.dispatcher.Invoke(() => { textbox.Text += value; });
        }

        public override void Write(string value)
        {
            this.dispatcher.Invoke(() => { textbox.Text += value; });
        }

        /// <inheritdoc />
        public override Encoding Encoding => Encoding.UTF8;
    }
}
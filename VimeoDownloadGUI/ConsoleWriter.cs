namespace VimeoDownload.GUI
{
    using System;
    using System.IO;
    using System.Text;
    using System.Windows.Controls;
    using System.Windows.Threading;

    /// <summary>
    /// 将控制台输出（主要是 <see cref="Console.WriteLine" /> 等方法）写入一文本框的辅助类。
    /// </summary>
    public class ConsoleWriter : TextWriter
    {
        /// <summary>
        /// 写入控制台输出内容的文本框。
        /// </summary>
        private TextBox textbox;

        /// <summary>
        /// UI 线程访问对象。
        /// </summary>
        private Dispatcher dispatcher;

        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="textbox">写入控制台输出内容的文本框。</param>
        /// <param name="dispatcher">UI 线程访问对象。</param>
        public ConsoleWriter(TextBox textbox, Dispatcher dispatcher)
        {
            this.textbox = textbox;
            this.dispatcher = dispatcher;
        }

        /// <inheritdoc />
        public override void Write(char value)
        {
            this.dispatcher.Invoke(() => { textbox.Text += value; });
        }

        /// <inheritdoc />
        public override void Write(string value)
        {
            this.dispatcher.Invoke(() => { textbox.Text += value; });
        }

        /// <inheritdoc />
        public override Encoding Encoding => Encoding.UTF8;
    }
}
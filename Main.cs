using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;

namespace Wox.Plugin.Clipboard
{
    public class Main : IPlugin
    {
        private const int MaxDataCount = 300;
        private readonly InputSimulator inputSimulator = new InputSimulator();
        private PluginInitContext context;
        List<string> dataList = new List<string>();

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();
            List<string> displayData;
            if (query.ActionParameters.Count == 0)
            {
                displayData = dataList;
            }
            else
            {
                displayData = dataList.Where(i => i.ToLower().Contains(query.GetAllRemainingParameter().ToLower()))
                        .ToList();
            }

            results.AddRange(displayData.Select(o => new Result
            {
                Title = o.Trim().Replace("\r\n", " ").Replace('\n', ' '),
                IcoPath = "Images\\clipboard.png",
                Action = c =>
                {
                    if (!ClipboardMonitor.ClipboardWrapper.SetText(o))
                        return false;
                    context.API.HideApp();
                    Task.Delay(50).ContinueWith(t => inputSimulator.Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_V));
                    return true;
                }
            }).Reverse());
            return results;
        }

        public void Init(PluginInitContext context)
        {
            this.context = context;
            ClipboardMonitor.OnClipboardChange += ClipboardMonitor_OnClipboardChange;
            ClipboardMonitor.Start();
        }

        void ClipboardMonitor_OnClipboardChange(ClipboardFormat format, object data)
        {
            if (format == ClipboardFormat.Html ||
                format == ClipboardFormat.SymbolicLink ||
                format == ClipboardFormat.Text ||
                format == ClipboardFormat.UnicodeText)
            {
                if (data != null && !string.IsNullOrEmpty(data.ToString().Trim()))
                {
                    if (dataList.Contains(data.ToString()))
                    {
                        dataList.Remove(data.ToString());
                    }
                    dataList.Add(data.ToString());

                    if (dataList.Count > MaxDataCount)
                    {
                        dataList.Remove(dataList.Last());
                    }
                }
            }
        }
    }
}

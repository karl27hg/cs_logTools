using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace RichTextBoxConsole.DebugTools
{
    class LogMan
    {
        private static LogMan _instance;
        private static Object _instanceLock = new Object();
        private LogMan()
        {
        }
        public static LogMan Instance
        {
            get
            {
                lock(_instanceLock)
                {
                    if (null == _instance)
                        _instance = new LogMan();
                    return _instance;
                }
            }
        }
        // class start
        private LinkedList<String> _logLinklist;
        private Object _logLinklistLock = new Object();
        private int _maxRows;
        private int _currentRow;
        private RichTextBox _component;
        private String[] _simbol;
        private bool _hasLastNewlineOfRichTb;
        //
        public void Init(ref RichTextBox component, int maxRows = 512)
        {
            _component = component;
            _maxRows = maxRows;
            _logLinklist = new LinkedList<string>();
            _currentRow = 0;
            _simbol[0] = "\r\n";
            _hasLastNewlineOfRichTb = false;
        }
        public void Write(String contents)
        {
            AddLogLinklist(contents);
            // show richTextBox
            bool isOverLog = RemoveOverLog();
            if (isOverLog)
                OutputOverwriteRichTextBox();
            else
                OutputAddRichTextBox(contents);
        }
        public void Write(String format, params object[] args)
        {
            Write(String.Format(format, args));
        }
        public void WriteLine(String contents)
        {
            StringBuilder tmp = new StringBuilder();
            tmp.Append(contents);
            tmp.Append("\r\n");
            Write(tmp.ToString());
        }
        public void WriteLine(String format, params object[] args)
        {
            StringBuilder tmp = new StringBuilder();
            tmp.Append(String.Format(format, args));
            tmp.Append("\r\n");
            Write(tmp.ToString());
        }
        public void RemoveRowsLogs(int rows)
        {
            for(int i = 0; i < rows; ++i)
            {
                _logLinklist.RemoveLast();
                --_currentRow;
            }
            // apply richtextbox
            OutputOverwriteRichTextBox();
        }
        public void Clear()
        {
            Debug.Assert(null == _component);
            _logLinklist.Clear();
            _component.Text = "";
            _hasLastNewlineOfRichTb = false;
        }
        //
        private int AddLogLinklist(String contents)
        {
            if (contents.Equals(""))
                return 0;
            Debug.Assert(!contents.Equals(""));
            var splitContents = contents.Split(_simbol, StringSplitOptions.None);
            // contents has not "\r\n"
            if(1 == splitContents.Count())
            {
                if (0 != _logLinklist.Count() && !_logLinklist.Last().Contains("\r\n"))
                    _logLinklist.Last.Value += contents;
                else
                {
                    _logLinklist.AddLast(contents);
                    ++_currentRow;
                }
                return 1;
            }
            // contents has "\r\n"
            else
            {
                // [0, n-1]
                for(int i = 0; i < splitContents.Count() -1; ++i)
                {
                    if (0 == i && 0 < _logLinklist.Count() && !_logLinklist.Last().Contains("\r\n"))
                        _logLinklist.Last.Value += splitContents[i] + "\r\n";
                    else
                    {
                        _logLinklist.AddLast(splitContents[i] + "\r\n");
                        ++_currentRow;
                    }
                }
                // [n] != ""
                if(!splitContents.Last().Equals(""))
                {
                    _logLinklist.AddLast(splitContents.Last());
                    ++_currentRow;
                    return splitContents.Count();
                }
                return splitContents.Count() - 1;
            }
        }
        private void OutputOverwriteRichTextBox()
        {
            Debug.Assert(null != _component);
            StringBuilder tmpLogs = new StringBuilder();
            foreach(var itLog in _logLinklist)
            {
                tmpLogs.Append(itLog);
            }
            if(_logLinklist.Last().Contains("\r\n"))
            {
                _hasLastNewlineOfRichTb = true;
                _component.Text = tmpLogs.ToString().Remove(tmpLogs.Length - 2);
            }
            else
            {
                _hasLastNewlineOfRichTb = false;
                _component.Text = tmpLogs.ToString();
            }
            _component.Select(_component.Text.Length, 0);
            _component.ScrollToCaret();
            _component.Refresh();
        }
        private void OutputAddRichTextBox(String contents)
        {
            Debug.Assert(null != _component);
            if (_hasLastNewlineOfRichTb)
                _component.AppendText("\r\n");
            if(_logLinklist.Last().Contains("\r\n"))
            {
                _hasLastNewlineOfRichTb = true;
                _component.AppendText(contents.Remove(contents.Length - 2));
            }
            else
            {
                _hasLastNewlineOfRichTb = false;
                _component.AppendText(contents);
            }
            _component.Select(_component.Text.Length, 0);
            _component.ScrollToCaret();
            _component.Refresh();
        }
        private bool RemoveOverLog()
        {
            bool isOverLog = (_currentRow > _maxRows);
            for(; _currentRow > _maxRows; --_currentRow)
            {
                _logLinklist.RemoveFirst();
            }
            return isOverLog;
        }
    }
}

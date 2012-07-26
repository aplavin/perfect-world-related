using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Media;
using IronPython.Hosting;
using PwLib;

namespace scripterPw
{
    public class Script : INotifyPropertyChanged
    {

        public class EventRaisingStream : Stream
        {
            public EventRaisingStream()
            {
            }

            public EventRaisingStream(Action<string> action)
            {
                WriteEvent += action;
            }

            public event Action<string> WriteEvent;

            private void LaunchEvent(string txtWritten)
            {
                if (WriteEvent != null)
                    WriteEvent(txtWritten);
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                string s = Encoding.ASCII.GetString(buffer, offset, count);
                if (!s.IsNullOrWhiteSpace())
                    LaunchEvent(s);
            }

            public override bool CanRead { get { return false; } }

            public override bool CanSeek { get { return false; } }

            public override bool CanWrite { get { return true; } }

            public override long Length { get { throw new NotSupportedException(); } }

            public override long Position
            {
                get { throw new NotSupportedException(); }
                set { throw new NotSupportedException(); }
            }
        }

        private static string PreScript
        {
            get
            {
                var sb = new StringBuilder();

                sb.AppendLine("import clr, System");
                sb.AppendLine("from System import Array");
                sb.AppendLine("clr.AddReference(\"System.Core\")");
                sb.AppendLine("clr.ImportExtensions(System.Linq)");

                sb.AppendLine("clr.AddReference(\"PwLib\")");
                sb.AppendLine("from PwLib.Objects import *");

                sb.AppendLine("from time import sleep");

                foreach (var method in typeof(PwInterface).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(m => !m.IsSpecialName))
                {
                    Func<ParameterInfo, string> parameterForDef = p =>
                    {
                        if (Attribute.IsDefined(p, typeof(ParamArrayAttribute)))
                            return string.Format("*{0}", p.Name);

                        if (p.IsOptional)
                        {
                            if (p.RawDefaultValue is string)
                                return string.Format("{0} = \"{1}\"", p.Name, p.RawDefaultValue);

                            return string.Format("{0} = {1}", p.Name, p.RawDefaultValue ?? "None");
                        }

                        return p.Name;
                    };
                    sb.AppendFormat("def {0}({1}):\n", method.Name, method.GetParameters().Aggregate(parameterForDef, ", "));
                    sb.AppendFormat(" return pw.Interface.{0}({1})\n", method.Name, method.GetParameters().Aggregate(par => par.Name, ", "));
                    sb.AppendLine();
                }
                foreach (var property in typeof(PwInterface).GetProperties())
                {
                    sb.AppendFormat("def {0}():\n", property.Name);
                    sb.AppendFormat(" return pw.Interface.{0}\n", property.Name);
                    sb.AppendLine();
                }

                return sb.ToString();
            }
        }

        public string FilePath { get; set; }
        public string Name { get { return Path.GetFileName(FilePath); } }
        public string Source
        {
            get { return File.ReadAllText(FilePath); }
            set { File.WriteAllText(FilePath, value); }
        }
        public Thread WorkingThread { get; set; }
        public bool IsRunning { get { return WorkingThread != null && WorkingThread.IsAlive; } }

        public Script(string filePath)
        {
            FilePath = filePath;
        }

        public void Start(PwClient client, Action<string, MsgType> log)
        {
            var runtime = Python.CreateRuntime();
            var scope = runtime.CreateScope();
            var engine = runtime.GetEngine("Python");

            runtime.IO.SetOutput(new EventRaisingStream(s => log(s, MsgType.Out)), Encoding.ASCII);
            runtime.IO.SetErrorOutput(new EventRaisingStream(s => log(s, MsgType.Error)), Encoding.ASCII);

            scope.SetVariable("pw", client);

            engine.Execute(PreScript, scope);

            scope.SetVariable("Log", new Action<dynamic>(msg => log(msg, MsgType.Out)));
            scope.SetVariable("Success", new Action<dynamic>(msg => log(msg, MsgType.Success)));
            scope.SetVariable("Error", new Action<dynamic>(msg => log(msg, MsgType.Error)));

            WorkingThread = new Thread(() =>
            {
                OnPropertyChanged("IsRunning");
                log("Script started", MsgType.System);
                try
                {
                    engine.Execute(Source, scope);

                    if (scope.ContainsVariable("Loop"))
                    {
                        while (true)
                        {
                            scope.GetVariable<Action>("Loop")();
                            Thread.Sleep(200);
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
                catch (Exception ex)
                {
                    log(ex.Message, MsgType.Error);
                }
                log("Script completed\n", MsgType.System);
                OnPropertyChanged("IsRunning");
            });
            WorkingThread.Start();
        }

        public void Stop()
        {
            WorkingThread.Abort();
            OnPropertyChanged("IsRunning");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
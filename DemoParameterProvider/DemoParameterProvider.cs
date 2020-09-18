using NetEti.ApplicationControl;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using Vishnu.Interchange;

namespace NetEti.ApplicationEnvironment
{
    /// <summary>
    /// Implementiert IParameterReader, kann Vishnu als
    /// UserParameterReader zur Verfügung gestellt werden.
    /// </summary>
    /// <remarks>
    /// File: DemoParameterProvider
    /// Autor: Erik Nagel
    ///
    /// 07.06.2015 Erik Nagel: erstellt
    /// 14.06.2019 Erik Nagel: überarbeitet.
    /// </remarks>

    public class DemoParameterProvider : IParameterReader
    {
        #region IParameterReader Implementation

        /// <summary>
        /// Event, das ausgelöst wird, wenn die Parameter neu geladen wurden.
        /// </summary>
        public event EventHandler ParametersReloaded;

        /// <summary>
        /// Liefert zu einem String-Parameter einen String-Wert.
        /// </summary>
        /// <param name="parameterName">Parameter-Name.</param>
        /// <returns>Parameter-Value.</returns>
        public string ReadParameter(string parameterName)
        {
            if (parameterName == "GesuchterParameter")
            {
                return "Gefunden!";
            }
            else
            {
                if (parameterName == "UebergebenerParameter")
                {
                    string timeString = this._lastTimerStart == DateTime.MinValue ? " - "
                        : this._lastTimerStart.ToString("hh:mm:ss.ms");
                    return String.Format($"{this._initParameter} - letzter Refresh: {timeString}");
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Einrichtungsroutine - übernimmt Parameter, holt alle Infos
        /// und stellt sie als Properties zur Verfügung.
        /// Startet ggf. einen Timer für den Parameter-Refresh.
        /// </summary>
        /// <param name="parameters">Ein Objekt zur Parameterübergabe; dieser ParameterProvider
        /// erwartet einen String mit einem übergebenem Testwert plus optional
        /// einen Timer-Parameter für regelmäßige Reloads durch Pipe-Symbol '|' abgetrennt.</param>
        public void Init(object parameters)
        {
            this._publisher = InfoController.GetInfoController();
            this.EvaluateParameters(parameters.ToString());

            this.ReloadApplicationParameters();

            if (this._eventTimer != null)
            {
                this._lastTimerStart = DateTime.Now;
                this._nextTimerStart = this._lastTimerStart.AddMilliseconds(this._timerInterval);
                this._eventTimer.Start();
            }
        }

        #endregion IParameterReader Implementation

        /// <summary>
        /// Löst das ParametersReloaded-Ereignis aus.
        /// </summary>
        protected virtual void OnParametersReloaded()
        {
            ParametersReloaded?.Invoke(this, new EventArgs());
        }

        private string _initParameter;

        private void ReloadApplicationParameters()
        {
            try
            {
                this._publisher.Publish("Lade aufwändige Parameter...");
                Thread.Sleep(2000);
            }
            catch (Exception ex)
            {
                this._publisher.Publish(this, ex.Message);
                throw;
            }
            this.OnParametersReloaded();
        }

        private IInfoPublisher _publisher;
        private System.Timers.Timer _eventTimer;
        private int _timerInterval;
        private string _textPattern;
        private Regex _compiledPattern;
        private DateTime _lastTimerStart;
        private DateTime _nextTimerStart;

        private void EvaluateParameters(string parameterUndTimer)
        {
            this._initParameter = null;
            this._eventTimer = null;
            this._lastTimerStart = DateTime.MinValue;
            this._nextTimerStart = DateTime.MinValue;
            this._textPattern = @"(?:MS|S|M|H|D):\d+";
            this._compiledPattern = new Regex(_textPattern);

            if (String.IsNullOrEmpty(parameterUndTimer))
            {
                return;
            }

            string[] para = (parameterUndTimer + "|").Split('|');

            MatchCollection alleTreffer;
            alleTreffer = _compiledPattern.Matches(para[para.Length - 2]);
            this._timerInterval = 0;
            if (alleTreffer.Count > 0)
            {
                string subKey = alleTreffer[0].Groups[0].Value;
                switch (subKey.Split(':')[0])
                {
                    case "MS": this._timerInterval = Convert.ToInt32(subKey.Split(':')[1]); break;
                    case "S": this._timerInterval = Convert.ToInt32(subKey.Split(':')[1]) * 1000; break; ;
                    case "M": this._timerInterval = Convert.ToInt32(subKey.Split(':')[1]) * 1000 * 60; break; ;
                    case "H": this._timerInterval = Convert.ToInt32(subKey.Split(':')[1]) * 1000 * 60 * 60; break; ;
                    case "D": this._timerInterval = Convert.ToInt32(subKey.Split(':')[1]) * 1000 * 60 * 60 * 24; break; ;
                    default:
                        throw new ArgumentException("Falsche Einheit, zulässig sind: MS=Millisekunden, S=Sekunden, M=Minuten, H=Stunden, D=Tage.");
                }
                this._eventTimer = new System.Timers.Timer(this._timerInterval);
                this._eventTimer.Elapsed += new ElapsedEventHandler(eventTimer_Elapsed);
                this._eventTimer.Stop();
            }
            string parameter = "";
            for (int i = 0; i < para.Length - 2; i++)
            {
                parameter += para[i].Trim();
            }
            this._initParameter = parameter;
        }

        private void eventTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (this._eventTimer != null)
            {
                this._eventTimer.Stop();
            }
            this.ReloadApplicationParameters();
            if (this._eventTimer != null)
            {
                this._lastTimerStart = DateTime.Now;
                this._nextTimerStart = this._lastTimerStart.AddMilliseconds(this._timerInterval);
                this._eventTimer.Start();
            }
        }

    }
}

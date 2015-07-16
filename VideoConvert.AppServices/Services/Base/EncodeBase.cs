// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncodeBase.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Base Encoder class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Services.Base
{
    using System;
    using VideoConvert.AppServices.Services.Base.Interfaces;
    using VideoConvert.AppServices.Services.Interfaces;
    using VideoConvert.Interop.EventArgs;
    using VideoConvert.Interop.Model;

    /// <summary>
    /// Base Encoder class
    /// </summary>
    public class EncodeBase : IEncodeBase
    {
        #region Private Variables

        /// <summary>
        /// The User Setting Service
        /// </summary>
        private readonly IAppConfigService _appConfig;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EncodeBase"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public EncodeBase(IAppConfigService appConfig)
        {
            _appConfig = appConfig;
        }

        #region Events

        /// <summary>
        /// Fires when a new Encoder Task starts
        /// </summary>
        public event EventHandler EncodeStarted;

        /// <summary>
        /// Fires when an Encoder Task finishes.
        /// </summary>
        public event EncodeCompletedStatus EncodeCompleted;

        /// <summary>
        /// Encode process has progressed
        /// </summary>
        public event EncodeProgressStatus EncodeStatusChanged;

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether IsEncoding.
        /// </summary>
        public bool IsEncoding { get; protected set; }

        #endregion

        #region Invoke Events

        /// <summary>
        /// Invoke the Encode Status Changed Event.
        /// </summary>
        /// <param name="e">
        /// The EncodeProgressEventArgs.
        /// </param>
        public void InvokeEncodeStatusChanged(EncodeProgressEventArgs e)
        {
            var handler = EncodeStatusChanged;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Invoke the Encode Completed Event
        /// </summary>
        /// <param name="e">
        /// The EncodeCompletedEventArgs.
        /// </param>
        public void InvokeEncodeCompleted(EncodeCompletedEventArgs e)
        {
            var handler = EncodeCompleted;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Invoke the Encode Started Event
        /// </summary>
        /// <param name="e">
        /// The EventArgs.
        /// </param>
        public void InvokeEncodeStarted(EventArgs e)
        {
            var handler = EncodeStarted;
            handler?.Invoke(this, e);
        }

        #endregion

        #region Methods

        /// <summary>
        /// A Start Method to be implemented
        /// </summary>
        /// <param name="encodeQueueTask"></param>
        public virtual void Start(EncodeInfo encodeQueueTask)
        {
            // Do Nothing
        }

        /// <summary>
        /// A Stop Method to be implemeneted.
        /// </summary>
        public virtual void Stop()
        {
            // Do Nothing
        }


        /// <summary>
        /// Pase the CLI status output (from standard output)
        /// </summary>
        /// <param name="encodeStatus">
        /// The encode Status.
        /// </param>
        /// <param name="startTime">
        /// The start Time.
        /// </param>
        /// <returns>
        /// The <see cref="EncodeProgressEventArgs"/>.
        /// </returns>
        public EncodeProgressEventArgs ReadEncodeStatus(string encodeStatus, DateTime startTime)
        {
            var eventArgs = new EncodeProgressEventArgs
            {
                AverageFrameRate = 0,
                CurrentFrameRate = 0,
                EstimatedTimeLeft = new TimeSpan(),
                PercentComplete = 0,
                ElapsedTime = DateTime.Now.TimeOfDay,
            };

            return eventArgs;
        }

        #endregion
    }
}
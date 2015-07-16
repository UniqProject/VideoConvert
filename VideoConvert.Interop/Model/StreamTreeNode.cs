// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StreamTreeNode.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   TreeView for Stream selection screen
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// TreeView for Stream selection screen
    /// </summary>
    public class StreamTreeNode : INotifyPropertyChanged
    {
        /// <summary>
        /// Item ID
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Item Name
        /// </summary>
        public string Name { get; set; }

        private object _data;

        /// <summary>
        /// Item data
        /// </summary>
        public object Data
        {
            get
            {
                return _data;
            }
            set
            {
                if (value == null || value.Equals(_data)) return;

                _data = value;
                OnPropertyChanged("Data");
            }
        }

        private bool _isChecked;

        /// <summary>
        /// Item Checked
        /// </summary>
        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (value == _isChecked) return;

                _isChecked = value;
                OnPropertyChanged("IsChecked");
            }
        }

        /// <summary>
        /// Item Expanded
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Parent Item
        /// </summary>
        public StreamTreeNode Parent { get; set; }

        /// <summary>
        /// Child Items
        /// </summary>
        public List<StreamTreeNode> Children { get; set; }

        private bool _hardcodeIntoVideo;
        private bool _matroskaDefault;
        private bool _keepOnlyForced;

        /// <summary>
        /// Hardcode subtitle into Video
        /// </summary>
        public bool HardcodeIntoVideo
        {
            get
            {
                return _hardcodeIntoVideo;
            }
            set
            {
                if (value == _hardcodeIntoVideo) return;

                _hardcodeIntoVideo = value;
                OnPropertyChanged("HardcodeIntoVideo");
            }
        }

        /// <summary>
        /// Default Stream in Matroska container
        /// </summary>
        public bool MatroskaDefault
        {
            get
            {
                return _matroskaDefault;
            }
            set
            {
                if (value == _matroskaDefault) return;

                _matroskaDefault = value;
                OnPropertyChanged("MatroskaDefault");
            }
        }

        /// <summary>
        /// Keep only forced subtitle captions
        /// </summary>
        public bool KeepOnlyForced
        {
            get
            {
                return _keepOnlyForced;
            }
            set
            {
                if (value == _keepOnlyForced) return;

                _keepOnlyForced = value;
                OnPropertyChanged("KeepOnlyForced");
            }
        }


        /// <summary>
        /// Property Changed Event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Property Changed Event handler
        /// </summary>
        /// <param name="e"></param>
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Property Changed Event handler
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
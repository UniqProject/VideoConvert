// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StreamTreeNode.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model
{
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// 
    /// </summary>
    public class StreamTreeNode : INotifyPropertyChanged
    {
        public int ID { get; set; }
        public string Name { get; set; }

        private object _data;

        public object Data
        {
            get
            {
                return _data;
            }
            set
            {
                if (value != null && !value.Equals(_data))
                {
                    _data = value;
                    OnPropertyChanged("Data");
                }
            }
        }

        private bool _isChecked;

        public bool IsChecked
        {
            get
            {
                return _isChecked;
            }
            set
            {
                if (value != _isChecked)
                {
                    _isChecked = value;
                    OnPropertyChanged("IsChecked");
                }
            }
        }

        public bool IsExpanded { get; set; }
        public StreamTreeNode Parent { get; set; }
        public List<StreamTreeNode> Children { get; set; }

        private bool _hardcodeIntoVideo;
        private bool _matroskaDefault;
        private bool _keepOnlyForced;

        public bool HardcodeIntoVideo
        {
            get
            {
                return _hardcodeIntoVideo;
            }
            set
            {
                if (value != _hardcodeIntoVideo)
                {
                    _hardcodeIntoVideo = value;
                    OnPropertyChanged("HardcodeIntoVideo");
                }
            }
        }

        public bool MatroskaDefault
        {
            get
            {
                return _matroskaDefault;
            }
            set
            {
                if (value != _matroskaDefault)
                {
                    _matroskaDefault = value;
                    OnPropertyChanged("MatroskaDefault");
                }
            }
        }

        public bool KeepOnlyForced
        {
            get
            {
                return _keepOnlyForced;
            }
            set
            {
                if (value != _keepOnlyForced)
                {
                    _keepOnlyForced = value;
                    OnPropertyChanged("KeepOnlyForced");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) 
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
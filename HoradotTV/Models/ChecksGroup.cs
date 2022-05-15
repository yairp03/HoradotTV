using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HoradotTV.Models
{
    internal class ChecksGroup : INotifyPropertyChanged
    {

        private string imageUri;
        public string ImageUri
        {
            get => imageUri;
            set
            {
                imageUri = value;
                NotifyPropertyChanged();
            }
        }


        private string displayText;
        public string DisplayText
        {
            get => displayText;
            set
            {
                displayText = value;
                NotifyPropertyChanged();
            }
        }

        public string FixProblemUrl { get; set; }

        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                NotifyPropertyChanged();
            }
        }

        private bool isDone;
        public bool IsDone
        {
            get => isDone;
            set
            {
                isDone = value;
                NotifyPropertyChanged();
            }
        }

        private bool isFailed;
        public bool IsFailed
        {
            get => isFailed;
            set
            {
                isFailed = value;
                NotifyPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public List<IDependencyCheck> Checks { get; set; }

        public ChecksGroup(string imageUri, List<IDependencyCheck> checks)
        {
            ImageUri = imageUri;
            Checks = checks;
        }

        public async Task<bool> RunChecksAsync()
        {
            IsBusy = true;
            foreach (var check in Checks)
            {
                DisplayText = check.LoadingText;
                if (!await check.RunCheckAsync())
                {
                    IsBusy = false;
                    FixProblemUrl = check.FixProblemUrl;
                    IsFailed = true;
                    return false;
                }
            }
            IsBusy = false;
            IsDone = true;
            return true;
        }

        public void Reset()
        {
            IsDone = false;
            IsFailed = false;
            IsBusy = false;
        }
    }
}

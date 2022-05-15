using HoradotTV.Models;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HoradotTV.ViewModels
{
    internal class StartupChecksViewModel : INotifyPropertyChanged
    {
        private bool isBusy;
        public bool IsBusy
        {
            get => isBusy;
            set
            {
                isBusy = value;
                NotifyPropertyChanged();
                RaiseCanExecuteChanged();
            }
        }
        public bool IsNotBusy
        {
            get => !isBusy;
            set
            {
                isBusy = !value;
                NotifyPropertyChanged();
                RaiseCanExecuteChanged();
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
                RaiseCanExecuteChanged();
            }
        }
        public List<ChecksGroup> ChecksGroups { get; set; }
        public string FixProblemUrl { get; set; }
        public Command FixProblemCommand { get; set; }
        public AsyncCommand RunChecksCommand { get; set; }
        public Command ProceedCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public StartupChecksViewModel()
        {
            ChecksGroups = new List<ChecksGroup>()
            {
                new ChecksGroup("/Resources/Images/chrome-logo.png", new()
                {
                    new ChromeDownloadCheck(),
                    new ChromeDriverCheck()
                }),
                new ChecksGroup("/Resources/Images/sdarot-logo.png", new()
                {
                    new SdarotConnectionCheck()
                }),
            };

            FixProblemCommand = new Command(FixProblem);
            RunChecksCommand = new AsyncCommand(RunChecks, (_) => IsNotBusy);
            ProceedCommand = new Command(ProceedToApp, () => IsDone);
        }

        internal async Task RunChecks()
        {
            foreach (var group in ChecksGroups)
            {
                group.Reset();
            }
            IsBusy = true;
            foreach (var group in ChecksGroups)
            {
                if (!await group.RunChecksAsync())
                {
                    FixProblemUrl = group.FixProblemUrl;
                    break;
                }
            }
            IsBusy = false;
        }

        internal void FixProblem()
        {
            Process.Start(new ProcessStartInfo(FixProblemUrl) { UseShellExecute = true });
        }

        internal void ProceedToApp()
        {
            
        }

        internal void RaiseCanExecuteChanged()
        {
            FixProblemCommand.RaiseCanExecuteChanged();
            RunChecksCommand.RaiseCanExecuteChanged();
            ProceedCommand.RaiseCanExecuteChanged();
        }
    }
}

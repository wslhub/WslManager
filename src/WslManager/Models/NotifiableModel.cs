using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WslManager.Models
{
    public abstract class NotifiableModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

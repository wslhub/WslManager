using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WslManager.ViewModels
{
    public abstract class NotifiableModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "", params string[] additionalProperties)
        {
            var handler = PropertyChanged;

            if (handler != null)
            {
                handler.Invoke(this, new PropertyChangedEventArgs(propertyName));

                if (additionalProperties != null && additionalProperties.Length > 0)
                    foreach (var eachPropertyName in additionalProperties)
                        handler.Invoke(this, new PropertyChangedEventArgs(eachPropertyName));
            }
        }
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LedMatrix.Client
{
   public abstract class NotifyChangeBase : INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;

      protected bool SetPropertyValue<T>(ref T field, T value, [CallerMemberName] string name = "")
      {
         if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

         field = value;
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
         return true;
      }
   }
}

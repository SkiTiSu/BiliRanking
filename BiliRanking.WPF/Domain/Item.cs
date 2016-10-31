using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliRanking.WPF.Domain
{
    public class Item : INotifyPropertyChanged
    {
        private string name;
        private object content;

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                this.MutateVerbose(ref name, value, RaisePropertyChanged());
            }
        }

        public object Content
        {
            get { return content; }
            set
            {
                this.MutateVerbose(ref content, value, RaisePropertyChanged());
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private Action<PropertyChangedEventArgs> RaisePropertyChanged()
        {
            return args => PropertyChanged?.Invoke(this, args);
        }
    }
}

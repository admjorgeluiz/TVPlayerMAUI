using System.Collections.ObjectModel;

namespace TVPlayerMAUI.Models
{
    public class ChannelGroup : ObservableCollection<Channel>
    {
        public string Name { get; private set; }

        public ChannelGroup(string name, IEnumerable<Channel> channels) : base(channels)
        {
            Name = name;
        }
    }
}
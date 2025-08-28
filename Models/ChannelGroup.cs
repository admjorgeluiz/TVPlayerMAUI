// Localização: Models/ChannelGroup.cs
using System.Collections.ObjectModel;

namespace TVPlayerMAUI.Models
{
    // Um grupo é simplesmente uma coleção de canais que também tem um nome.
    public class ChannelGroup : ObservableCollection<Channel>
    {
        public string Name { get; private set; }

        public ChannelGroup(string name, IEnumerable<Channel> channels) : base(channels)
        {
            Name = name;
        }
    }
}
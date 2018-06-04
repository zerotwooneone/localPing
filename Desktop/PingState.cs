using Desktop.Target;
using Desktop.Vector;

namespace Desktop
{
    public class PingState
    {
        public TargetDatamodel TargetDatamodel { get; set; }
        public IVector Previous { get; set; }
    }
}
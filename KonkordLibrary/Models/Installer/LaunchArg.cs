namespace KonkordLibrary.Models.Installer
{
    public class LaunchArg
    {
        public string Arg {  get; set; }
        public int Priority { get; set; }

        public LaunchArg() { }

        public LaunchArg(string arg, int priority)
        {
            Arg = arg;
            Priority = priority;
        }
    }
}

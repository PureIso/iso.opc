using System.Threading;

namespace Server
{
    public static class Program
    {
        #region Properties
        public static MainService MainService { get; set; }
        public static AutoResetEvent AutoResetEvent { get; set; }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main()
        {
            AutoResetEvent = new AutoResetEvent(false);
            MainService = new MainService();

#if DEBUG
            MainService.OnDebug();
            AutoResetEvent.WaitOne();
#else
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MainService()
            };
            ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}

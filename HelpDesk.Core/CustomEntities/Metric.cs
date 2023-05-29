namespace HelpDesk.Core.CustomEntities
{
    public class Metric
    {
        public int Abiertos { get; set; }
        public int Enproceso { get; set; }
        public int Completados { get; set; }
        public int Costumers { get; set; }
        public int WaitingToAttend { get; set; }
        public int Waiting2ToAttend { get; set; }
        public int Waiting3ToAttend { get; set; }
        public int Finished { get; set; }
        public int FinishedToAttend { get; set; }
    }
}

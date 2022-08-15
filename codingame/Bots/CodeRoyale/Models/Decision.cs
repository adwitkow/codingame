public readonly struct Decision
{
    public Decision(string mainDecision, string trainDecision)
    {
        this.MainDecision = mainDecision;
        this.TrainDecision = trainDecision;
    }

    public string MainDecision { get; }

    public string TrainDecision { get; }
}
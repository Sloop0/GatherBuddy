namespace GatherBuddy.Alarms;

public enum AlarmType : byte
{
    Node,
    Fish,
}

public interface IAlarm
{
    public AlarmType Type         { get; }
    public uint      Id           { get; }
    public string    Name         { get; set; }
    public int       SecondOffset { get; set; }
    public Sounds    SoundId      { get; set; }
    public bool      Enabled      { get; set; }
    public bool      PrintMessage { get; set; }

    public void SendMessage(long timeDiff);
}

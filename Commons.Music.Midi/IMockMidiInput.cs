namespace Commons.Music.Midi
{
    public interface IMockMidiInput
    {
        public void MockMessageReceived(byte[] bytes, int offset, int length, long timestamp);
    }
}
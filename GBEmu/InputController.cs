namespace GBEmu;

public class InputController
{
    private byte buttonState = 0xFF; // All buttons released

    public byte Read()
    {
        return buttonState;
    }

    public void Write(byte value)
    {
        // Typically ignored on real hardware, but can be used for column selection
    }

    public void PressButton(GameBoyButton button)
    {
        buttonState &= (byte)~button; // Clear bit (pressed)
        SM83.RequestInterrupt(SM83.InterruptFlags.Joypad);
    }

    public void ReleaseButton(GameBoyButton button)
    {
        buttonState |= (byte)button; // Set bit (released)
    }
}

[Flags]
public enum GameBoyButton
{
    A = 1 << 0,
    B = 1 << 1,
    Select = 1 << 2,
    Start = 1 << 3,
    Right = 1 << 4,
    Left = 1 << 5,
    Up = 1 << 6,
    Down = 1 << 7
}

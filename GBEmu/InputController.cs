namespace GBEmu;

public class InputController
{
    public byte buttonState = 0xFF; // All buttons released
    private bool rightPressed, leftPressed, upPressed, downPressed;
    private bool aPressed, bPressed, selectPressed, startPressed;

    private bool SelectButtons => (buttonState & (1 << 5)) == 0;
    private bool SelectDPad => (buttonState & (1 << 4)) == 0;

    public byte Read()
    {
        byte joyp = (byte)(buttonState & 0b00110000);

        if (SelectDPad)
        {
            if (rightPressed) joyp = (byte)(joyp & ~0b00000001);
            if (leftPressed)  joyp = (byte)(joyp & ~0b00000010);
            if (upPressed)    joyp = (byte)(joyp & ~0b00000100);
            if (downPressed)  joyp = (byte)(joyp & ~0b00001000);
        }
        
        if (SelectButtons)
        {
            if (aPressed)      joyp = (byte)(joyp & ~0b00000001);
            if (bPressed)      joyp = (byte)(joyp & ~0b00000010);
            if (selectPressed) joyp = (byte)(joyp & ~0b00000100);
            if (startPressed)  joyp = (byte)(joyp & ~0b00001000);
        }
        
        return joyp;
    }

    public void Write(byte value)
    {
        buttonState = (byte)((buttonState & 0b00001111) | (value & 0b11110000));
        Program.debugText.DisplayedString = "Input: " + Convert.ToString(Read(), 2).PadLeft(8, '0');
    }

    public void PressButton(GameBoyButton button)
    {
        switch (button)
        {
            case GameBoyButton.ARight: rightPressed = true; break;
            case GameBoyButton.BLeft: leftPressed = true; break;
            case GameBoyButton.SelectUp: upPressed = true; break;
            case GameBoyButton.StartDown: downPressed = true; break;
            case GameBoyButton.A: aPressed = true; break;
            case GameBoyButton.B: bPressed = true; break;
            case GameBoyButton.Select: selectPressed = true; break;
            case GameBoyButton.Start: startPressed = true; break;
        }

        Program.debugText.DisplayedString = "Input: " + Convert.ToString(Read(), 2).PadLeft(8, '0');
        SM83.RequestInterrupt(SM83.InterruptFlags.Joypad);
    }

    public void ReleaseButton(GameBoyButton button)
    {
        switch (button)
        {
            case GameBoyButton.ARight: rightPressed = false; break;
            case GameBoyButton.BLeft: leftPressed = false; break;
            case GameBoyButton.SelectUp: upPressed = false; break;
            case GameBoyButton.StartDown: downPressed = false; break;
            case GameBoyButton.A: aPressed = false; break;
            case GameBoyButton.B: bPressed = false; break;
            case GameBoyButton.Select: selectPressed = false; break;
            case GameBoyButton.Start: startPressed = false; break;
        }

        Program.debugText.DisplayedString = "Input: " + Convert.ToString(Read(), 2).PadLeft(8, '0');
        SM83.RequestInterrupt(SM83.InterruptFlags.Joypad);
    }
}

[Flags]
public enum GameBoyButton
{
    ARight = 1 << 0,
    BLeft = 1 << 1,
    SelectUp = 1 << 2,
    StartDown = 1 << 3,
    A = 1 << 4,
    B = 1 << 5,
    Select = 1 << 6,
    Start = 1 << 7
}
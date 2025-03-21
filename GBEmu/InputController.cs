namespace GBEmu;

/// <summary>
/// Handles GameBoy joypad input
/// Implements the behavior of the FF00 (JOYP) register
/// </summary>
public class InputController
{
    // GameBoy joypad register (FF00)
    // Bit 7-6: Not used
    // Bit 5: Select Action buttons (0=Select)
    // Bit 4: Select Direction buttons (0=Select)
    // Bit 3-0: Input states for selected buttons (0=Pressed, 1=Released)
    private byte joypadRegister = 0xFF;
    
    // Internal button states (true = pressed)
    private bool rightPressed, leftPressed, upPressed, downPressed;
    private bool aPressed, bPressed, selectPressed, startPressed;

    // Check if action buttons are selected for reading
    private bool AreActionButtonsSelected => (joypadRegister & (1 << 5)) == 0;
    
    // Check if direction pad is selected for reading
    private bool IsDirectionPadSelected => (joypadRegister & (1 << 4)) == 0;

    /// <summary>
    /// Read the current state of the joypad register (FF00)
    /// </summary>
    /// <returns>Current value of the joypad register</returns>
    public byte Read()
    {
        // Start with current register value (only bits 4-5 are writable)
        byte value = (byte)(joypadRegister & 0b00111111);
        
        // Set bits 7-6 to 1 (unused)
        value |= 0b11000000;

        // Handle direction pad if selected
        if (IsDirectionPadSelected)
        {
            // Set bit to 0 if button is pressed
            if (rightPressed) value = (byte)(value & ~0b00000001);
            if (leftPressed)  value = (byte)(value & ~0b00000010);
            if (upPressed)    value = (byte)(value & ~0b00000100);
            if (downPressed)  value = (byte)(value & ~0b00001000);
        }
        
        // Handle action buttons if selected
        if (AreActionButtonsSelected)
        {
            // Set bit to 0 if button is pressed
            if (aPressed)      value = (byte)(value & ~0b00000001);
            if (bPressed)      value = (byte)(value & ~0b00000010);
            if (selectPressed) value = (byte)(value & ~0b00000100);
            if (startPressed)  value = (byte)(value & ~0b00001000);
        }
        
        Program.debugText.DisplayedString = $"Input: {Convert.ToString(value, 2).PadLeft(8, '0')}";
        return value;
    }

    /// <summary>
    /// Write to the joypad register (FF00)
    /// Only bits 4-5 are writable
    /// </summary>
    /// <param name="value">Value to write</param>
    public void Write(byte value)
    {
        byte oldValue = joypadRegister;
        
        // Only bits 4-5 are writable
        joypadRegister = (byte)((joypadRegister & 0b11001111) | (value & 0b00110000));
        
        // Check if any previously unselected buttons are now selected and pressed
        // This can trigger a joypad interrupt
        if ((oldValue != joypadRegister) && ShouldTriggerInterrupt())
        {
            RequestInterrupt();
        }
        Program.debugText.DisplayedString = $"Input: {Convert.ToString(joypadRegister, 2).PadLeft(8, '0')}";
    }

    /// <summary>
    /// Determines if a joypad interrupt should be triggered after a register change
    /// </summary>
    private bool ShouldTriggerInterrupt()
    {
        // Check if any newly selected buttons are already pressed
        if (IsDirectionPadSelected && (rightPressed || leftPressed || upPressed || downPressed))
            return true;
            
        if (AreActionButtonsSelected && (aPressed || bPressed || selectPressed || startPressed))
            return true;
            
        return false;
    }

    /// <summary>
    /// Requests a joypad interrupt
    /// </summary>
    private void RequestInterrupt()
    {
        // Request joypad interrupt
        SM83.RequestInterrupt(SM83.InterruptFlags.Joypad);
    }

    /// <summary>
    /// Press a GameBoy button
    /// </summary>
    /// <param name="button">The button to press</param>
    public void PressButton(GameBoyButton button)
    {
        bool oldButtonState = GetButtonState(button);
        
        // Update button state
        switch (button)
        {
            case GameBoyButton.Right: rightPressed = true; break;
            case GameBoyButton.Left: leftPressed = true; break;
            case GameBoyButton.Up: upPressed = true; break;
            case GameBoyButton.Down: downPressed = true; break;
            case GameBoyButton.A: aPressed = true; break;
            case GameBoyButton.B: bPressed = true; break;
            case GameBoyButton.Select: selectPressed = true; break;
            case GameBoyButton.Start: startPressed = true; break;
        }
        
        // Only trigger interrupt if button state changed and it's currently selected
        if (!oldButtonState && IsButtonSelected(button))
        {
            RequestInterrupt();
        }
    }

    /// <summary>
    /// Release a GameBoy button
    /// </summary>
    /// <param name="button">The button to release</param>
    public void ReleaseButton(GameBoyButton button)
    {
        // Update button state
        switch (button)
        {
            case GameBoyButton.Right: rightPressed = false; break;
            case GameBoyButton.Left: leftPressed = false; break;
            case GameBoyButton.Up: upPressed = false; break;
            case GameBoyButton.Down: downPressed = false; break;
            case GameBoyButton.A: aPressed = false; break;
            case GameBoyButton.B: bPressed = false; break;
            case GameBoyButton.Select: selectPressed = false; break;
            case GameBoyButton.Start: startPressed = false; break;
        }
        
        // Note: GameBoy does not trigger interrupts on button release
    }
    
    /// <summary>
    /// Gets the current state of a button
    /// </summary>
    /// <param name="button">The button to check</param>
    /// <returns>True if the button is pressed</returns>
    private bool GetButtonState(GameBoyButton button)
    {
        return button switch
        {
            GameBoyButton.Right => rightPressed,
            GameBoyButton.Left => leftPressed,
            GameBoyButton.Up => upPressed,
            GameBoyButton.Down => downPressed,
            GameBoyButton.A => aPressed,
            GameBoyButton.B => bPressed,
            GameBoyButton.Select => selectPressed,
            GameBoyButton.Start => startPressed,
            _ => false
        };
    }
    
    /// <summary>
    /// Checks if a button is currently selected for reading
    /// </summary>
    /// <param name="button">The button to check</param>
    /// <returns>True if the button is currently selected</returns>
    private bool IsButtonSelected(GameBoyButton button)
    {
        // Direction buttons
        if ((button == GameBoyButton.Right || button == GameBoyButton.Left || 
             button == GameBoyButton.Up || button == GameBoyButton.Down) && 
            IsDirectionPadSelected)
            return true;
            
        // Action buttons
        if ((button == GameBoyButton.A || button == GameBoyButton.B || 
             button == GameBoyButton.Select || button == GameBoyButton.Start) && 
            AreActionButtonsSelected)
            return true;
            
        return false;
    }
    
    /// <summary>
    /// Resets all button states and the joypad register
    /// </summary>
    public void Reset()
    {
        joypadRegister = 0xFF;
        rightPressed = leftPressed = upPressed = downPressed = false;
        aPressed = bPressed = selectPressed = startPressed = false;
    }
}

/// <summary>
/// GameBoy buttons
/// </summary>
public enum GameBoyButton
{
    Right,
    Left,
    Up,
    Down,
    A,
    B,
    Select,
    Start
}
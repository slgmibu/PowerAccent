﻿using PowerAccent.Core.Services;
using PowerAccent.Core.Tools;

namespace PowerAccent.Core;

public class PowerAccent
{
    private readonly SettingsService _settingService = new SettingsService();
    private readonly KeyboardListener _keyboardListener = new KeyboardListener();

    private LetterKey? letterPressed = null;
    private bool _visible = false;
    private char[] _characters = new char[0];
    private int _selectedIndex = -1;

    public event Action<bool, char[]> OnChangeDisplay;
    public event Action<int, char> OnSelectCharacter;

    public PowerAccent()
    {
        _keyboardListener.KeyDown += PowerAccent_KeyDown;
        _keyboardListener.KeyUp += PowerAccent_KeyUp;
    }

    private bool PowerAccent_KeyDown(object sender, KeyboardListener.RawKeyEventArgs args)
    {
        if (Enum.IsDefined(typeof(LetterKey), (int)args.Key))
        {
            letterPressed = (LetterKey)args.Key;
        }

        if (Enum.IsDefined(typeof(LetterKey), (int)args.Key))
        {
            letterPressed = (LetterKey)args.Key;
        }

        TriggerKey? triggerPressed = null;
        if (letterPressed.HasValue)
            if (Enum.IsDefined(typeof(TriggerKey), (int)args.Key))
            {
                triggerPressed = (TriggerKey)args.Key;
            }

        if (!_visible && letterPressed.HasValue && triggerPressed.HasValue)
        {
            _visible = true;
            _characters = _settingService.GetLetterKey(letterPressed.Value);
            OnChangeDisplay?.Invoke(true, _characters);
        }

        if (_visible && triggerPressed.HasValue)
        {
            if (_selectedIndex == -1)
            {
                if (triggerPressed.Value == TriggerKey.Left)
                    _selectedIndex = _characters.Length / 2 - 1;

                if (triggerPressed.Value == TriggerKey.Right)
                    _selectedIndex = _characters.Length / 2;

                if (triggerPressed.Value == TriggerKey.Space)
                    _selectedIndex = 0;

                if (_selectedIndex < 0) _selectedIndex = 0;
                if (_selectedIndex > _characters.Length - 1) _selectedIndex = _characters.Length - 1;

                OnSelectCharacter?.Invoke(_selectedIndex, _characters[_selectedIndex]);
                return false;
            }

            if (triggerPressed.Value == TriggerKey.Space)
            {
                if (_selectedIndex < _characters.Length - 1)
                    ++_selectedIndex;
                else
                    _selectedIndex = 0;
            }

            if (triggerPressed.Value == TriggerKey.Left && _selectedIndex > 0)
                --_selectedIndex;
            if (triggerPressed.Value == TriggerKey.Right && _selectedIndex < _characters.Length - 1)
                ++_selectedIndex;

            OnSelectCharacter?.Invoke(_selectedIndex, _characters[_selectedIndex]);
            return false;
        }

        return true;
    }

    private bool PowerAccent_KeyUp(object sender, KeyboardListener.RawKeyEventArgs args)
    {
        if (Enum.IsDefined(typeof(LetterKey), (int)args.Key))
        {
            letterPressed = null;
            if (_visible)
            {
                OnChangeDisplay?.Invoke(false, null);
                if (_selectedIndex != -1)
                    WindowsFunctions.Insert(_characters[_selectedIndex]);
                _selectedIndex = -1;
                _visible = false;
            }
        }

        return true;
    }

    public Point GetDisplayCoordinates(Size window)
    {
        var activeDisplay = WindowsFunctions.GetActiveDisplay();
        Rect screen = new Rect(activeDisplay.Location, activeDisplay.Size) / activeDisplay.Dpi;
        Position position = _settingService.Position;

#if DEBUG
        System.Diagnostics.Trace.WriteLine("Dpi: " + activeDisplay.Dpi);
#endif

        if (!_settingService.UseCaretPosition)
        {
            return Calculation.GetRawCoordinatesFromPosition(position, screen, window);
        }

        Point carretPixel = WindowsFunctions.GetCaretPosition();
        if (carretPixel.X == 0 && carretPixel.Y == 0)
        {
            return Calculation.GetRawCoordinatesFromPosition(position, screen, window);
        }

        Point dpi = new Point(activeDisplay.Dpi, activeDisplay.Dpi);
        Point caret = new Point(carretPixel.X / dpi.X, carretPixel.Y / dpi.Y);
        return Calculation.GetRawCoordinatesFromCaret(caret, screen, window);
    }

    public char[] GetLettersFromKey(LetterKey letter)
    {
        return _settingService.GetLetterKey(letter);
    }

    public void ReloadSettings()
    {
        _settingService.Reload();
    }

    public void Dispose()
    {
        _keyboardListener.Dispose();
    }
}
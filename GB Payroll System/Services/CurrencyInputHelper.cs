using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GB_Payroll_System.Services
{
    public static class CurrencyInputHelper
    {
        private static bool _isFormatting = false;

        public static readonly DependencyProperty IsCurrencyInputProperty =
            DependencyProperty.RegisterAttached(
                "IsCurrencyInput",
                typeof(bool),
                typeof(CurrencyInputHelper),
                new UIPropertyMetadata(false, OnIsCurrencyInputChanged));

        public static bool GetIsCurrencyInput(DependencyObject obj) =>
            (bool)obj.GetValue(IsCurrencyInputProperty);

        public static void SetIsCurrencyInput(DependencyObject obj, bool value) =>
            obj.SetValue(IsCurrencyInputProperty, value);

        private static void OnIsCurrencyInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if ((bool)e.NewValue)
                {
                    Attach(textBox);
                }
                else
                {
                    Detach(textBox);
                }
            }
        }

        public static void Attach(TextBox textBox)
        {
            textBox.PreviewTextInput -= TextBox_PreviewTextInput;
            textBox.TextChanged -= TextBox_TextChanged;
            textBox.LostFocus -= TextBox_LostFocus;
            textBox.GotFocus -= TextBox_GotFocus;

            textBox.PreviewTextInput += TextBox_PreviewTextInput;
            textBox.TextChanged += TextBox_TextChanged;
            textBox.LostFocus += TextBox_LostFocus;
            textBox.GotFocus += TextBox_GotFocus;
        }

        public static void Detach(TextBox textBox)
        {
            textBox.PreviewTextInput -= TextBox_PreviewTextInput;
            textBox.TextChanged -= TextBox_TextChanged;
            textBox.LostFocus -= TextBox_LostFocus;
            textBox.GotFocus -= TextBox_GotFocus;
        }

        private static void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox tb)
            {
                // Disallow second decimal point
                if (e.Text == "." && tb.Text.Contains('.'))
                {
                    e.Handled = true;
                    return;
                }
            }

            // Allow only numbers and period
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9.]+$");
        }

        private static void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isFormatting) return;
            if (sender is not TextBox tb) return;

            string originalText = tb.Text;
            if (string.IsNullOrWhiteSpace(originalText)) return;

            int originalCaret = tb.CaretIndex;

            // Count how many non-comma characters were before the cursor
            int rawCharsBeforeCaret = 0;
            for (int i = 0; i < originalCaret && i < originalText.Length; i++)
            {
                if (originalText[i] != ',') rawCharsBeforeCaret++;
            }

            // Split integer and decimal parts
            int dotIndex = originalText.IndexOf('.');
            string intPartRaw = dotIndex >= 0 ? originalText.Substring(0, dotIndex) : originalText;
            string decPart = dotIndex >= 0 ? originalText.Substring(dotIndex) : "";

            // Strip commas from integer part
            string cleanInt = Regex.Replace(intPartRaw, @"[^\d]", "");

            if (string.IsNullOrEmpty(cleanInt) && string.IsNullOrEmpty(decPart)) return;

            string formattedInt = "";
            if (!string.IsNullOrEmpty(cleanInt))
            {
                if (decimal.TryParse(cleanInt, out decimal intVal))
                {
                    formattedInt = intVal.ToString("#,##0"); // Adds thousand separator in real-time
                }
                else
                {
                    formattedInt = cleanInt;
                }
            }
            else if (dotIndex == 0)
            {
                formattedInt = "0";
            }

            // Limit decimal part to maximum 2 digits
            if (decPart.Length > 3) // e.g. ".123" -> truncate to ".12"
            {
                decPart = decPart.Substring(0, 3);
            }

            string newText = formattedInt + decPart;

            if (newText != originalText)
            {
                _isFormatting = true;
                tb.Text = newText;

                // Accurately restore caret position
                int newCaret = 0;
                int matchedRaw = 0;
                while (newCaret < newText.Length && matchedRaw < rawCharsBeforeCaret)
                {
                    if (newText[newCaret] != ',')
                    {
                        matchedRaw++;
                    }
                    newCaret++;
                }

                tb.CaretIndex = Math.Clamp(newCaret, 0, newText.Length);
                _isFormatting = false;
            }
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                FormatTextBoxAsCurrency(tb);
            }
        }

        public static void FormatTextBoxAsCurrency(TextBox tb)
        {
            if (string.IsNullOrWhiteSpace(tb.Text)) return;

            string clean = tb.Text.Replace(",", "").Trim();
            if (decimal.TryParse(clean, out decimal value))
            {
                _isFormatting = true;
                tb.Text = value.ToString("N2"); // Formats with full thousand comma and .00 centavos on blur
                _isFormatting = false;
            }
        }
    }
}

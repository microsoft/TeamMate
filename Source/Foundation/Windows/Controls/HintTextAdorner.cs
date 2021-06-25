using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// This class can be used to adorn WPF ComboBoxes or TextBoxes with some informational text.
    /// The text is applied when the control has not been filled in (i.e. the text property is empty)
    /// in the adorner layer.
    /// </summary>
    /// <remarks>
    /// Note: like all WPF adorners this one will not have any effect unless the control is surrounded by 
    /// and AdornerDecorator that provides the AdornerLayer.
    /// </remarks>
    public class HintTextAdorner : Adorner
    {
        private String hintText;
        private ComboBox comboBox;
        private TextBox textBox;
        private CultureInfo uiCulture;

        /// <summary>
        /// Associates a given control with the specified hint text.
        /// </summary>
        /// <param name="control">A control.</param>
        /// <param name="hintText">The hint text.</param>
        /// <returns><c>true</c> if the hint text was successfully associated; or <c>false</c> if the association
        /// failed (e.g. there was no adorner layer for the control).</returns>
        /// <remarks>
        /// Currently the controls supported include only combobox and textbox.
        /// </remarks>
        public static bool AddHintText(Control control, String hintText)
        {
            HintTextAdorner hintTextAdorner = null;
            return AddHintText(control, hintText, ref hintTextAdorner);
        }

        /// <summary>
        /// Associates a given control with the specified hint text.
        /// </summary>
        /// <param name="control">A control.</param>
        /// <param name="hintText">The hint text.</param>
        /// <param name="hintTextAdorner">created hint text adorner</param>
        /// <returns><c>true</c> if the hint text was successfully associated; or <c>false</c> if the association
        /// failed (e.g. there was no adorner layer for the control).</returns>
        /// <remarks>
        /// Currently the controls supported include only combobox and textbox.
        /// </remarks>
        public static bool AddHintText(Control control, String hintText, ref HintTextAdorner hintTextAdorner)
        {
            hintTextAdorner = null;
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(control);
            if (adornerLayer != null)
            {
                hintTextAdorner = new HintTextAdorner(control, hintText); 
                adornerLayer.Add(hintTextAdorner);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a hint text associated with a given control
        /// </summary>
        /// <param name="control">A control</param>
        public static void RemoveHintTexts(Control control)
        {
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(control);
            if (adornerLayer != null)
            {
                Adorner[] currentAdorners = adornerLayer.GetAdorners(control);
                if (currentAdorners != null)
                {
                    foreach (HintTextAdorner adorner in currentAdorners.OfType<HintTextAdorner>())
                    {
                        adornerLayer.Remove(adorner);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new instance that associates a hint text with a given control.
        /// </summary>
        /// <param name="control">The associated control.</param>
        /// <param name="hintText">The hint text.</param>
        public HintTextAdorner(Control control, String hintText)
            : base(control)
        {
            // Capture culture once, it could change upon multiple draws later and modify the way
            // we draw this string...
            this.uiCulture = CultureInfo.CurrentUICulture;
            this.hintText = hintText;
            IsHitTestVisible = false;

            // Hook common control events
            control.GotKeyboardFocus += HandleControlChanged;
            control.LostKeyboardFocus += HandleControlChanged;
            control.IsVisibleChanged += HandleControlChanged;

            // Assume the control is a combobox
            this.comboBox = control as ComboBox;
            if (this.comboBox != null)
            {
                this.comboBox.SelectionChanged += HandleControlChanged;
                this.comboBox.DropDownOpened += HandleControlChanged;
                this.comboBox.DropDownClosed += HandleControlChanged;
                return;
            }

            // Assume the control is a textbox
            this.textBox = control as TextBox;
            if (this.textBox != null)
            {
                this.textBox.TextChanged += HandleControlChanged;
                return;
            }

            // control is not supported
            Debug.Fail("HintTextAdorner: Unsupported control type - " + control.GetType().Name);
        }

        /// <summary>
        /// Determines whether the associated control is in a state that requires the hint text 
        /// to be displayed.
        /// </summary>
        public bool IsHintTextNeeded
        {
            get
            {
                if (this.comboBox != null && this.comboBox.SelectedIndex >= 0)
                {
                    return false;
                }

                if (this.textBox != null && (this.textBox.IsKeyboardFocused || this.textBox.Text.Length > 0))
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Get/Set the hint text
        /// </summary>
        public String HintText
        {
            get { return this.hintText; }
            set
            {
                this.hintText = value;
                InvalidateVisual();
            }
        }

        /// <summary>
        /// Get/Set the hint text font style
        /// </summary>
        public FontStyle HintTextFontStyle { get; set; }

        /// <summary>
        /// Overridden to render the hint text when appropriate.
        /// </summary>
        /// <param name="drawingContext">The drawing context.</param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            Control control = this.AdornedElement as Control;

            if (!IsHintTextNeeded || !control.IsVisible)
            {
                base.OnRender(drawingContext);
                return;
            }

            // Some arbitrary drawing implements.
            double leftTextPadding = 5.0;

            Brush renderBrush = control.Foreground.Clone();
            renderBrush.Opacity = 0.6;

            // Draw the hint text on top of the control
            FontStyle fontStyle = HintTextFontStyle;
            if (fontStyle == null)
            {
                fontStyle = control.FontStyle;
            }

            FormattedText text = new FormattedText(hintText, uiCulture, control.FlowDirection,
                    new Typeface(control.FontFamily, fontStyle, control.FontWeight, control.FontStretch),
                    control.FontSize, renderBrush);

            text.TextAlignment = GetTextAlignment();
            text.MaxTextHeight = Math.Max(control.ActualHeight, 1.0);
            text.MaxTextWidth = Math.Max(control.ActualWidth, leftTextPadding + 1.0) - leftTextPadding;
            text.MaxLineCount = 1;
            text.Trimming = TextTrimming.CharacterEllipsis;

            // Center the hint text vertically in the control
            double topTextPadding = Math.Max(control.ActualHeight - text.Height, 0.0) / 2;

            // If this is a multi-line control, it should draw the hint text in the first line
            if (control is TextBox && ((TextBox)control).TextWrapping != TextWrapping.NoWrap)
            {
                topTextPadding = 3.0;
            }
            else if (control is ComboBox)
            {
                // Subtract off the width of the combobox button. Using Vertical ScrollBarWidth for now (should be close).
                text.MaxTextWidth -= SystemParameters.VerticalScrollBarWidth;
            }

            Point topLeft = new Point(leftTextPadding, topTextPadding);

            if (control.FlowDirection == FlowDirection.RightToLeft)
            {
                // Somehow everything got drawn reflected. Add a transform to correct.
                // http://c-sharp-snippets.blogspot.com/2008/10/placeholder-text-using-adorner-improved.html
                topLeft.X = -topLeft.X;
                drawingContext.PushTransform(new ScaleTransform(-1.0, 1.0, RenderSize.Width / 2.0, 0.0));
                drawingContext.DrawText(text, topLeft);
                drawingContext.Pop();
            }
            else
            {
                drawingContext.DrawText(text, topLeft);
            }
        }

        /// <summary>
        /// Returns the alignment for the hint text.
        /// </summary>
        private TextAlignment GetTextAlignment()
        {
            return (textBox != null)? textBox.TextAlignment: TextAlignment.Left;
        }

        /// <summary>
        /// Handles control change events to invalidate the adorner.
        /// </summary>
        private void HandleControlChanged(object sender, EventArgs e)
        {
            this.InvalidateVisual();
        }

        /// <summary>
        /// Handles control change events to invalidate the adorner.
        /// </summary>
        private void HandleControlChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.InvalidateVisual();
        }
    }
}

using Microsoft.Tools.TeamMate.Foundation.Windows.Media.Animation;
using System;
using System.Globalization;
using System.Windows.Media.Animation;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls
{
    /// <summary>
    /// Interaction logic for BusySpinner.xaml
    /// </summary>
    public partial class BusySpinner
    {
        public BusySpinner()
        {
            InitializeComponent();

            Storyboard rotateStoryBoard = (Storyboard)Resources["Storyboard"];
            AnimationHelper helper = new AnimationHelper(this, rotateStoryBoard);
            helper.Enable();
        }
    }

    /// <summary>
    /// A converter used to adjust the scale of the busy spinner canvas size.
    /// </summary>
    internal class CanvasScaleConverter : OneWayConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double canvasWidthOrHeight = 120;
            double gridWidthOrHeight = (double)value;
            return gridWidthOrHeight / canvasWidthOrHeight;
        }
    }
}


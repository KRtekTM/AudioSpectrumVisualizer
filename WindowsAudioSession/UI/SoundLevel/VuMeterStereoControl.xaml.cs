using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace WindowsAudioSession.UI.SoundLevel
{
    /// <summary>
    /// vu-meter stereo control
    /// </summary>
    public partial class VuMeterStereoControl : UserControl, IVuMeterStereoControl
    {
        IVuMeterStereoViewModel _viewModel;
        /// <inheritdoc/>
        public IVuMeterStereoViewModel ViewModel
        {
            get => _viewModel;
            set
            {
                _viewModel = value;
                DataContext = _viewModel;
            }
        }

        /// <summary>
        /// creates a new instance of the control
        /// </summary>
        public VuMeterStereoControl()
        {
            InitializeComponent();
        }

        internal void ChangeBrush(Brush vuMeterBrush)
        {
            this.VuMeterLeft.VolumeVisual0.Fill = vuMeterBrush;
            this.VuMeterLeft.VolumeVisual1.Fill = vuMeterBrush;

            this.VuMeterRight.VolumeVisual0.Fill = vuMeterBrush;
            this.VuMeterRight.VolumeVisual0.Fill = vuMeterBrush;
        }
    }
}

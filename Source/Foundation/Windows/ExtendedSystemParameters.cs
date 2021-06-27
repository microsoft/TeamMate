using Microsoft.Tools.TeamMate.Foundation.Native;
using System;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    public static class ExtendedSystemParameters
    {
        public static bool IsSystemAnimationEnabled
        {
            get
            {
                ANIMATIONINFO animationInfo = ANIMATIONINFO.Create();
                NativeMethods.SystemParametersInfo(SPI.SPI_GETANIMATION, ANIMATIONINFO.Size, ref animationInfo, SPIF.NONE);
                return animationInfo.IsAnimationEnabled;
            }

            set
            {
                ANIMATIONINFO animationInfo = ANIMATIONINFO.Create(value);
                NativeMethods.SystemParametersInfo(SPI.SPI_SETANIMATION, ANIMATIONINFO.Size, ref animationInfo, SPIF.SPIF_SENDCHANGE);
            }
        }

        public static IDisposable DisableSystemAnimations()
        {
            return new SystemAnimationDisabler();
        }

        private class SystemAnimationDisabler : IDisposable
        {
            private bool wasSystemAnimationEnabled;

            public SystemAnimationDisabler()
            {
                wasSystemAnimationEnabled = ExtendedSystemParameters.IsSystemAnimationEnabled;
                if (wasSystemAnimationEnabled)
                {
                    ExtendedSystemParameters.IsSystemAnimationEnabled = false;
                }
            }

            public void Dispose()
            {
                if (wasSystemAnimationEnabled)
                {
                    ExtendedSystemParameters.IsSystemAnimationEnabled = true;
                }
            }
        }
    }
}

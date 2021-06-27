// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WMEncoderLib;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Runtime.InteropServices;

namespace Microsoft.Tools.TeamMate.Sandbox.Console.Windows.Media
{
    public static class EncoderConstants
    {
        public const string ScreenCaptureDevice = "ScreenCap://ScreenCapture1";
        public const string DefaultAudioDevice = "Device://Default_Audio_Device";

        public static readonly string[] DefaultProfileNames = new string[] {
            "Screen Video High (CBR)",
            "Screen Video/Audio High (CBR)",
            "Screen Video (CBR)",
            "Screen Video/Audio",
            "Screen Video Medium (CBR)",
            "Screen Video/Audio Medium (CBR)",
        };
    }

    public class RecordingSession2 : IDisposable
    {
        private const string DefaultVideoSource = EncoderConstants.ScreenCaptureDevice;
        private const string DefaultAudioSource = EncoderConstants.DefaultAudioDevice;

        private const string DefaultProfile = "Screen Video/Audio High (CBR)";

        private RecordingSessionInput input;
        private RecordingState state;
        private bool initialized;
        private object lockObj = new object();

        private WMEncoder encoder;
        private IWMEncSourceGroup mainSourceGroup;
        private IWMEncProfileCollection profileCollection;
        private IWMEncSourceGroupCollection sourceGroupCollection;
        private IWMEncAudioSource srcAudio;
        private IWMEncVideoSource2 srcVideo;

        public RecordingSessionInput Input
        {
            get { return this.input; }
        }

        public RecordingState State
        {
            get { return this.state; }
        }

        public void Start(RecordingSessionInput input)
        {
            if (!InteropUtilities.IsTypeRegistered(typeof(WMEncoder)))
            {
                throw new Exception("Encoder is not installed");
            }

            this.input = input;

            Initialize();

            this.encoder.File.LocalFileName = input.OutputFile;
            this.sourceGroupCollection.Active = this.mainSourceGroup;
            this.encoder.Start();
            WaitForEncoderState(WMENC_ENCODER_STATE.WMENC_ENCODER_RUNNING);
            this.state = RecordingState.Recording;
        }

        public void Stop()
        {
            this.encoder.Stop();
            this.input = null;
            WaitForEncoderState(WMENC_ENCODER_STATE.WMENC_ENCODER_STOPPED);
            this.state = RecordingState.Idle;
        }

        public void Pause()
        {
            this.encoder.Pause();
            WaitForEncoderState(WMENC_ENCODER_STATE.WMENC_ENCODER_PAUSED);
            this.state = RecordingState.Paused;
        }

        public void Reset()
        {
            this.encoder.Reset();
            this.input = null;
            this.state = RecordingState.Idle;
        }

        private void WaitForEncoderState(WMENC_ENCODER_STATE runState)
        {
            while (this.encoder.RunState != runState)
            {
                // TODO: Do we throw after a while?
                Thread.Sleep(20);
            }
        }

        private void Initialize()
        {
            this.encoder = new WMEncoderClass();
            this.sourceGroupCollection = this.encoder.SourceGroupCollection;
            this.profileCollection = this.encoder.ProfileCollection;

            ConfigureMainSourceGroup(input.CaptureBounds, input.CaptureWindow, input.CaptureAudio);
            ConfigureProfile();
            this.encoder.PrepareToEncode(true);

            this.initialized = true;
        }

        private void Release()
        {
            if (this.sourceGroupCollection != null)
            {
                try
                {
                    this.sourceGroupCollection.Remove(this.mainSourceGroup);
                }
                catch (Exception e)
                {
                    // TODO: this.context.Log.LogInformation(0, e.Message, new string[0]);
                }
            }

            if (this.mainSourceGroup != null)
            {
                try
                {
                    this.mainSourceGroup.set_Profile(null);
                }
                catch (Exception e)
                {
                    // TODO: this.context.Log.LogInformation(0, e.Message, new string[0]);
                }
            }

            ReleaseComObject(ref this.sourceGroupCollection);
            ReleaseComObject(ref this.mainSourceGroup);
            ReleaseComObject(ref this.srcVideo);
            ReleaseComObject(ref this.srcAudio);
            ReleaseComObject(ref this.profileCollection);
            ReleaseComObject(ref this.encoder);

            GC.WaitForPendingFinalizers();
            this.initialized = false;
        }

        /*
         * TODO: Initialize without audio
        public void InitializeRecordingSession(Rectangle rect, IntPtr recordingHandle)
        {
            if (!this.initialized)
            {
                try
                {
                    this.InitializeSession(rect, recordingHandle, true);
                }
                catch (COMException e1)
                {
                    if (((e1.ErrorCode != -1072882849) && (e1.ErrorCode != -1072889742)) && (string.Compare(e1.Message, "No specified device driver is present.", true) != 0))
                    {
                        throw;
                    }
                    this.InitializeSession(rect, recordingHandle, false);
                }
            }
        }
         */

        private void ConfigureProfile()
        {
            IWMEncProfile inputProfile = null;

            if (!String.IsNullOrEmpty(input.PreferredRecordingProfile))
                inputProfile = LoadProfile(input.PreferredRecordingProfile);

            if (inputProfile == null)
                inputProfile = LoadProfile(DefaultProfile);

            if (inputProfile == null)
                throw new Exception("Couldn't find profile!");

            int height, width;
            IWMEncProfile2 customProfile = AdjustProfile(inputProfile, out height, out width);
            this.mainSourceGroup.set_Profile(customProfile);


            ReleaseComObject(ref inputProfile);
            ReleaseComObject(ref customProfile);
        }

        private IWMEncProfile2 AdjustProfile(IWMEncProfile profile, out int height, out int width)
        {
            IWMEncProfile2 customProfile = new WMEncProfile2Class();
            customProfile.LoadFromIWMProfile(profile);
            this.GetVideoSize(out width, out height);
            if (width > 0 && height > 0)
            {
                this.SetAudienceVideoSize(customProfile, width, height);
            }
            return customProfile;
        }

        private void SetAudienceVideoSize(IWMEncProfile2 profile, int width, int height)
        {
            int count = profile.AudienceCount;
            for (int i = 0; i < count; i++)
            {
                IWMEncAudienceObj audience = profile.get_Audience(i);
                try
                {
                    audience.set_VideoWidth(0, width);
                    audience.set_VideoHeight(0, height);
                }
                finally
                {
                    ReleaseComObject(ref audience);
                }
            }
        }

        private IWMEncProfile LoadProfile(string profileName)
        {
            for (int i = 0; i < this.profileCollection.Count; i++)
            {
                IWMEncProfile profile = this.profileCollection.Item(i);
                Debug.WriteLine(profile.Name);
            }

            for (int i = 0; i < this.profileCollection.Count; i++)
            {
                IWMEncProfile profile = this.profileCollection.Item(i);
                if (string.Compare(profile.Name, profileName, true) == 0)
                {
                    return profile;
                }

                Marshal.ReleaseComObject(profile);
                profile = null;
            }

            return null;
        }

        private void GetVideoSize(out int width, out int height)
        {
            height = input.CaptureBounds.Height;
            width = input.CaptureBounds.Width;

            /* TODO:
            Screen screen = Screen.PrimaryScreen;
            if (screen.WorkingArea.Width < width)
            {
                width = screen.WorkingArea.Width;
            }
            if (screen.WorkingArea.Height < height)
            {
                height = screen.WorkingArea.Height;
            }
             */
        }


        private void ConfigureMainSourceGroup(Rectangle bounds, IntPtr windowHandle, bool captureAudio)
        {
            this.mainSourceGroup = this.sourceGroupCollection.Add("Main");
            this.srcVideo = (IWMEncVideoSource2)this.mainSourceGroup.AddSource(WMENC_SOURCE_TYPE.WMENC_VIDEO);
            this.srcVideo.SetInput(DefaultVideoSource, "", "");

            ConfigureScreenBounds(bounds, windowHandle);

            if (captureAudio)
            {
                this.srcAudio = (IWMEncAudioSource)this.mainSourceGroup.AddSource(WMENC_SOURCE_TYPE.WMENC_AUDIO);
                this.srcAudio.SetInput(DefaultAudioSource, "", "");
            }
        }

        private Rectangle ConfigureScreenBounds(Rectangle recordingBounds, IntPtr capturedWindowHandle)
        {
            if (capturedWindowHandle != IntPtr.Zero)
            {
                this.SetScreenRegion(this.srcVideo, capturedWindowHandle);
            }
            else if (recordingBounds == Rectangle.Empty)
            {
                this.SetScreenRegionIfMultipleMon(this.srcVideo);
            }
            else
            {
                this.SetScreenRegion(this.srcVideo, recordingBounds, false);
            }
            return recordingBounds;
        }

        private void SetScreenRegion(IWMEncVideoSource2 srcVideo, IntPtr handle)
        {
            UnsafeNativeMethods.IPropertyBag propertyBag = (UnsafeNativeMethods.IPropertyBag)srcVideo;
            propertyBag.SetProperty("Screen", false);
            propertyBag.SetProperty("CaptureWindow", handle.ToInt32());
            propertyBag.SetProperty("FlashRect", true);
            Marshal.ReleaseComObject(propertyBag);
        }

        private void SetScreenRegion(IWMEncVideoSource2 srcVideo, Rectangle rect, bool useFullScreen)
        {
            UnsafeNativeMethods.IPropertyBag propertyBag = (UnsafeNativeMethods.IPropertyBag)srcVideo;
            object left = (rect.Left > rect.Right) ? rect.Right : rect.Left;
            object top = (rect.Top > rect.Bottom) ? rect.Bottom : rect.Top;
            object right = (rect.Right < rect.Left) ? rect.Left : rect.Right;
            object bottom = (rect.Bottom < rect.Top) ? rect.Top : rect.Bottom;
            object fullScreen = false;
            object flashRect = (!useFullScreen) ? true : false;

            propertyBag.SetProperty("Screen", fullScreen);
            propertyBag.SetProperty("Left", left);
            propertyBag.SetProperty("Right", right);
            propertyBag.SetProperty("Top", top);
            propertyBag.SetProperty("Bottom", bottom);
            propertyBag.SetProperty("FlashRect", flashRect);
            Marshal.ReleaseComObject(propertyBag);
        }

        private void SetScreenRegionIfMultipleMon(IWMEncVideoSource2 srcVideo)
        {
            /*
             * TODO: Support full screen capture if no bounds are passed? Are multiple monitors not supported?

            if (Screen.AllScreens.Length > 1)
            {
                Screen screen = Screen.PrimaryScreen;
                this.SetScreenRegion(srcVideo, screen.Bounds, true);
            }
             */
        }

        private void ReleaseComObject<T>(ref T o)
        {
            if (o != null)
            {
                Marshal.ReleaseComObject(o);
                o = default(T);
            }
        }

        #region IDisposable Members

        // Track whether Dispose has been called.
        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to take this object off the finalization queue
            // and prevent finalization code for this object from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!this.disposed)
            {
                // If disposing equals true, dispose all managed and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    // TODO: component.Dispose();
                }

                // Call the appropriate methods to clean up unmanaged resources here.
                // If disposing is false, only the following code is executed.
                Release();

                // Note disposing has been done.
                this.disposed = true;
            }
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~RecordingSession2()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of readability and maintainability.
            Dispose(false);
        }
        #endregion
    }

    public enum RecordingState
    {
        Idle = 0,
        Recording = 1,
        Paused = 2,
    }

    public class RecordingSessionInput
    {
        public string OutputFile { get; set; }
        public bool CaptureAudio { get; set; }
        public string PreferredRecordingProfile { get; set; }

        public bool CaptureFullScreen { get; set; }
        public Rectangle CaptureBounds { get; set; }
        public IntPtr CaptureWindow { get; set; }
        public bool DisplayFlashRect { get; set; }
    }

    internal static class UnsafeNativeMethods
    {
        public static void SetProperty(this IPropertyBag propertyBag, string property, object value)
        {
            object tempValue = value;
            propertyBag.Write(property, ref tempValue);
        }

        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("3127CA40-446E-11CE-8135-00AA004BB851")]
        public interface IErrorLog
        {
            void AddError([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName_p0, [In, MarshalAs(UnmanagedType.Struct)] UnsafeNativeMethods.tagEXCEPINFO pExcepInfo_p1);
        }

        [ComImport, Guid("55272A00-42CB-11CE-8135-00AA004BB851"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPropertyBag
        {
            [PreserveSig]
            int Read([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName, [In, Out] ref object pVar, [In] UnsafeNativeMethods.IErrorLog pErrorLog);
            [PreserveSig]
            int Write([In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName, [In] ref object pVar);
        }

        [StructLayout(LayoutKind.Sequential)]
        public class tagEXCEPINFO
        {
            [MarshalAs(UnmanagedType.U2)]
            public short wCode;
            [MarshalAs(UnmanagedType.U2)]
            public short wReserved;
            [MarshalAs(UnmanagedType.BStr)]
            public string bstrSource;
            [MarshalAs(UnmanagedType.BStr)]
            public string bstrDescription;
            [MarshalAs(UnmanagedType.BStr)]
            public string bstrHelpFile;
            [MarshalAs(UnmanagedType.U4)]
            public int dwHelpContext;
            public IntPtr pvReserved = IntPtr.Zero;
            public IntPtr pfnDeferredFillIn = IntPtr.Zero;
            [MarshalAs(UnmanagedType.U4)]
            public int scode;
        }
    }

}

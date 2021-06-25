using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Drawing;
using WMEncoderLib;

namespace Microsoft.Office.OfficeLabs.CommunityClips
{
    internal class RecordingSession : IDisposable
    {
        private static string Setting_DefaultVideoSource = "ScreenCap://ScreenCapture1";
        private static string Setting_DefaultAudioSource = "Device://Default_Audio_Device";
        private static int Setting_VideoWidth = 800;
        private static int Setting_VideoHeight = 600;
        private static string Setting_ProfileName = null;
        private static bool Setting_DoNotShowStartDialog = true;
        private static string Setting_MediaLocation = @"E:\Temp\Temp";
        private static string Save_Video_As_DefaultExt = ".avi";

        private bool bNotificationsActive;
        private WMEncoder encoder;
        private bool initialized;
        private object lockObj = new object();
        private IWMEncSourceGroup mainSourceGroup;
        private int numPauses;
        private IWMEncProfileCollection profileCollection;
        private List<string> programList;
        private int recordingType;
        private string REGKEY_USER_SETTINGS_BASE = @"Software\Microsoft\Office Labs\CommunityClips\1.0";
        private IWMEncSourceGroupCollection sourceGroupCollection;
        private IWMEncAudioSource srcAudio;
        private IWMEncVideoSource2 srcVideo;
        private long startDurationTickCount;
        private long startSessionDurationTickCount;
        private RecordingState state;
        private string tmpFilePath;
        private long totalDurationTickCount;
        private string WMEncoderRegKey = @"Software\Microsoft\Windows Media\Encoder";

        public event RecordingStateChangedEventHandler SessionStateChanged;

        public RecordingSession()
        {
            this.InstallWMEncoderIfRequired();
        }

        private void AddMainSourceGroup(Rectangle rect, IntPtr recordingHandle, bool bUseAudio)
        {
            this.mainSourceGroup = this.sourceGroupCollection.Add("Main");
            this.srcVideo = (IWMEncVideoSource2)this.mainSourceGroup.AddSource(WMENC_SOURCE_TYPE.WMENC_VIDEO);
            this.srcVideo.SetInput(Setting_DefaultVideoSource, "", "");
            if (recordingHandle != IntPtr.Zero)
            {
                this.SetScreenRegion(this.srcVideo, recordingHandle);
            }
            else if (rect == Rectangle.Empty)
            {
                this.SetScreenRegionIfMultipleMon(this.srcVideo);
            }
            else
            {
                Rectangle r2 = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
                this.SetScreenRegion(this.srcVideo, r2, false);
            }
            if (bUseAudio)
            {
                this.srcAudio = (IWMEncAudioSource)this.mainSourceGroup.AddSource(WMENC_SOURCE_TYPE.WMENC_AUDIO);
                this.srcAudio.SetInput(Setting_DefaultAudioSource, "", "");
            }
        }

        private Dictionary<string, string> CreateLookupTable()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            this.UninitalizeRecordingSession();
            if (!string.IsNullOrEmpty(this.tmpFilePath))
            {
                try
                {
                    if (File.Exists(this.tmpFilePath))
                    {
                        // TODO: Don't want to delete temp object
                        // File.Delete(this.tmpFilePath);
                    }
                }
                catch (Exception e1)
                {
                    // TODO: this.context.Log.LogInformation(0, e1.ToString(), new string[0]);
                }
            }
        }

        internal List<string> GetActiveWindowNames()
        {
            return this.programList;
        }

        private string GetInstallDirectory()
        {
            return (string)Registry.CurrentUser.OpenSubKey(this.REGKEY_USER_SETTINGS_BASE, true).GetValue("InstallDirectory");
        }

        private static string GetTempFileName(string folder)
        {
            int i = 1;
            while (true)
            {
                string fileName = Path.Combine(folder, "ScreenCast" + i.ToString() + Save_Video_As_DefaultExt);
                if (!File.Exists(fileName))
                {
                    return fileName;
                }
                i++;
            }
        }

        private void GetVideoHeightAndWidth(out int height, out int width)
        {
            height = (int)Setting_VideoHeight;
            width = (int)Setting_VideoWidth;

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

        private void InitializeSession(Rectangle rect, IntPtr recordingHandle, bool bUseAudio)
        {
            this.state = RecordingState.Idle;
            this.UninitalizeRecordingSession();
            this.encoder = new WMEncoderClass();
            this.sourceGroupCollection = this.encoder.SourceGroupCollection;
            this.profileCollection = this.encoder.ProfileCollection;
            this.AddMainSourceGroup(rect, recordingHandle, bUseAudio);
            this.SetRecordingProfileToUse();
            this.encoder.PrepareToEncode(true);
            this.initialized = true;
        }

        private void InstallWMEncoderIfRequired()
        {
            try
            {
                if (Registry.LocalMachine.OpenSubKey(this.WMEncoderRegKey) == null)
                {
                    string exePath = this.GetInstallDirectory();
                    Process configure = new Process
                    {
                        StartInfo = { FileName = "wmencoder.exe", Arguments = "/Q", WorkingDirectory = exePath, WindowStyle = ProcessWindowStyle.Hidden }
                    };
                    configure.Start();
                    configure.WaitForExit();
                }

                // TODO:
                // string targetFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), @"Windows Media Components\Encoder\Profiles\commclips_V2.prx");
                // if (!File.Exists(targetFile))
                // {
                   //  File.Copy(Path.Combine(this.GetInstallDirectory(), "commclips_V2.prx"), targetFile);
                // }
            }
            catch
            {
                /*
                 * // TODO: 
                if (this.context != null)
                {
                    this.context.ApplicationSqmSession[0x26].IncrementBy((uint)1);
                }
                 */
            }
        }

        private void myNotifyProc(string programName)
        {
            this.programList.Add(programName);
        }

        public void Pause()
        {
            this.Pause(true);
        }

        private void Pause(bool fireEvent)
        {
            // TODO: this.context.Log.LogVerbose(0, "Pausing...", new string[0]);
            if (this.state == RecordingState.Recording)
            {
                lock (this.lockObj)
                {
                    this.numPauses++;
                    this.totalDurationTickCount += DateTime.Now.Ticks - this.startDurationTickCount;
                    // TODO: this.context.Log.LogVerbose(0, string.Concat(new object[] { "The encoder state is: ", this.encoder.RunState, "\t The recording state is: ", this.state }), new string[0]);
                    this.encoder.Pause();
                    while (this.encoder.RunState != WMENC_ENCODER_STATE.WMENC_ENCODER_PAUSED)
                    {
                        Thread.Sleep(20);
                    }

                    this.state = RecordingState.Paused;
                    if (fireEvent && (this.SessionStateChanged != null))
                    {
                        this.SessionStateChanged(this, new RecordingStateChangedEventArgs(this.state));
                    }
                }
            }
            // TODO: this.context.Log.LogVerbose(0, "Paused.", new string[0]);
        }

        private IWMEncProfile2 PrepareProfile(string profileName)
        {
            IWMEncProfile2 customProfile = null;
            for (int i = 0; i < this.profileCollection.Count; i++)
            {
                IWMEncProfile profile = this.profileCollection.Item(i);
                if (string.Compare(profile.Name, profileName, true) == 0)
                {
                    int height;
                    int width;
                    customProfile = new WMEncProfile2Class();
                    customProfile.LoadFromIWMProfile(profile);
                    this.GetVideoHeightAndWidth(out height, out width);
                    if ((height > 0) && (width > 0))
                    {
                        this.SetAudienceVideoHeightWidth(customProfile, height, width);
                    }
                    Marshal.ReleaseComObject(profile);
                    profile = null;
                    break;
                }
                Marshal.ReleaseComObject(profile);
                profile = null;
            }
            if (customProfile == null)
            {
                return this.PrepareProfile("Screen Video/Audio High (CBR)");
            }
            return customProfile;
        }

        private void SetAudienceVideoHeightWidth(IWMEncProfile2 profile, int height, int width)
        {
            int count = profile.AudienceCount;
            for (int i = 0; i < count; i++)
            {
                IWMEncAudienceObj audience = profile.get_Audience(i);
                try
                {
                    audience.set_VideoHeight(0, height);
                    audience.set_VideoWidth(0, width);
                }
                finally
                {
                    if (audience != null)
                    {
                        Marshal.ReleaseComObject(audience);
                        audience = null;
                    }
                }
            }
        }

        private void SetRecordingProfileToUse()
        {
            IWMEncProfile2 customProfile = this.PrepareProfile(Setting_ProfileName);
            if (customProfile != null)
            {
                this.mainSourceGroup.set_Profile(customProfile);
                Marshal.ReleaseComObject(customProfile);
                customProfile = null;
            }
        }

        public void SetScreenRegion(IWMEncVideoSource2 srcVideo, IntPtr handle)
        {
            UnsafeNativeMethods.IPropertyBag props = (UnsafeNativeMethods.IPropertyBag)srcVideo;
            object fullScreen = false;
            props.Write("Screen", ref fullScreen);
            object value = handle.ToInt32();
            props.Write("CaptureWindow", ref value);
            object flashRect = true;
            props.Write("FlashRect", ref flashRect);
            Marshal.ReleaseComObject(props);
        }

        public void SetScreenRegion(IWMEncVideoSource2 srcVideo, Rectangle rect, bool useFullScreen)
        {
            UnsafeNativeMethods.IPropertyBag props = (UnsafeNativeMethods.IPropertyBag)srcVideo;
            object left = (rect.Left > rect.Right) ? rect.Right : rect.Left;
            object top = (rect.Top > rect.Bottom) ? rect.Bottom : rect.Top;
            object right = (rect.Right < rect.Left) ? rect.Left : rect.Right;
            object bottom = (rect.Bottom < rect.Top) ? rect.Top : rect.Bottom;
            object fullScreen = false;
            object flashRect = true;
            if (useFullScreen)
            {
                flashRect = false;
            }
            props.Write("Screen", ref fullScreen);
            props.Write("Left", ref left);
            props.Write("Right", ref right);
            props.Write("Top", ref top);
            props.Write("Bottom", ref bottom);
            props.Write("FlashRect", ref flashRect);
            Marshal.ReleaseComObject(props);
        }

        public void SetScreenRegionIfMultipleMon(IWMEncVideoSource2 srcVideo)
        {
            /* TODO:
            if (Screen.AllScreens.Length > 1)
            {
                Screen screen = Screen.PrimaryScreen;
                this.SetScreenRegion(srcVideo, screen.Bounds, true);
            }
             */
        }

        public void Start(Rectangle recordingRegion, IntPtr recordingWindowHandle)
        {
            this.Start(recordingRegion, recordingWindowHandle, true);
        }

        private void Start(Rectangle recordingRegion, IntPtr recordingWindowHandle, bool fireEvent)
        {
            // TODO: this.context.Log.LogVerbose(0, "Starting...", null);
            lock (this.lockObj)
            {
                switch (this.state)
                {
                    case RecordingState.Idle:
                        try
                        {
                            this.startSessionDurationTickCount = DateTime.Now.Ticks;
                            this.numPauses = 0;
                            this.totalDurationTickCount = 0L;
                            this.InitializeRecordingSession(recordingRegion, recordingWindowHandle);
                            if (recordingRegion != Rectangle.Empty)
                            {
                                this.recordingType = 2;
                            }
                            else if (recordingWindowHandle != IntPtr.Zero)
                            {
                                this.recordingType = 3;
                            }
                            else
                            {
                                this.recordingType = 1;
                            }
                        }
                        catch (Exception e)
                        {
                            // TODO: MessageBox.Show(string.Format("Failed to start recording. Error:{0}. Please try again. Please report the problem to the Community Clips team, if it happens again.", e.Message), Resources.App_Name, MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            // TODO: this.context.Log.LogError(0, "{0}", new string[] { e.ToString() });
                            return;
                        }
                        if (!Setting_DoNotShowStartDialog)
                        {
                            //TODO: new frmStartRecording().ShowDialog();
                        }
                        if (!string.IsNullOrEmpty(this.tmpFilePath))
                        {
                            try
                            {
                                File.Delete(this.tmpFilePath);
                            }
                            catch (Exception e1)
                            {
                                //TODO: MessageBox.Show(e1.Message, this.tmpFilePath);
                            }
                        }
                        if (!Directory.Exists(Setting_MediaLocation))
                        {
                            Directory.CreateDirectory(Setting_MediaLocation);
                        }
                        this.tmpFilePath = GetTempFileName(Setting_MediaLocation);
                        this.encoder.File.LocalFileName = this.tmpFilePath;
                        this.sourceGroupCollection.Active = this.mainSourceGroup;
                        break;

                    case RecordingState.Paused:
                        break;

                    default:
                        goto Label_0242;
                }
                this.startDurationTickCount = DateTime.Now.Ticks;
                this.encoder.Start();
                while (this.encoder.RunState != WMENC_ENCODER_STATE.WMENC_ENCODER_RUNNING)
                {
                    Thread.Sleep(20);
                }
                if (!this.bNotificationsActive)
                {
                    this.bNotificationsActive = true;
                }
                this.state = RecordingState.Recording;
                if (fireEvent && (this.SessionStateChanged != null))
                {
                    this.SessionStateChanged(this, new RecordingStateChangedEventArgs(this.state));
                }
            Label_0242: ;
            }
            // TODO: this.context.Log.LogVerbose(0, "Started.", new string[0]);
        }

        public void Stop()
        {
            this.Stop(true);
        }

        private void Stop(bool fireEvent)
        {
            // TODO: this.context.Log.LogVerbose(0, "Stopping...", new string[0]);
            if ((this.state == RecordingState.Recording) || (this.state == RecordingState.Paused))
            {
                lock (this.lockObj)
                {
                    TimeSpan duration;
                    if (this.state == RecordingState.Recording)
                    {
                        duration = new TimeSpan(this.totalDurationTickCount + (DateTime.Now.Ticks - this.startDurationTickCount));
                    }
                    else
                    {
                        duration = new TimeSpan(this.totalDurationTickCount);
                    }
                    TimeSpan sessionDuration = new TimeSpan(DateTime.Now.Ticks - this.startSessionDurationTickCount);
                    this.bNotificationsActive = false;
                    this.encoder.Stop();
                    while (this.encoder.RunState != WMENC_ENCODER_STATE.WMENC_ENCODER_STOPPED)
                    {
                        Thread.Sleep(10);
                    }
                    this.state = RecordingState.Idle;
                    if (fireEvent && (this.SessionStateChanged != null))
                    {
                        this.SessionStateChanged(this, new RecordingStateChangedEventArgs(this.state));
                    }
                    this.UninitalizeRecordingSession();
                    // TODO: this.context.ApplicationSqmSession[0x3b].AddToStream(new int[] { this.recordingType, (int)duration.TotalSeconds, (int)sessionDuration.TotalSeconds, this.numPauses });
                }
            }
            // TODO: this.context.Log.LogVerbose(0, "Stopped.", new string[0]);
        }

        public void UninitalizeRecordingSession()
        {
            if (this.state == RecordingState.Recording)
            {
                this.Stop(false);
            }
            if (this.sourceGroupCollection != null)
            {
                try
                {
                    this.sourceGroupCollection.Remove(this.mainSourceGroup);
                    Marshal.ReleaseComObject(this.sourceGroupCollection);
                    this.sourceGroupCollection = null;
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
                    Marshal.ReleaseComObject(this.mainSourceGroup);
                    this.mainSourceGroup = null;
                }
                catch (Exception e)
                {
                    // TODO: this.context.Log.LogInformation(0, e.Message, new string[0]);
                }
            }
            if (this.srcVideo != null)
            {
                Marshal.ReleaseComObject(this.srcVideo);
                this.srcVideo = null;
            }
            if (this.srcAudio != null)
            {
                Marshal.ReleaseComObject(this.srcAudio);
                this.srcAudio = null;
            }
            if (this.profileCollection != null)
            {
                Marshal.ReleaseComObject(this.profileCollection);
                this.profileCollection = null;
            }
            if (this.encoder != null)
            {
                Marshal.ReleaseComObject(this.encoder);
                this.encoder = null;
            }
            //TODO: Application.DoEvents();
            GC.WaitForPendingFinalizers();
            this.initialized = false;
        }

        public WMEncoder Encoder
        {
            get
            {
                return this.encoder;
            }
        }

        public RecordingState State
        {
            get
            {
                return this.state;
            }
        }

        public string TmpFilePath
        {
            get
            {
                return this.tmpFilePath;
            }
            set
            {
                this.tmpFilePath = value;
            }
        }
    }

    public delegate void RecordingStateChangedEventHandler(object sender, RecordingStateChangedEventArgs args);

    [Serializable]
    public class RecordingStateChangedEventArgs : EventArgs
    {
        private RecordingState state;

        public RecordingStateChangedEventArgs(RecordingState state)
        {
            this.state = state;
        }

        public RecordingState State
        {
            get
            {
                return this.state;
            }
        }
    }

    public enum RecordingState
    {
        Idle = 0,
        Recording = 1,
        Paused = 2,
    }

    internal static class UnsafeNativeMethods
    {
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


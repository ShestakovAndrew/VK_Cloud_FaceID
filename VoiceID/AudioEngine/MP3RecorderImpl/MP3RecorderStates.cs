﻿using NAudio.CoreAudioApi;
using NAudio.Lame;
using NAudio.Wave;

namespace AuthFaceIDModernUI.VoiceID.AudioEngine.MP3RecorderImpl
{
    internal abstract class AMP3RecorderState
    {
        protected InternalData Data;

        protected AMP3RecorderState(InternalData data)
        {
            Data = data;
        }

        public abstract void StartRecording();

        public abstract void StopRecording();

        public abstract bool IsActive();

        protected void Dispose()
        {
            Data.WaveIn?.Dispose();
            Data.WaveIn = null;
            Data.Writer?.Close();
            Data.Writer = null;
        }
    }

    internal class RecordingState : AMP3RecorderState
    {
        public RecordingState(InternalData data)
            : base(data)
        { }

        public override void StartRecording()
        {
            throw new InvalidOperationException("ERROR: The recording is already started.");
        }

        public override void StopRecording()
        {
            Data.WaveIn?.StopRecording();
            Dispose();
        }

        public override bool IsActive()
        {
            return true;
        }
    }

    internal class StoppedState : AMP3RecorderState
    {
        public StoppedState(InternalData data)
            : base(data)
        { }

        public override void StartRecording()
        {
            InitRecorder();
            Data.WaveIn?.StartRecording();
        }

        public override void StopRecording()
        {
            throw new InvalidOperationException("ERROR: The recording is already Stopped.");
        }

        public override bool IsActive()
        {
            return false;
        }

        private void InitRecorder()
        {
            if (Data.WaveIn != null) { throw new InvalidOperationException("ERROR: Data.WaveIn should be null."); }
            Data.WaveIn = new WasapiCapture((Data.ActiveDevice as Microphone).Device);

            if (Data.Writer != null) { throw new InvalidOperationException("ERROR: Data.Writer should be null."); }
            Data.Writer = new LameMP3FileWriter(Data.OutFileName, Data.WaveIn.WaveFormat, 128);

            Data.WaveIn.DataAvailable += AvailableDataHandler!;
            Data.WaveIn.RecordingStopped += StopRecordingHandler!;
        }

        private void AvailableDataHandler(object sender, WaveInEventArgs e)
        {
            Data.Writer?.Write(e.Buffer, 0, e.BytesRecorded);
        }

        private static void StopRecordingHandler(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                throw new Exception($"ERROR: A problem was encountered during recording {e.Exception.Message}", e.Exception);
            }
        }
    }
}
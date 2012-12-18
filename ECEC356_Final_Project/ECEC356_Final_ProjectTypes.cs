//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ECEC356_Final_ProjectTypes.cs $ $Revision: 11 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using System;
using System.Collections.Generic;
using W3C.Soap;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using MicArray = Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy;
using System.ComponentModel;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.DsspHttp;
using Microsoft.Dss.Core.Utilities;

namespace ECEC356_Final_Project
{
    /// <summary>
    /// ECEC356_Final_Project Contract
    /// </summary>
    public sealed class Contract
    {
        /// <summary>
        /// Unique ECEC356_Final_Project contract identifier
        /// </summary>
        public const string Identifier = "http://schemas.microsoft.com/robotics/simulation/services/2006/05/ecec356_final_project.html";
    }

    /// <summary>
    /// ECEC356_Final_Project state
    /// </summary>
    [DataContract]
    public class ECEC356_Final_ProjectState
    {
        private MicArray.SpeechRecognizerState speechRecognizerState;

        /// <summary>
        /// The speech recognizer's state
        /// </summary>
        [DataMember]
        [Description("The speech recognizer's state.")]
        public MicArray.SpeechRecognizerState SpeechRecognizerState
        {
            get { return this.speechRecognizerState; }
            set { this.speechRecognizerState = value; }
        }

        private List<EventListEntry> speechEvents = new List<EventListEntry>();

        /// <summary>
        /// Past speech events received from speech recognizer
        /// </summary>
        [DataMember]
        [Description("Past speech events received from speech recognizer.")]
        public List<EventListEntry> SpeechEvents
        {
            get { return this.speechEvents; }
            set { this.speechEvents = value; }
        }

        private double _angle;
        
        /// <summary>
        /// Angle
        /// </summary>
        [DataMember]
        public double Angle
        {
            get { return _angle; }
            set { _angle = value; }
        }
    }

    #region EventListEntry class, needed to serialize a list of mixed types
    /// <summary>
    /// EventListEntry - A list of events that have occurred
    /// </summary>
    [DataContract]
    public class EventListEntry
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EventListEntry()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Detected notification event</param>
        public EventListEntry(MicArray.SpeechDetectedNotification content)
        {
            this.SpeechDetected = content;
            this.Timestamp = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Recognized notification event</param>
        public EventListEntry(MicArray.SpeechRecognizedNotification content)
        {
            this.SpeechRecognized = content;
            this.Timestamp = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Rejected notification event</param>
        public EventListEntry(MicArray.SpeechRecognitionRejectedNotification content)
        {
            this.RecognitionRejected = content;
            this.Timestamp = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Beam changed notification event</param>
        public EventListEntry(MicArray.BeamDirectionChangedNotification content)
        {
            this.BeamDirectionChanged = content;
            this.Timestamp = DateTime.Now.Ticks;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="content">Info on the recognized speech</param>
        public EventListEntry(MicArray.SpeechInformation content)
        {
            this.SpeechInformation = content;
            this.Timestamp = DateTime.Now.Ticks;
        }

        private long timestamp;

        /// <summary>
        /// Timestamp when the event occurred
        /// </summary>
        [DataMember]
        public long Timestamp
        {
            get { return this.timestamp; }
            set { this.timestamp = value; }
        }

        private MicArray.SpeechDetectedNotification speechDetected;

        /// <summary>
        /// Speech Detected Notification
        /// </summary>
        [DataMember]
        public MicArray.SpeechDetectedNotification SpeechDetected
        {
            get { return this.speechDetected; }
            set { this.speechDetected = value; }
        }

        private MicArray.SpeechRecognizedNotification speechRecognized;

        /// <summary>
        /// Speech Recognized Notification
        /// </summary>
        [DataMember]
        public MicArray.SpeechRecognizedNotification SpeechRecognized
        {
            get { return this.speechRecognized; }
            set { this.speechRecognized = value; }
        }

        private MicArray.SpeechRecognitionRejectedNotification recognitionRejected;

        /// <summary>
        /// Speech Rejected Notification
        /// </summary>
        [DataMember]
        public MicArray.SpeechRecognitionRejectedNotification RecognitionRejected
        {
            get { return this.recognitionRejected; }
            set { this.recognitionRejected = value; }
        }

        private MicArray.BeamDirectionChangedNotification beamDirectionChanged;

        /// <summary>
        /// Speech Detected Notification
        /// </summary>
        [DataMember]
        public MicArray.BeamDirectionChangedNotification BeamDirectionChanged
        {
            get { return this.beamDirectionChanged; }
            set { this.beamDirectionChanged = value; }
        }

        private MicArray.SpeechInformation speechInformation;

        /// <summary>
        /// Speech Information
        /// </summary>
        [DataMember]
        public MicArray.SpeechInformation SpeechInformation
        {
            get { return this.speechInformation; }
            set { this.speechInformation = value; }
        }
    }
    #endregion

    /// <summary>
    /// ECEC356_Lab3_q5 main operations port
    /// </summary>
    [ServicePort]
    public class ECEC356_Final_ProjectOperations : PortSet<
        DsspDefaultLookup,
        DsspDefaultDrop,
        Get,
        Subscribe,
        HttpGet,
        Replace,
        HttpQuery,
        HttpPost
        >
    {
    }

    /// <summary>
    /// ECEC356_Lab3_q5 get operation
    /// </summary>
    public class Get : Get<GetRequestType, PortSet<ECEC356_Final_ProjectState, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        public Get()
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">the request message body</param>
        public Get(GetRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Get
        /// </summary>
        /// <param name="body">the request message body</param>
        /// <param name="responsePort">the response port for the request</param>
        public Get(GetRequestType body, PortSet<ECEC356_Final_ProjectState, Fault> responsePort)
            : base(body, responsePort)
        {
        }
    }

    /// <summary>
    /// ECEC356_Lab3_q5 subscribe operation
    /// </summary>
    public class Subscribe : Subscribe<SubscribeRequestType, PortSet<SubscribeResponseType, Fault>>
    {
        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        public Subscribe()
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">the request message body</param>
        public Subscribe(SubscribeRequestType body)
            : base(body)
        {
        }

        /// <summary>
        /// Creates a new instance of Subscribe
        /// </summary>
        /// <param name="body">the request message body</param>
        /// <param name="responsePort">the response port for the request</param>
        public Subscribe(SubscribeRequestType body, PortSet<SubscribeResponseType, Fault> responsePort)
            : base(body, responsePort)
        {
        }

    }

    /// <summary>
    /// ECEC356_Lab3_q5 Replace Operation
    /// </summary>
    /// <remarks>The Replace class is specific to a service because it uses
    /// the service state.</remarks>
    public class Replace : Replace<ECEC356_Final_ProjectState, PortSet<DefaultReplaceResponseType, Fault>>
    {
    }

}

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17626
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: global::System.Reflection.AssemblyVersionAttribute("0.0.0.0")]
[assembly: global::Microsoft.Dss.Core.Attributes.ServiceDeclarationAttribute(global::Microsoft.Dss.Core.Attributes.DssServiceDeclaration.Transform, SourceAssemblyKey="User.SimulatedDifferentialDrive.2006.M06, Version=0.0.0.0, Culture=neutral, Publi" +
    "cKeyToken=0d3af891cb183898")]
[assembly: global::System.Security.SecurityTransparentAttribute()]
[assembly: global::System.Security.SecurityRulesAttribute(global::System.Security.SecurityRuleSet.Level1)]

namespace Dss.Transforms.TransformUser {
    
    
    public class Transforms : global::Microsoft.Dss.Core.Transforms.TransformBase {
        
        static Transforms() {
            Register();
        }
        
        public static void Register() {
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::ECEC356_Final_Project.Proxy.ECEC356_Final_ProjectState), new global::Microsoft.Dss.Core.Attributes.Transform(ECEC356_Final_Project_Proxy_ECEC356_Final_ProjectState_TO_ECEC356_Final_Project_ECEC356_Final_ProjectState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::ECEC356_Final_Project.ECEC356_Final_ProjectState), new global::Microsoft.Dss.Core.Attributes.Transform(ECEC356_Final_Project_ECEC356_Final_ProjectState_TO_ECEC356_Final_Project_Proxy_ECEC356_Final_ProjectState));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddProxyTransform(typeof(global::ECEC356_Final_Project.Proxy.EventListEntry), new global::Microsoft.Dss.Core.Attributes.Transform(ECEC356_Final_Project_Proxy_EventListEntry_TO_ECEC356_Final_Project_EventListEntry));
            global::Microsoft.Dss.Core.Transforms.TransformBase.AddSourceTransform(typeof(global::ECEC356_Final_Project.EventListEntry), new global::Microsoft.Dss.Core.Attributes.Transform(ECEC356_Final_Project_EventListEntry_TO_ECEC356_Final_Project_Proxy_EventListEntry));
        }
        
        public static object ECEC356_Final_Project_Proxy_ECEC356_Final_ProjectState_TO_ECEC356_Final_Project_ECEC356_Final_ProjectState(object transformFrom) {
            global::ECEC356_Final_Project.ECEC356_Final_ProjectState target = new global::ECEC356_Final_Project.ECEC356_Final_ProjectState();
            global::ECEC356_Final_Project.Proxy.ECEC356_Final_ProjectState from = ((global::ECEC356_Final_Project.Proxy.ECEC356_Final_ProjectState)(transformFrom));
            if ((from.SpeechRecognizerState != null)) {
                global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognizerState tmp = new global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognizerState();
                ((Microsoft.Dss.Core.IDssSerializable)(from.SpeechRecognizerState)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp)));
                target.SpeechRecognizerState = tmp;
            }
            else {
                target.SpeechRecognizerState = null;
            }
            if ((from.SpeechEvents != null)) {
                int count = from.SpeechEvents.Count;
                global::System.Collections.Generic.List<global::ECEC356_Final_Project.EventListEntry> tmp0 = new global::System.Collections.Generic.List<global::ECEC356_Final_Project.EventListEntry>(count);
                for (int index = 0; (index < count); index = (index + 1)) {
                    global::ECEC356_Final_Project.EventListEntry tmp1 = default(global::ECEC356_Final_Project.EventListEntry);
                    if ((from.SpeechEvents[index] != null)) {
                        tmp1 = ((global::ECEC356_Final_Project.EventListEntry)(ECEC356_Final_Project_Proxy_EventListEntry_TO_ECEC356_Final_Project_EventListEntry(from.SpeechEvents[index])));
                    }
                    else {
                        tmp1 = null;
                    }
                    tmp0.Add(tmp1);
                }
                target.SpeechEvents = tmp0;
            }
            else {
                target.SpeechEvents = null;
            }
            target.Angle = from.Angle;
            return target;
        }
        
        public static object ECEC356_Final_Project_ECEC356_Final_ProjectState_TO_ECEC356_Final_Project_Proxy_ECEC356_Final_ProjectState(object transformFrom) {
            global::ECEC356_Final_Project.Proxy.ECEC356_Final_ProjectState target = new global::ECEC356_Final_Project.Proxy.ECEC356_Final_ProjectState();
            global::ECEC356_Final_Project.ECEC356_Final_ProjectState from = ((global::ECEC356_Final_Project.ECEC356_Final_ProjectState)(transformFrom));
            global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognizerState tmp = from.SpeechRecognizerState;
            if ((tmp != null)) {
                global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognizerState tmp0 = new global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognizerState();
                ((Microsoft.Dss.Core.IDssSerializable)(tmp)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp0)));
                target.SpeechRecognizerState = tmp0;
            }
            global::System.Collections.Generic.List<global::ECEC356_Final_Project.EventListEntry> tmp1 = from.SpeechEvents;
            if ((tmp1 != null)) {
                int count = tmp1.Count;
                global::System.Collections.Generic.List<global::ECEC356_Final_Project.Proxy.EventListEntry> tmp2 = new global::System.Collections.Generic.List<global::ECEC356_Final_Project.Proxy.EventListEntry>(count);
                for (int index = 0; (index < count); index = (index + 1)) {
                    global::ECEC356_Final_Project.Proxy.EventListEntry tmp3 = default(global::ECEC356_Final_Project.Proxy.EventListEntry);
                    global::ECEC356_Final_Project.EventListEntry tmp4 = tmp1[index];
                    if ((tmp4 != null)) {
                        tmp3 = ((global::ECEC356_Final_Project.Proxy.EventListEntry)(ECEC356_Final_Project_EventListEntry_TO_ECEC356_Final_Project_Proxy_EventListEntry(tmp4)));
                    }
                    tmp2.Add(tmp3);
                }
                target.SpeechEvents = tmp2;
            }
            target.Angle = from.Angle;
            return target;
        }
        
        public static object ECEC356_Final_Project_Proxy_EventListEntry_TO_ECEC356_Final_Project_EventListEntry(object transformFrom) {
            global::ECEC356_Final_Project.EventListEntry target = new global::ECEC356_Final_Project.EventListEntry();
            global::ECEC356_Final_Project.Proxy.EventListEntry from = ((global::ECEC356_Final_Project.Proxy.EventListEntry)(transformFrom));
            target.Timestamp = from.Timestamp;
            if ((from.SpeechDetected != null)) {
                global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechDetectedNotification tmp = new global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechDetectedNotification();
                ((Microsoft.Dss.Core.IDssSerializable)(from.SpeechDetected)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp)));
                target.SpeechDetected = tmp;
            }
            else {
                target.SpeechDetected = null;
            }
            if ((from.SpeechRecognized != null)) {
                global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognizedNotification tmp0 = new global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognizedNotification();
                ((Microsoft.Dss.Core.IDssSerializable)(from.SpeechRecognized)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp0)));
                target.SpeechRecognized = tmp0;
            }
            else {
                target.SpeechRecognized = null;
            }
            if ((from.RecognitionRejected != null)) {
                global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognitionRejectedNotification tmp1 = new global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognitionRejectedNotification();
                ((Microsoft.Dss.Core.IDssSerializable)(from.RecognitionRejected)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp1)));
                target.RecognitionRejected = tmp1;
            }
            else {
                target.RecognitionRejected = null;
            }
            if ((from.BeamDirectionChanged != null)) {
                global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.BeamDirectionChangedNotification tmp2 = new global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.BeamDirectionChangedNotification();
                ((Microsoft.Dss.Core.IDssSerializable)(from.BeamDirectionChanged)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp2)));
                target.BeamDirectionChanged = tmp2;
            }
            else {
                target.BeamDirectionChanged = null;
            }
            if ((from.SpeechInformation != null)) {
                global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechInformation tmp3 = new global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechInformation();
                ((Microsoft.Dss.Core.IDssSerializable)(from.SpeechInformation)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp3)));
                target.SpeechInformation = tmp3;
            }
            else {
                target.SpeechInformation = null;
            }
            return target;
        }
        
        public static object ECEC356_Final_Project_EventListEntry_TO_ECEC356_Final_Project_Proxy_EventListEntry(object transformFrom) {
            global::ECEC356_Final_Project.Proxy.EventListEntry target = new global::ECEC356_Final_Project.Proxy.EventListEntry();
            global::ECEC356_Final_Project.EventListEntry from = ((global::ECEC356_Final_Project.EventListEntry)(transformFrom));
            target.Timestamp = from.Timestamp;
            global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechDetectedNotification tmp = from.SpeechDetected;
            if ((tmp != null)) {
                global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechDetectedNotification tmp0 = new global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechDetectedNotification();
                ((Microsoft.Dss.Core.IDssSerializable)(tmp)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp0)));
                target.SpeechDetected = tmp0;
            }
            global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognizedNotification tmp1 = from.SpeechRecognized;
            if ((tmp1 != null)) {
                global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognizedNotification tmp2 = new global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognizedNotification();
                ((Microsoft.Dss.Core.IDssSerializable)(tmp1)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp2)));
                target.SpeechRecognized = tmp2;
            }
            global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognitionRejectedNotification tmp3 = from.RecognitionRejected;
            if ((tmp3 != null)) {
                global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognitionRejectedNotification tmp4 = new global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechRecognitionRejectedNotification();
                ((Microsoft.Dss.Core.IDssSerializable)(tmp3)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp4)));
                target.RecognitionRejected = tmp4;
            }
            global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.BeamDirectionChangedNotification tmp5 = from.BeamDirectionChanged;
            if ((tmp5 != null)) {
                global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.BeamDirectionChangedNotification tmp6 = new global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.BeamDirectionChangedNotification();
                ((Microsoft.Dss.Core.IDssSerializable)(tmp5)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp6)));
                target.BeamDirectionChanged = tmp6;
            }
            global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechInformation tmp7 = from.SpeechInformation;
            if ((tmp7 != null)) {
                global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechInformation tmp8 = new global::Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy.SpeechInformation();
                ((Microsoft.Dss.Core.IDssSerializable)(tmp7)).CopyTo(((Microsoft.Dss.Core.IDssSerializable)(tmp8)));
                target.SpeechInformation = tmp8;
            }
            return target;
        }
    }
}

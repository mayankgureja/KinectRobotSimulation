//-----------------------------------------------------------------------
//  This file is part of Microsoft Robotics Developer Studio Code Samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  $File: ECEC356_Final_Project.cs $ $Revision: 54 $
//-----------------------------------------------------------------------

using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;
using Microsoft.Dss.Core.Attributes;
using Microsoft.Dss.ServiceModel.Dssp;
using Microsoft.Dss.ServiceModel.DsspServiceBase;
using submgr = Microsoft.Dss.Services.SubscriptionManager;
using System;
using System.Collections.Generic;
using System.Xml;
using diffdrive = Microsoft.Robotics.Services.Drive.Proxy;
using W3C.Soap;

using simtypes = Microsoft.Robotics.Simulation;
using simengine = Microsoft.Robotics.Simulation.Engine;
using physics = Microsoft.Robotics.Simulation.Physics;
using System.ComponentModel;
using Microsoft.Dss.Core.DsspHttp;
using MicArray = Microsoft.Robotics.Services.Sensors.Kinect.MicArraySpeechRecognizer.Proxy;
using Microsoft.Dss.Core.Utilities;
using System.Threading;
using ccrwpf = Microsoft.Ccr.Adapters.Wpf;
using System.Windows;

namespace ECEC356_Final_Project
{
    /// <summary>
    /// Provides access to a simulated differential drive service.\n(Uses the Generic Differential Drive contract.
    /// </summary>
    [DisplayName("(User) Simulated Generic Differential Drive")]
    [AlternateContract(diffdrive.Contract.Identifier)]
    [DssCategory(simtypes.PublishedCategories.SimulationService)]
    [Contract(Contract.Identifier)]
    [DssServiceDescription("http://msdn.microsoft.com/library/cc998469.aspx")]
    public class ECEC356_Final_ProjectService : Microsoft.Dss.ServiceModel.DsspServiceBase.DsspServiceBase
    {
        #region Simulation Variables
        simengine.SimulationEnginePort _simEngine;
        simengine.DifferentialDriveEntity _entity;
        simengine.SimulationEnginePort _notificationTarget;
        #endregion

        ccrwpf.WpfServicePort _wpfServicePort;
        MainWindow _userInterface;

        private static Dictionary<string, simengine.DifferentialDriveEntity> entities = new Dictionary<string, simengine.DifferentialDriveEntity>();
        private static Boolean SpeechSubscribeState = false, DictionaryState = false;
        Thread t1, t2, t3, t4, t5;

        [InitialStatePartner(Optional = true)]
        private diffdrive.DriveDifferentialTwoWheelState _state = new diffdrive.DriveDifferentialTwoWheelState();
        ECEC356_Final_ProjectState _stateAudio = new ECEC356_Final_ProjectState();

        [ServicePort("/ECEC356_Final_Project", AllowMultipleInstances = true)]
        private diffdrive.DriveOperations _mainPort = new diffdrive.DriveOperations();

        //Declare and create a Subscription Manager to handle the subscriptions
        [SubscriptionManagerPartner]
        private submgr.SubscriptionManagerPort _subMgrPort = new submgr.SubscriptionManagerPort();

        //Partner with KinectMicArraySpeechRecognizer and refer to it by the name KinectMic
        [Partner("KinectMic", Contract = MicArray.Contract.Identifier, CreationPolicy = PartnerCreationPolicy.UseExistingOrCreate)]
        MicArray.SpeechRecognizerOperations _speechRecoPort = new MicArray.SpeechRecognizerOperations();
        MicArray.SpeechRecognizerOperations _speechRecoNotifyPort = new MicArray.SpeechRecognizerOperations();

        /// <summary>
        /// ECEC356_Final_ProjectService constructor that takes a PortSet to
        /// notify when the service is created
        /// </summary>
        /// <param name="creationPort"></param>
        public ECEC356_Final_ProjectService(DsspServiceCreationPort creationPort) :
            base(creationPort)
        {
            PartnerEnumerationTimeout = TimeSpan.FromSeconds(5);
        }

        /// <summary>
        /// Start initializes service state and listens for drop messages
        /// </summary>
        protected override void Start()
        {
            // Find our simulation entity that represents the "hardware" or real-world service.
            // To hook up with simulation entities we do the following steps
            // 1) have a manifest or some other service create us, specifying a partner named SimulationEntity
            // 2) in the simulation service (us) issue a subscribe to the simulation engine looking for
            //    an instance of that simulation entity. We use the Entity.State.Name for the match so it must be
            //    exactly the same. See SimulationTutorial2 for the creation process
            // 3) Listen for a notification telling us the entity is available
            // 4) cache reference to entity and communicate with it issuing low level commands.

            base.Start();

            _simEngine = simengine.SimulationEngine.GlobalInstancePort;
            _notificationTarget = new simengine.SimulationEnginePort();

            if (_state == null)
                CreateDefaultState();

            // enabled by default
            _state.IsEnabled = true;

            // PartnerType.Service is the entity instance name.
            _simEngine.Subscribe(ServiceInfo.PartnerList, _notificationTarget);

            // dont start listening to DSSP operations, other than drop, until notification of entity
            Activate(new Interleave(
                new TeardownReceiverGroup
                (
                    Arbiter.Receive<simengine.InsertSimulationEntity>(false, _notificationTarget, InsertEntityNotificationHandlerFirstTime),
                    Arbiter.Receive<DsspDefaultDrop>(false, _mainPort, DefaultDropHandler)
                ),
                new ExclusiveReceiverGroup(

                    ),
                new ConcurrentReceiverGroup()
            ));

            if (SpeechSubscribeState == false)
            {
                SpeechSubscribeState = true;

                // Register handlers for notification from speech recognizer
                Activate(new Interleave(
                    //    MainPortInterleave.CombineWith(new Interleave(
                    new TeardownReceiverGroup(

                        ),
                    new ExclusiveReceiverGroup(

                        ),
                    new ConcurrentReceiverGroup(
                        Arbiter.Receive<MicArray.Replace>(true, this._speechRecoNotifyPort, this.SpeechRecognizerReplaceHandler),
                        Arbiter.Receive<MicArray.SpeechDetected>(true, this._speechRecoNotifyPort, this.SpeechDetectedHandler),
                        Arbiter.Receive<MicArray.SpeechRecognized>(true, this._speechRecoNotifyPort, this.SpeechRecognizedHandler),
                        Arbiter.Receive<MicArray.SpeechRecognitionRejected>(true, this._speechRecoNotifyPort, this.SpeechRecognitionRejectedHandler),
                        Arbiter.Receive<MicArray.BeamDirectionChanged>(true, this._speechRecoNotifyPort, this.BeamDirectionChangedHandler)
                        )
                        )
                );

                this._speechRecoPort.Subscribe(this._speechRecoNotifyPort);

                SpawnIterator(Initialize);
            }
        }

        IEnumerator<ITask> Initialize()
        {

            // create WPF adapter
            _wpfServicePort = ccrwpf.WpfAdapter.Create(TaskQueue);

            var runWindow = _wpfServicePort.RunWindow(() => new MainWindow(this));
            yield return (Choice)runWindow;

            var exception = (Exception)runWindow;
            if (exception != null)
            {
                LogError(exception);
                StartFailed();
                yield break;
            }

            // need double cast because WPF adapter doesn't know about derived window types
            _userInterface = (Window)runWindow as MainWindow;

        }

        void CreateDefaultState()
        {
            _state = new diffdrive.DriveDifferentialTwoWheelState();
            _state.LeftWheel = new Microsoft.Robotics.Services.Motor.Proxy.WheeledMotorState();
            _state.RightWheel = new Microsoft.Robotics.Services.Motor.Proxy.WheeledMotorState();
            _state.LeftWheel.MotorState = new Microsoft.Robotics.Services.Motor.Proxy.MotorState();
            _state.RightWheel.MotorState = new Microsoft.Robotics.Services.Motor.Proxy.MotorState();
        }

        void UpdateStateFromSimulation()
        {
            if (_entity != null)
            //if(entities.Count != 0)
            {
                _state.TimeStamp = DateTime.Now;
                _state.LeftWheel.MotorState.CurrentPower = _entity.LeftWheel.Wheel.MotorTorque;
                _state.RightWheel.MotorState.CurrentPower = _entity.RightWheel.Wheel.MotorTorque;
                _state.IsEnabled = _entity.IsEnabled;
            }
        }

        void InsertEntityNotificationHandlerFirstTime(simengine.InsertSimulationEntity ins)
        {
            InsertEntityNotificationHandler(ins);

            //base.Start();

            // Listen on the main port for requests and call the appropriate handler.
            MainPortInterleave.CombineWith(
                new Interleave(
                    new TeardownReceiverGroup(),
                    new ExclusiveReceiverGroup(
                        Arbiter.Receive<simengine.InsertSimulationEntity>(true, _notificationTarget, InsertEntityNotificationHandler),
                        Arbiter.Receive<simengine.DeleteSimulationEntity>(true, _notificationTarget, DeleteEntityNotificationHandler)
                    ),
                    new ConcurrentReceiverGroup()
                )
            );
        }

        void InsertEntityNotificationHandler(simengine.InsertSimulationEntity ins)
        {
            _entity = (simengine.DifferentialDriveEntity)ins.Body;
            _entity.ServiceContract = Contract.Identifier;

            // create default state based on the physics entity
            if (_entity.ChassisShape != null)
                _state.DistanceBetweenWheels = _entity.ChassisShape.BoxState.Dimensions.X;

            _state.LeftWheel.MotorState.PowerScalingFactor = _entity.MotorTorqueScaling;
            _state.RightWheel.MotorState.PowerScalingFactor = _entity.MotorTorqueScaling;

            string RobotColor = "";
            if (ins.Body.State.Name.Equals("Robot1"))
                RobotColor = "Blue";
            if (ins.Body.State.Name.Equals("Robot2"))
                RobotColor = "Green";
            if (ins.Body.State.Name.Equals("Robot3"))
                RobotColor = "Red";
            if (ins.Body.State.Name.Equals("Robot4"))
                RobotColor = "Yellow";
            if (ins.Body.State.Name.Equals("Robot5"))
                RobotColor = "Pink";

            entities.Add(RobotColor, _entity);

            //SpawnIterator(OnStartup);

            MakeDictionary();
        }

        void DeleteEntityNotificationHandler(simengine.DeleteSimulationEntity del)
        {
            _entity = null;
            entities.Clear();
        }

        void MakeDictionary()
        {
            if (DictionaryState == false)
            {
                DictionaryState = true;

                MicArray.SpeechRecognizerState insert = new MicArray.SpeechRecognizerState();
                insert.GrammarType = MicArray.GrammarType.DictionaryStyle;
                insert.DictionaryGrammar = new DssDictionary<string, string>();

                insert.DictionaryGrammar.Add("blue robot circle clockwise", "");
                insert.DictionaryGrammar.Add("green robot circle clockwise", "");
                insert.DictionaryGrammar.Add("red robot circle clockwise", "");
                insert.DictionaryGrammar.Add("yellow robot circle clockwise", "");
                insert.DictionaryGrammar.Add("pink robot circle clockwise", "");

                insert.DictionaryGrammar.Add("blue robot circle anticlockwise", "");
                insert.DictionaryGrammar.Add("green robot circle anticlockwise", "");
                insert.DictionaryGrammar.Add("red robot circle anticlockwise", "");
                insert.DictionaryGrammar.Add("yellow robot circle anticlockwise", "");
                insert.DictionaryGrammar.Add("pink robot circle anticlockwise", "");

                insert.DictionaryGrammar.Add("blue robot square clockwise", "");
                insert.DictionaryGrammar.Add("green robot square clockwise", "");
                insert.DictionaryGrammar.Add("red robot square clockwise", "");
                insert.DictionaryGrammar.Add("yellow robot square clockwise", "");
                insert.DictionaryGrammar.Add("pink robot square clockwise", "");

                insert.DictionaryGrammar.Add("blue robot square anticlockwise", "");
                insert.DictionaryGrammar.Add("green robot square anticlockwise", "");
                insert.DictionaryGrammar.Add("red robot square anticlockwise", "");
                insert.DictionaryGrammar.Add("yellow robot square anticlockwise", "");
                insert.DictionaryGrammar.Add("pink robot square anticlockwise", "");

                insert.DictionaryGrammar.Add("blue robot triangle clockwise", "");
                insert.DictionaryGrammar.Add("green robot triangle clockwise", "");
                insert.DictionaryGrammar.Add("red robot triangle clockwise", "");
                insert.DictionaryGrammar.Add("yellow robot triangle clockwise", "");
                insert.DictionaryGrammar.Add("pink robot triangle clockwise", "");

                insert.DictionaryGrammar.Add("blue robot triangle anticlockwise", "");
                insert.DictionaryGrammar.Add("green robot triangle anticlockwise", "");
                insert.DictionaryGrammar.Add("red robot triangle anticlockwise", "");
                insert.DictionaryGrammar.Add("yellow robot triangle anticlockwise", "");
                insert.DictionaryGrammar.Add("pink robot triangle anticlockwise", "");

                insert.DictionaryGrammar.Add("blue robot stop", "");
                insert.DictionaryGrammar.Add("green robot stop", "");
                insert.DictionaryGrammar.Add("red robot stop", "");
                insert.DictionaryGrammar.Add("yellow robot stop", "");
                insert.DictionaryGrammar.Add("pink robot stop", "");

                insert.DictionaryGrammar.Add("all robots circle", "");
                insert.DictionaryGrammar.Add("all robots square", "");
                insert.DictionaryGrammar.Add("all robots triangle", "");
                insert.DictionaryGrammar.Add("all robots stop", "");

                MicArray.Replace replaceRequest = new MicArray.Replace(insert);
                this._speechRecoPort.Post(replaceRequest);
            }
        }
        #region Movement methods

        /// <summary>
        /// CircularMotion_Clockwise method
        /// </summary>
        /// <returns></returns>
        public void CircularMotion_Clockwise(simengine.DifferentialDriveEntity entity)
        //public IEnumerator<ITask> CircularMotion_Clockwise(simengine.DifferentialDriveEntity entity)
        {
            entity.SetMotorTorque((float)0.50, (float)0.30);
            //yield break;
        }

        /// <summary>
        /// CircularMotion_AntiClockwise method
        /// </summary>
        /// <returns></returns>
        public void CircularMotion_AntiClockwise(simengine.DifferentialDriveEntity entity)
        //public IEnumerator<ITask> CircularMotion_AntiClockwise(simengine.DifferentialDriveEntity entity)
        {
            entity.SetMotorTorque((float)0.30, (float)0.50);
            //yield break;
        }

        /// <summary>
        /// SquareMotion_Clockwise method
        /// </summary>
        /// <returns></returns>
        public void SquareMotion_Clockwise(simengine.DifferentialDriveEntity entity)
        {
            float powerdist = 0.4f;
            float degrees = -90.0f;
            float powerdeg = 0.1f;
            float stop = 0.0f;

            Port<simengine.OperationResult> entityResponse = new Port<simengine.OperationResult>();
            DateTime a;
            DateTime b;

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }
        }

        /// <summary>
        /// SquareMotion_AntiClockwise method
        /// </summary>
        /// <returns></returns>
        public void SquareMotion_AntiClockwise(simengine.DifferentialDriveEntity entity)
        {
            float powerdist = 0.4f;
            float degrees = 90.0f;
            float powerdeg = 0.1f;
            float stop = 0.0f;

            Port<simengine.OperationResult> entityResponse = new Port<simengine.OperationResult>();
            DateTime a;
            DateTime b;

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }
        }

        /// <summary>
        /// TriangleMotion_AntiClockwise method
        /// </summary>
        /// <returns></returns>
        public void TriangleMotion_Clockwise(simengine.DifferentialDriveEntity entity)
        {
            float powerdist = 0.4f;
            float degrees = -120.0f;
            float powerdeg = 0.1f;
            float stop = 0.0f;

            Port<simengine.OperationResult> entityResponse = new Port<simengine.OperationResult>();
            DateTime a;
            DateTime b;

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }
        }

        /// <summary>
        /// TriangleMotion_AntiClockwise method
        /// </summary>
        /// <returns></returns>
        public void TriangleMotion_AntiClockwise(simengine.DifferentialDriveEntity entity)
        {
            float powerdist = 0.4f;
            float degrees = 120.0f;
            float powerdeg = 0.1f;
            float stop = 0.0f;

            Port<simengine.OperationResult> entityResponse = new Port<simengine.OperationResult>();
            DateTime a;
            DateTime b;

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(powerdist, powerdist);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }

            entity.SetMotorTorque(stop, stop);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }

            entity.RotateDegrees(degrees, powerdeg, entityResponse);
            a = DateTime.Now;
            b = DateTime.Now;
            while (b.Subtract(a).TotalSeconds < 4)
            {
                b = DateTime.Now;
            }
        }

        /// <summary>
        /// Stop method
        /// </summary>
        /// <returns></returns>
        public void Stop(simengine.DifferentialDriveEntity entity)
        {
            DateTime a = DateTime.Now;
            DateTime b = DateTime.Now;

            entity.SetMotorTorque(0.0f, 0.0f);
            while (b.Subtract(a).TotalSeconds < 7)
            {
                b = DateTime.Now;
            }
        }


        ///// <summary>
        ///// Circular motion method
        ///// </summary>
        ///// <returns></returns>
        //public IEnumerator<ITask> CircularMotion()
        //{
        //    diffdrive.SetDrivePowerRequest setPower = new diffdrive.SetDrivePowerRequest();
        //    setPower.RightWheelPower = 0.52;
        //    setPower.LeftWheelPower = 0.30;

        //    _mainPort.Post(new diffdrive.SetDrivePower(setPower));

        //    yield break;
        //}

        ///// <summary>
        ///// Square motion method
        ///// </summary>
        ///// <returns></returns>
        //public IEnumerator<ITask> SquareMotion()
        //{
        //    diffdrive.DriveDistanceRequest drive = new diffdrive.DriveDistanceRequest(1, 0.5);
        //    diffdrive.RotateDegreesRequest rotate = new diffdrive.RotateDegreesRequest(90, 0.1);



        //    _state.DriveDistanceStage = diffdrive.DriveStage.InitialRequest;
        //    _mainPort.Post(new diffdrive.DriveDistance(drive));
        //    while (_state.DriveDistanceStage != diffdrive.DriveStage.Completed)
        //        System.Threading.Thread.Sleep(500);

        //    _state.RotateDegreesStage = diffdrive.DriveStage.InitialRequest;
        //    _mainPort.Post(new diffdrive.RotateDegrees(rotate));
        //    while (_state.RotateDegreesStage != diffdrive.DriveStage.Completed)
        //        System.Threading.Thread.Sleep(500);

        //    _state.DriveDistanceStage = diffdrive.DriveStage.InitialRequest;
        //    _mainPort.Post(new diffdrive.DriveDistance(drive));
        //    while (_state.DriveDistanceStage != diffdrive.DriveStage.Completed)
        //        System.Threading.Thread.Sleep(500);

        //    _state.RotateDegreesStage = diffdrive.DriveStage.InitialRequest;
        //    _mainPort.Post(new diffdrive.RotateDegrees(rotate));
        //    while (_state.RotateDegreesStage != diffdrive.DriveStage.Completed)
        //        System.Threading.Thread.Sleep(500); System.Threading.Thread.Sleep(500);

        //    _state.DriveDistanceStage = diffdrive.DriveStage.InitialRequest;
        //    _mainPort.Post(new diffdrive.DriveDistance(drive));
        //    while (_state.DriveDistanceStage != diffdrive.DriveStage.Completed)
        //        System.Threading.Thread.Sleep(500);

        //    _state.RotateDegreesStage = diffdrive.DriveStage.InitialRequest;
        //    _mainPort.Post(new diffdrive.RotateDegrees(rotate));
        //    while (_state.RotateDegreesStage != diffdrive.DriveStage.Completed)
        //        System.Threading.Thread.Sleep(500);

        //    _state.DriveDistanceStage = diffdrive.DriveStage.InitialRequest;
        //    _mainPort.Post(new diffdrive.DriveDistance(drive));
        //    while (_state.DriveDistanceStage != diffdrive.DriveStage.Completed)
        //        System.Threading.Thread.Sleep(500);

        //    _state.RotateDegreesStage = diffdrive.DriveStage.InitialRequest;
        //    _mainPort.Post(new diffdrive.RotateDegrees(rotate));
        //    while (_state.RotateDegreesStage != diffdrive.DriveStage.Completed)
        //        System.Threading.Thread.Sleep(500);

        //    SquareMotion();

        //    yield break;

        //}

        #endregion

        /// <summary>
        /// Get handler retrieves service state
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(HttpGet get)
        {
            UpdateStateFromSimulation();
            get.ResponsePort.Post(new HttpResponseType(_state));
            yield break;
        }

        /// <summary>
        /// Get handler retrieves service state
        /// </summary>
        /// <param name="get"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> GetHandler(diffdrive.Get get)
        {
            UpdateStateFromSimulation();
            get.ResponsePort.Post(_state);
            yield break;
        }

        #region Subscribe Handling

        /// <summary>
        /// Subscribe to Differential Drive service
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> SubscribeHandler(diffdrive.Subscribe subscribe)
        {
            Activate(Arbiter.Choice(
                SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    _subMgrPort.Post(new submgr.Submit(
                        subscribe.Body.Subscriber, DsspActions.UpdateRequest, _state, null));
                },
                delegate(Exception ex) { LogError(ex); }
            ));

            yield break;
        }

        /// <summary>
        /// Subscribe to Differential Drive service
        /// </summary>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> ReliableSubscribeHandler(diffdrive.ReliableSubscribe subscribe)
        {
            Activate(Arbiter.Choice(
                SubscribeHelper(_subMgrPort, subscribe.Body, subscribe.ResponsePort),
                delegate(SuccessResult success)
                {
                    _subMgrPort.Post(new submgr.Submit(
                        subscribe.Body.Subscriber, DsspActions.UpdateRequest, _state, null));
                },
                delegate(Exception ex) { LogError(ex); }
            ));
            yield break;
        }
        #endregion

        #region DifferentialDrive Handlers

        /// <summary>
        /// Handler for drive request
        /// </summary>
        /// <param name="driveDistance"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> DriveDistanceHandler(diffdrive.DriveDistance driveDistance)
        {
            if (_entity == null)
                throw new InvalidOperationException("Simulation entity not registered with service");

            if (!_state.IsEnabled)
            {
                driveDistance.ResponsePort.Post(Fault.FromException(new Exception("Drive is not enabled.")));
                LogError("DriveDistance request to disabled drive.");
                yield break;
            }

            if ((driveDistance.Body.Power > 1.0f) || (driveDistance.Body.Power < -1.0f))
            {
                // invalid drive power
                driveDistance.ResponsePort.Post(Fault.FromException(new Exception("Invalid Power parameter.")));
                LogError("Invalid Power parameter in DriveDistanceHandler.");
                yield break;
            }

            _state.DriveDistanceStage = driveDistance.Body.DriveDistanceStage;
            if (driveDistance.Body.DriveDistanceStage == diffdrive.DriveStage.InitialRequest)
            {
                Port<simengine.OperationResult> entityResponse = new Port<simengine.OperationResult>();
                Activate(Arbiter.Receive<simengine.OperationResult>(false, entityResponse, delegate(simengine.OperationResult result)
                {
                    // post a message to ourselves indicating that the drive distance has completed
                    diffdrive.DriveDistanceRequest req = new diffdrive.DriveDistanceRequest(0, 0);
                    switch (result)
                    {
                        case simengine.OperationResult.Error:
                            req.DriveDistanceStage = diffdrive.DriveStage.Canceled;
                            break;
                        case simengine.OperationResult.Canceled:
                            req.DriveDistanceStage = diffdrive.DriveStage.Canceled;
                            break;
                        case simengine.OperationResult.Completed:
                            req.DriveDistanceStage = diffdrive.DriveStage.Completed;
                            break;
                    }
                    _mainPort.Post(new diffdrive.DriveDistance(req));
                }));

                _entity.DriveDistance((float)driveDistance.Body.Distance, (float)driveDistance.Body.Power, entityResponse);

                diffdrive.DriveDistanceRequest req2 = new diffdrive.DriveDistanceRequest(0, 0);
                req2.DriveDistanceStage = diffdrive.DriveStage.Started;
                _mainPort.Post(new diffdrive.DriveDistance(req2));
            }
            else
            {
                base.SendNotification(_subMgrPort, driveDistance);
            }
            driveDistance.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Handler for rotate request
        /// </summary>
        /// <param name="rotate"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> RotateHandler(diffdrive.RotateDegrees rotate)
        {
            if (_entity == null)
                throw new InvalidOperationException("Simulation entity not registered with service");

            if (!_state.IsEnabled)
            {
                rotate.ResponsePort.Post(Fault.FromException(new Exception("Drive is not enabled.")));
                LogError("RotateDegrees request to disabled drive.");
                yield break;
            }

            _state.RotateDegreesStage = rotate.Body.RotateDegreesStage;
            if (rotate.Body.RotateDegreesStage == diffdrive.DriveStage.InitialRequest)
            {
                Port<simengine.OperationResult> entityResponse = new Port<simengine.OperationResult>();
                Activate(Arbiter.Receive<simengine.OperationResult>(false, entityResponse, delegate(simengine.OperationResult result)
                {
                    // post a message to ourselves indicating that the drive distance has completed
                    diffdrive.RotateDegreesRequest req = new diffdrive.RotateDegreesRequest(0, 0);
                    switch (result)
                    {
                        case simengine.OperationResult.Error:
                            req.RotateDegreesStage = diffdrive.DriveStage.Canceled;
                            break;
                        case simengine.OperationResult.Canceled:
                            req.RotateDegreesStage = diffdrive.DriveStage.Canceled;
                            break;
                        case simengine.OperationResult.Completed:
                            req.RotateDegreesStage = diffdrive.DriveStage.Completed;
                            break;
                    }
                    _mainPort.Post(new diffdrive.RotateDegrees(req));
                }));

                _entity.RotateDegrees((float)rotate.Body.Degrees, (float)rotate.Body.Power, entityResponse);

                diffdrive.RotateDegreesRequest req2 = new diffdrive.RotateDegreesRequest(0, 0);
                req2.RotateDegreesStage = diffdrive.DriveStage.Started;
                _mainPort.Post(new diffdrive.RotateDegrees(req2));
            }
            else
            {
                base.SendNotification(_subMgrPort, rotate);
            }
            rotate.ResponsePort.Post(DefaultUpdateResponseType.Instance);
            yield break;
        }

        /// <summary>
        /// Handler for setting the drive power
        /// </summary>
        /// <param name="setPower"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SetPowerHandler(diffdrive.SetDrivePower setPower)
        {
            if (_entity == null)
                throw new InvalidOperationException("Simulation entity not registered with service");

            if (!_state.IsEnabled)
            {
                setPower.ResponsePort.Post(Fault.FromException(new Exception("Drive is not enabled.")));
                LogError("SetPower request to disabled drive.");
                yield break;
            }

            if ((setPower.Body.LeftWheelPower > 1.0f) || (setPower.Body.LeftWheelPower < -1.0f) ||
                (setPower.Body.RightWheelPower > 1.0f) || (setPower.Body.RightWheelPower < -1.0f))
            {
                // invalid drive power
                setPower.ResponsePort.Post(Fault.FromException(new Exception("Invalid Power parameter.")));
                LogError("Invalid Power parameter in SetPowerHandler.");
                yield break;
            }


            // Call simulation entity method for setting wheel torque
            _entity.SetMotorTorque(
                (float)(setPower.Body.LeftWheelPower),
                (float)(setPower.Body.RightWheelPower));

            UpdateStateFromSimulation();
            setPower.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send update notification for entire state
            _subMgrPort.Post(new submgr.Submit(_state, DsspActions.UpdateRequest));
            yield break;
        }

        /// <summary>
        /// Handler for setting the drive speed
        /// </summary>
        /// <param name="setSpeed"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> SetSpeedHandler(diffdrive.SetDriveSpeed setSpeed)
        {
            if (_entity == null)
                throw new InvalidOperationException("Simulation entity not registered with service");

            if (!_state.IsEnabled)
            {
                setSpeed.ResponsePort.Post(Fault.FromException(new Exception("Drive is not enabled.")));
                LogError("SetSpeed request to disabled drive.");
                yield break;
            }

            _entity.SetVelocity(
                (float)setSpeed.Body.LeftWheelSpeed,
                (float)setSpeed.Body.RightWheelSpeed);

            UpdateStateFromSimulation();
            setSpeed.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send update notification for entire state
            _subMgrPort.Post(new submgr.Submit(_state, DsspActions.UpdateRequest));
            yield break;
        }

        /// <summary>
        /// Handler for enabling or disabling the drive
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Concurrent)]
        public IEnumerator<ITask> EnableHandler(diffdrive.EnableDrive enable)
        {
            if (_entity == null)
                throw new InvalidOperationException("Simulation entity not registered with service");

            _state.IsEnabled = enable.Body.Enable;
            _entity.IsEnabled = _state.IsEnabled;

            UpdateStateFromSimulation();
            enable.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send update for entire state
            _subMgrPort.Post(new submgr.Submit(_state, DsspActions.UpdateRequest));
            yield break;
        }

        /// <summary>
        /// Handler when the drive receives an all stop message
        /// </summary>
        /// <param name="estop"></param>
        /// <returns></returns>
        [ServiceHandler(ServiceHandlerBehavior.Exclusive)]
        public IEnumerator<ITask> AllStopHandler(diffdrive.AllStop estop)
        {
            if (_entity == null)
                throw new InvalidOperationException("Simulation entity not registered with service");

            _entity.SetMotorTorque(0, 0);
            _entity.SetVelocity(0);

            // AllStop disables the drive
            _entity.IsEnabled = false;

            UpdateStateFromSimulation();
            estop.ResponsePort.Post(DefaultUpdateResponseType.Instance);

            // send update for entire state
            _subMgrPort.Post(new submgr.Submit(_state, DsspActions.UpdateRequest));
            yield break;
        }

        #endregion

        #region speech handlers

        /// <summary>
        /// Speech recognizer replace handler
        /// </summary>
        /// <param name="replace"></param>
        private void SpeechRecognizerReplaceHandler(MicArray.Replace replace)
        {
            this._stateAudio.SpeechRecognizerState = replace.Body;
        }

        /// <summary>
        /// Maximum number of past speech events received from the speech recognizer that shall
        /// be retained
        /// </summary>
        private const int MaxSpeechEventsToRetain = 20;

        /// <summary>
        /// Speech detected handler
        /// </summary>
        /// <param name="detected"></param>
        private void SpeechDetectedHandler(MicArray.SpeechDetected detected)
        {
            // Keep speech event queue from growing infinitely
            if (this._stateAudio.SpeechEvents.Count == MaxSpeechEventsToRetain)
            {
                this._stateAudio.SpeechEvents.RemoveAt(0);
            }

            // Add latest event to queue
            this._stateAudio.SpeechEvents.Add(new EventListEntry(detected.Body));
            LogInfo("Speech detected");
        }

        /// <summary>
        /// Changes the status messages on the XAML window
        /// </summary>
        private void StatusText(string color, string status)
        {
            switch (color)
            {
                case "blue":
                    _userInterface.Blue = status;
                    break;
                case "green":
                    _userInterface.Green = status;
                    break;
                case "red":
                    _userInterface.Red = status;
                    break;
                case "yellow":
                    _userInterface.Yellow = status;
                    break;
                case "pink":
                    _userInterface.Pink = status;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Speech recognized handler
        /// </summary>
        /// <param name="recognized"></param>
        private void SpeechRecognizedHandler(MicArray.SpeechRecognized recognized)
        {
            // Keep speech event queue from growing infinitely
            if (this._stateAudio.SpeechEvents.Count == MaxSpeechEventsToRetain)
            {
                this._stateAudio.SpeechEvents.RemoveAt(0);
            }

            // Add latest event to queue
            this._stateAudio.SpeechEvents.Add(new EventListEntry(recognized.Body));

            LogInfo("Recognized word: " + recognized.Body.Text);

            _stateAudio.Angle = recognized.Body.Angle;

            string command = recognized.Body.Text;

            //string[] commandsplit = recognized.Body.Text.Split();
            string color = recognized.Body.Text.Split()[0];
            string[] colors = {"blue", "green", "red", "yellow", "pink"};

            switch (color)
            {
                case "blue":
                    TryAbort(t1);
                    break;
                case "green":
                    TryAbort(t2);
                    break;
                case "red":
                    TryAbort(t3);
                    break;
                case "yellow":
                    TryAbort(t4);
                    break;
                case "pink":
                    TryAbort(t5);
                    break;
                default:
                    break;
            }

            #region Circle Clockwise
            if (command.Equals("blue robot circle clockwise"))
            {
                t1 = new Thread(() => CircularMotion_Clockwise(entities["Blue"]));
                t1.Start(); 
                StatusText(color, "Circle Clockwise");
            }
            if (command.Equals("green robot circle clockwise"))
            {
                t2 = new Thread(() => CircularMotion_Clockwise(entities["Green"]));
                t2.Start();
                StatusText(color, "Circle Clockwise");
            }
            if (command.Equals("red robot circle clockwise"))
            {
                t3 = new Thread(() => CircularMotion_Clockwise(entities["Red"]));
                t3.Start();
                StatusText(color, "Circle Clockwise");
            }
            if (command.Equals("yellow robot circle clockwise"))
            {
                t4 = new Thread(() => CircularMotion_Clockwise(entities["Yellow"]));
                t4.Start();
                StatusText(color, "Circle Clockwise");
            }
            if (command.Equals("pink robot circle clockwise"))
            {
                t5 = new Thread(() => CircularMotion_Clockwise(entities["Pink"]));
                t5.Start();
                StatusText(color, "Circle Clockwise");
            }
            #endregion


            #region Circle Anticlockwise
            if (command.Equals("blue robot circle anticlockwise"))
            {
                t1 = new Thread(() => CircularMotion_AntiClockwise(entities["Blue"]));
                t1.Start();
                StatusText(color, "Circle Anti-Clockwise");
            }
            if (command.Equals("green robot circle anticlockwise"))
            {
                t2 = new Thread(() => CircularMotion_AntiClockwise(entities["Green"]));
                t2.Start();
                StatusText(color, "Circle Anti-Clockwise");
            }
            if (command.Equals("red robot circle anticlockwise"))
            {
                t3 = new Thread(() => CircularMotion_AntiClockwise(entities["Red"]));
                t3.Start();
                StatusText(color, "Circle Anti-Clockwise");
            }
            if (command.Equals("yellow robot circle anticlockwise"))
            {
                t4 = new Thread(() => CircularMotion_AntiClockwise(entities["Yellow"]));
                t4.Start();
                StatusText(color, "Circle Anti-Clockwise");
            }
            if (command.Equals("pink robot circle anticlockwise"))
            {
                t5 = new Thread(() => CircularMotion_AntiClockwise(entities["Pink"]));
                t5.Start();
                StatusText(color, "Circle Anti-Clockwise");
            }
            #endregion


            #region Square Clockwise
            if (command.Equals("blue robot square clockwise"))
            {
                t1 = new Thread(() => SquareMotion_Clockwise(entities["Blue"]));
                t1.Start();
                StatusText(color, "Square Clockwise");
            }
            if (command.Equals("green robot square clockwise"))
            {
                t2 = new Thread(() => SquareMotion_Clockwise(entities["Green"]));
                t2.Start();
                StatusText(color, "Square Clockwise");
            }
            if (command.Equals("red robot square clockwise"))
            {
                t3 = new Thread(() => SquareMotion_Clockwise(entities["Red"]));
                t3.Start();
                StatusText(color, "Square Clockwise");
            }
            if (command.Equals("yellow robot square clockwise"))
            {
                t4 = new Thread(() => SquareMotion_Clockwise(entities["Yellow"]));
                t4.Start();
                StatusText(color, "Square Clockwise");
            }
            if (command.Equals("pink robot square clockwise"))
            {
                t5 = new Thread(() => SquareMotion_Clockwise(entities["Pink"]));
                t5.Start();
                StatusText(color, "Square Clockwise");
            }
            #endregion


            #region Square Anticlockwise
            if (command.Equals("blue robot square anticlockwise"))
            {
                t1 = new Thread(() => SquareMotion_AntiClockwise(entities["Blue"]));
                t1.Start();
                StatusText(color, "Square Anti-Clockwise");
            }
            if (command.Equals("green robot square anticlockwise"))
            {
                t2 = new Thread(() => SquareMotion_AntiClockwise(entities["Green"]));
                t2.Start();
                StatusText(color, "Square Anti-Clockwise");
            }
            if (command.Equals("red robot square anticlockwise"))
            {
                t3 = new Thread(() => SquareMotion_AntiClockwise(entities["Red"]));
                t3.Start();
                StatusText(color, "Square Anti-Clockwise");
            }
            if (command.Equals("yellow robot square anticlockwise"))
            {
                t4 = new Thread(() => SquareMotion_AntiClockwise(entities["Yellow"]));
                t4.Start();
                StatusText(color, "Square Anti-Clockwise");
            }
            if (command.Equals("pink robot square anticlockwise"))
            {
                t5 = new Thread(() => SquareMotion_AntiClockwise(entities["Pink"]));
                t5.Start();
                StatusText(color, "Square Anti-Clockwise");
            }
            #endregion


            #region Triangle Clockwise
            if (command.Equals("blue robot triangle clockwise"))
            {
                t1 = new Thread(() => TriangleMotion_Clockwise(entities["Blue"]));
                t1.Start();
                StatusText(color, "Triangle Clockwise");
            }
            if (command.Equals("green robot triangle clockwise"))
            {
                t2 = new Thread(() => TriangleMotion_Clockwise(entities["Green"]));
                t2.Start();
                StatusText(color, "Triangle Clockwise");
            }
            if (command.Equals("red robot triangle clockwise"))
            {
                t3 = new Thread(() => TriangleMotion_Clockwise(entities["Red"]));
                t3.Start();
                StatusText(color, "Triangle Clockwise");
            }
            if (command.Equals("yellow robot triangle clockwise"))
            {
                t4 = new Thread(() => TriangleMotion_Clockwise(entities["Yellow"]));
                t4.Start();
                StatusText(color, "Triangle Clockwise");
            }
            if (command.Equals("pink robot triangle clockwise"))
            {
                t5 = new Thread(() => TriangleMotion_Clockwise(entities["Pink"]));
                t5.Start();
                StatusText(color, "Triangle Clockwise");
            }
            #endregion


            #region Triangle Anticlockwise
            if (command.Equals("blue robot triangle anticlockwise"))
            {
                t1 = new Thread(() => TriangleMotion_AntiClockwise(entities["Blue"]));
                t1.Start();
                StatusText(color, "Triangle Anti-Clockwise");
            }
            if (command.Equals("green robot triangle anticlockwise"))
            {
                t2 = new Thread(() => TriangleMotion_AntiClockwise(entities["Green"]));
                t2.Start();
                StatusText(color, "Triangle Anti-Clockwise");
            }
            if (command.Equals("red robot triangle anticlockwise"))
            {
                t3 = new Thread(() => TriangleMotion_AntiClockwise(entities["Red"]));
                t3.Start();
                StatusText(color, "Triangle Anti-Clockwise");
            }
            if (command.Equals("yellow robot triangle anticlockwise"))
            {
                t4 = new Thread(() => TriangleMotion_AntiClockwise(entities["Yellow"]));
                t4.Start();
                StatusText(color, "Triangle Anti-Clockwise");
            }
            if (command.Equals("pink robot triangle anticlockwise"))
            {
                t5 = new Thread(() => TriangleMotion_AntiClockwise(entities["Pink"]));
                t5.Start();
                StatusText(color, "Triangle Anti-Clockwise");
            }
            #endregion


            #region Stop
            if (command.Equals("blue robot stop"))
            {
                t1 = new Thread(() => Stop(entities["Blue"]));
                t1.Start();
                StatusText(color, "Stopped");
            }
            if (command.Equals("green robot stop"))
            {
                t2 = new Thread(() => Stop(entities["Green"]));
                t2.Start();
                StatusText(color, "Stopped");
            }
            if (command.Equals("red robot stop"))
            {
                t3 = new Thread(() => Stop(entities["Red"]));
                t3.Start();
                StatusText(color, "Stopped");
            }
            if (command.Equals("yellow robot stop"))
            {
                t4 = new Thread(() => Stop(entities["Yellow"]));
                t4.Start();
                StatusText(color, "Stopped");
            }
            if (command.Equals("pink robot stop"))
            {
                t5 = new Thread(() => Stop(entities["Pink"]));
                t5.Start();
                StatusText(color, "Stopped");
            }
            #endregion


            #region All Robots Circle
            if (command.Equals("all robots circle"))
            {
                t1 = new Thread(() => CircularMotion_Clockwise(entities["Blue"]));
                t1.Start();

                t2 = new Thread(() => CircularMotion_Clockwise(entities["Green"]));
                t2.Start();

                t3 = new Thread(() => CircularMotion_Clockwise(entities["Red"]));
                t3.Start();

                t4 = new Thread(() => CircularMotion_Clockwise(entities["Yellow"]));
                t4.Start();

                t5 = new Thread(() => CircularMotion_Clockwise(entities["Pink"]));
                t5.Start();

                foreach(string col in colors)
                {
                    StatusText(col, "Circle Clockwise");
                }
            }
            #endregion


            #region All Robots Square
            if (command.Equals("all robots square"))
            {
                t1 = new Thread(() => SquareMotion_Clockwise(entities["Blue"]));
                t1.Start();

                t2 = new Thread(() => SquareMotion_Clockwise(entities["Green"]));
                t2.Start();

                t3 = new Thread(() => SquareMotion_Clockwise(entities["Red"]));
                t3.Start();

                t4 = new Thread(() => SquareMotion_Clockwise(entities["Yellow"]));
                t4.Start();

                t5 = new Thread(() => SquareMotion_Clockwise(entities["Pink"]));
                t5.Start();

                foreach (string col in colors)
                {
                    StatusText(col, "Square Clockwise");
                }
            }
            #endregion


            #region All Robots Triangle
            if (command.Equals("all robots triangle"))
            {
                t1 = new Thread(() => TriangleMotion_Clockwise(entities["Blue"]));
                t1.Start();

                t2 = new Thread(() => TriangleMotion_Clockwise(entities["Green"]));
                t2.Start();

                t3 = new Thread(() => TriangleMotion_Clockwise(entities["Red"]));
                t3.Start();

                t4 = new Thread(() => TriangleMotion_Clockwise(entities["Yellow"]));
                t4.Start();

                t5 = new Thread(() => TriangleMotion_Clockwise(entities["Pink"]));
                t5.Start();

                foreach (string col in colors)
                {
                    StatusText(col, "Triangle Clockwise");
                }
            }
            #endregion

            #region All Robots Stop
            if (command.Equals("all robots stop"))
            {
                t1 = new Thread(() => Stop(entities["Blue"]));
                t1.Start();

                t2 = new Thread(() => Stop(entities["Green"]));
                t2.Start();

                t3 = new Thread(() => Stop(entities["Red"]));
                t3.Start();

                t4 = new Thread(() => Stop(entities["Yellow"]));
                t4.Start();

                t5 = new Thread(() => Stop(entities["Pink"]));
                t5.Start();

                foreach (string col in colors)
                {
                    StatusText(col, "Stopped");
                }
            }
            #endregion

        }

        /// <summary>
        /// Tries to abort a thread if it is active
        /// </summary>
        /// <param name="t">Thread</param>
        private void TryAbort(Thread t)
        {
            if (t != null)
                t.Abort();
        }

        /// <summary>
        /// Speech recognition rejected handler
        /// </summary>
        /// <param name="rejected"></param>
        private void SpeechRecognitionRejectedHandler(MicArray.SpeechRecognitionRejected rejected)
        {
            // Keep speech event queue from growing infinitely
            if (this._stateAudio.SpeechEvents.Count == MaxSpeechEventsToRetain)
            {
                this._stateAudio.SpeechEvents.RemoveAt(0);
            }

            // Add latest event to queue
            this._stateAudio.SpeechEvents.Add(new EventListEntry(rejected.Body));
        }

        /// <summary>
        /// Mic array beam direction changed event handler
        /// </summary>
        /// <param name="beamDirectionChanged"></param>
        private void BeamDirectionChangedHandler(MicArray.BeamDirectionChanged beamDirectionChanged)
        {
            // Keep speech event queue from growing infinitely
            if (this._stateAudio.SpeechEvents.Count == MaxSpeechEventsToRetain)
            {
                this._stateAudio.SpeechEvents.RemoveAt(0);
            }

            // Add latest event to queue
            this._stateAudio.SpeechEvents.Add(new EventListEntry(beamDirectionChanged.Body));
            LogInfo("Beam Direction Changed (radians): " + beamDirectionChanged.Body.Angle);
        }
        #endregion
    }
}

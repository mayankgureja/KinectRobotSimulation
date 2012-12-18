//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace ECEC356_Final_Project
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string BlueStatus = "Stopped";
        private string GreenStatus = "Stopped";
        private string RedStatus = "Stopped";
        private string YellowStatus = "Stopped";
        private string PinkStatus = "Stopped";

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event implementing INotifyPropertyChanged interface.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets/sets Blue Robot Status message.
        /// </summary>
        public string Blue
        {
            get
            {
                return this.BlueStatus;
            }

            set
            {
                if (this.BlueStatus != value)
                {
                    this.BlueStatus = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Blue"));
                    }
                }
            }
        }

        /// <summary>
        /// Gets/sets Green Robot Status message.
        /// </summary>
        public string Green
        {
            get
            {
                return this.GreenStatus;
            }

            set
            {
                if (this.GreenStatus != value)
                {
                    this.GreenStatus = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Green"));
                    }
                }
            }
        }

        /// <summary>
        /// Gets/sets Red Robot Status message.
        /// </summary>
        public string Red
        {
            get
            {
                return this.RedStatus;
            }

            set
            {
                if (this.RedStatus != value)
                {
                    this.RedStatus = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Red"));
                    }
                }
            }
        }

        /// <summary>
        /// Gets/sets Yellow Robot Status message.
        /// </summary>
        public string Yellow
        {
            get
            {
                return this.YellowStatus;
            }

            set
            {
                if (this.YellowStatus != value)
                {
                    this.YellowStatus = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Yellow"));
                    }
                }
            }
        }

        /// <summary>
        /// Gets/sets Pink Robot Status message.
        /// </summary>
        public string Pink
        {
            get
            {
                return this.PinkStatus;
            }

            set
            {
                if (this.PinkStatus != value)
                {
                    this.PinkStatus = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("Pink"));
                    }
                }
            }
        }

        /// <summary>
        /// MainWindow() constructor
        /// </summary>
        ECEC356_Final_ProjectService _service;
        internal MainWindow(ECEC356_Final_ProjectService service)
            : this()
        {
            _service = service;
        }


    }
}

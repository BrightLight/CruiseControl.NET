﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetLog.cs" company="The CruiseControl.NET Team">
//   Copyright (C) 2011 by The CruiseControl.NET Team
// 
//   Permission is hereby granted, free of charge, to any person obtaining a copy
//   of this software and associated documentation files (the "Software"), to deal
//   in the Software without restriction, including without limitation the rights
//   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//   copies of the Software, and to permit persons to whom the Software is
//   furnished to do so, subject to the following conditions:
//   
//   The above copyright notice and this permission notice shall be included in
//   all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//   THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ThoughtWorks.CruiseControl.PowerShell.Cmdlets
{
    using System;
    using System.Linq;
    using System.Management.Automation;

    /// <summary>
    /// Retrieves a log.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, Nouns.Log, DefaultParameterSetName = CommonCmdlet.CommonParameterSet)]
    public class GetLog
        : ConnectionCmdlet
    {
        #region Public properties
        #region Project
        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        [Parameter(Position = 0, ValueFromPipeline = true, ParameterSetName = ProjectCmdlet.ProjectParameterSet)]
        public CCProject Project { get; set; }
        #endregion

        #region Build
        /// <summary>
        /// Gets or sets the build.
        /// </summary>
        /// <value>
        /// The build.
        /// </value>
        [Parameter(Position = 0, ValueFromPipeline = true, ParameterSetName = "BuildParameterSet")]
        public CCBuild Build { get; set; }
        #endregion

        #region ShowRaw
        /// <summary>
        /// Gets or sets a value indicating whether the raw log should be displayed or not.
        /// </summary>
        /// <value>
        /// <c>true</c> to show the raw log; otherwise, <c>false</c>.
        /// </value>
        [Parameter]
        public bool ShowRaw { get; set; }
        #endregion
        #endregion

        #region Protected methods
        #region
        /// <summary>
        /// Processes a record.
        /// </summary>
        protected override void ProcessRecord()
        {
            this.WriteVerbose("Getting log");

            if (this.Project != null)
            {
                this.WriteLog(this.Project.GetLog(), this.ShowRaw);
            }
            else if (this.Build != null)
            {
                this.WriteLog(this.Build.GetLog(), true);
            }
            else
            {
                var connection = this.Connection
                                 ?? new CCConnection(ClientHelpers.GenerateClient(this.Address, this), new Version());
                this.WriteLog(connection.GetLog(), this.ShowRaw);
            }
        }
        #endregion
        #endregion

        #region Private methods
        #region WriteLog()
        /// <summary>
        /// Writes the log.
        /// </summary>
        /// <param name="log">The log.</param>
        /// <param name="showRaw">If set to <c>true</c> show the raw log.</param>
        private void WriteLog(string log, bool showRaw)
        {
            if (log == null)
            {
                return;
            }

            var lines = log.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (showRaw)
            {
                this.WriteObject(lines, true);
            }
            else
            {
                this.WriteObject(lines.Select(l => CCLogLine.Parse(l)), true);
            }
        }
        #endregion
        #endregion
    }
}
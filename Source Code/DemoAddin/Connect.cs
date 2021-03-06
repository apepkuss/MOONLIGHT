using System;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;

namespace DemoAddin
{
    /// <summary>The object for implementing an Add-in.</summary>
    /// <seealso class='IDTExtensibility2' />
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        #region IDTExtensibility2 Members

        /// <summary>Implements the constructor for the Add-in object. Place your initialization code within this method.</summary>
        public Connect()
        {
        }

        /// <summary>Implements the Exec method of the IDTCommandTarget interface. This is called when the command is invoked.</summary>
        /// <param term='commandName'>The name of the command to execute.</param>
        /// <param term='executeOption'>Describes how the command should be run.</param>
        /// <param term='varIn'>Parameters passed from the caller to the command handler.</param>
        /// <param term='varOut'>Parameters passed from the command handler to the caller.</param>
        /// <param term='handled'>Informs the caller if the command was handled or not.</param>
        /// <seealso class='Exec' />
        public void Exec(string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                if (commandName == "DemoAddin.Connect.DemoAddin")
                {
                    // anslyseBrowserWindow.Activate();
                    ChangeWindow(_applicationObject);

                    handled = true;
                    return;
                }
            }
        }

        /// <summary>Implements the OnAddInsUpdate method of the IDTExtensibility2 interface. Receives notification when the collection of Add-ins has changed.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>Implements the OnBeginShutdown method of the IDTExtensibility2 interface. Receives notification that the host application is being unloaded.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        /// <summary>Implements the OnConnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being loaded.</summary>
        /// <param term='application'>Root object of the host application.</param>
        /// <param term='connectMode'>Describes how the Add-in is being loaded.</param>
        /// <param term='addInInst'>Object representing this Add-in.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            this._applicationObject = (DTE2)application;
            this._addInInstance = (AddIn)addInInst;
            this._solutionEvents = _applicationObject.Events.SolutionEvents;
            DataModel.ApplicationObject = this._applicationObject;
            // Get the root directory of a solution
            solutionDir = _applicationObject.Solution.FullName;
            if (connectMode == ext_ConnectMode.ext_cm_AfterStartup ||
                connectMode == ext_ConnectMode.ext_cm_Startup)
            {
                #region Make sure add-in DemoAddin exist in menu bar "Tools"

                object[] contextGUIDS = new object[] { };
                Commands2 commands = (Commands2)_applicationObject.Commands;
                string toolsMenuName = "Tools";

                // Place the command on the tools menu.
                // Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
                Microsoft.VisualStudio.CommandBars.CommandBar menuBarCommandBar = ((Microsoft.VisualStudio.CommandBars.CommandBars)_applicationObject.CommandBars)["MenuBar"];

                // Find the Tools command bar on the MenuBar command bar:
                CommandBarControl toolsControl = menuBarCommandBar.Controls[toolsMenuName];
                CommandBarPopup toolsPopup = (CommandBarPopup)toolsControl;

                // This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
                // just make sure you also update the QueryStatus/Exec method to include the new command names.
                try
                {
                    // Add a command to the Commands collection:
                    Command command = commands.AddNamedCommand2(_addInInstance, "DemoAddin", "DemoAddin", "Executes the command for DemoAddin", true, 59, ref contextGUIDS, (int)vsCommandStatus.vsCommandStatusSupported + (int)vsCommandStatus.vsCommandStatusEnabled, (int)vsCommandStyle.vsCommandStylePictAndText, vsCommandControlType.vsCommandControlTypeButton);

                    // Add a control for the command to the tools menu:
                    if ((command != null) && (toolsPopup != null))
                    {
                        command.AddControl(toolsPopup.CommandBar, 1);
                    }
                }
                catch (System.ArgumentException)
                {
                    //If we are here, then the exception is probably because a command with that name
                    //  already exists. If so there is no need to recreate the command and we can
                    //  safely ignore the exception.
                }
                #endregion

                #region Generate Analyse window
                this.GenerateAnalyseExplorer();

                AddSloutionEventHandler();

                #endregion

            }
        }
        public void ChangeWindow(DTE2 dte)
        {
            // Create variables for the various tool windows.
            EnvDTE80.Window2 winFrame;
            Window win1 =
              dte.Windows.Item(Constants.vsWindowKindSolutionExplorer);
            Window win2 = dte.Windows.Item(Constants.vsWindowKindOutput);
            Window win3 = dte.Windows.Item(Constants.vsWindowKindCommandWindow);
            // Create a linked window frame and dock Solution 
            // Explorer and the Ouput window together inside it.
            winFrame = (Window2)dte.Windows.CreateLinkedWindowFrame(win1, anslyseBrowserWindow, vsLinkedWindowType.vsLinkedWindowTypeTabbed);

        }

        private void GenerateAnalyseExplorer()
        {
            string ctrlProgID = "DemoAddin.AnalysePanel";
            string guid = Guid.NewGuid().ToString("B");
            string ExplorerName = "Explorer";
            object controlObject = null;

            anslyseBrowserWindow = CreateWindow(ctrlProgID, guid, ref controlObject, ExplorerName);

            if (anslyseBrowserWindow != null)
            {
                anslyseBrowserWindow.WindowState = vsWindowState.vsWindowStateNormal;
                anslyseBrowserWindow.IsFloating = false;
                anslyseBrowserWindow.Linkable = false;
                anslyseBrowserWindow.Visible = true;
            }
        }

        /// <summary>Implements the OnDisconnection method of the IDTExtensibility2 interface. Receives notification that the Add-in is being unloaded.</summary>
        /// <param term='disconnectMode'>Describes how the Add-in is being unloaded.</param>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
        }

        /// <summary>Implements the OnStartupComplete method of the IDTExtensibility2 interface. Receives notification that the host application has completed loading.</summary>
        /// <param term='custom'>Array of parameters that are host application specific.</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>Implements the QueryStatus method of the IDTCommandTarget interface. This is called when the command's availability is updated</summary>
        /// <param term='commandName'>The name of the command to determine state for.</param>
        /// <param term='neededText'>Text that is needed for the command.</param>
        /// <param term='status'>The state of the command in the user interface.</param>
        /// <param term='commandText'>Text requested by the neededText parameter.</param>
        /// <seealso class='Exec' />
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                if (commandName == "DemoAddin.Connect.DemoAddin")
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
            }
        }

        /// <summary>
        /// add events handler to specific add-in events
        /// </summary>
        private void AddSloutionEventHandler()
        {
            this._solutionEvents.Opened +=
                new _dispSolutionEvents_OpenedEventHandler(this.SolutionOpened);

            this._solutionEvents.AfterClosing +=
                new _dispSolutionEvents_AfterClosingEventHandler(this.SolutionClosing);
        }

        private Window CreateWindow(string ctrlProgID, string guid, ref object controlObject, string caption)
        {
            Window win = null;
            try
            {
                // Get the executing assembly...
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                // Get Visual Studio's global collection of tool windows...
                Windows2 toolWins = (Windows2)this._applicationObject.Windows;

                // Create a new tool window, embedding the LineCounterBrowser control inside it...
                win = toolWins.CreateToolWindow2(this._addInInstance, asm.Location, ctrlProgID, caption, guid, ref controlObject);
            }
            catch (Exception)
            {
                throw new Exception();
            }
            return win;
        }

        private void SolutionClosing()
        {
            //  this.loadToolButton.Enabled = true;
        }

        private void SolutionOpened()
        {
            //if (anslyseBrowserWindow != null && anslyseBrowserWindow.Visible == false)
            //{
            //    anslyseBrowserWindow.Visible = true;
            //}
            //  ChangeWindow(_applicationObject);


            //this.loadToolButton.Enabled = true;
        }

        #endregion IDTExtensibility2 Members

        private AddIn _addInInstance;
        private DTE2 _applicationObject;
        private Window anslyseBrowserWindow;
        private EnvDTE.SolutionEvents _solutionEvents;
        private string solutionDir;
    }
}
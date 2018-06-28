using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using BSky.Lifetime;
using BSky.Interfaces.Interfaces;

namespace BlueSky
{
    public static class BSkyMouseBusyHandler
    {
        static Cursor defaultcursor;

        static int busycounter;
        //Shows mouse busy
        //function calling another function which in turn may call another function, when all these functions makes call to MouseBusy,
        // will increment the busycounter.
        //so later when we start returning back from each of these functions we make multiple MouseFree calls and decrement 
        // the busycounter finally making mouse free when counter reaches 0 or less.
        static public void ShowMouseBusy()
        {
            busycounter++;
            if (busycounter == 1)
            {
                defaultcursor = Mouse.OverrideCursor;
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                OutputWindowsMouseBusyShowHide(true);//make Scintilla control mouse busy
            }
        }

        /////////Hides Mouse busy///////
        // forcefree will help us to free mouse at once in case of exceptions. 
        // If there had been multiple MouseBusy calls the busycounter will be incemented. When the process is complete MouseFree will be called after 
        // returning from each function so the counter is decremented, finally the mouse will be set free.
        // But if something goes wrong and an exception is raised at that time there may not be a chance to return back from each function 
        // and call MouseFree that many times. Rather control goes to one level higher catch/finally block. so in such situations
        // we force mousefree by forceFree=true and set mouse free in just one call. 
        // And this call can be kept in some 'catch' or 'finally' block to make sure it runs
        static public void HideMouseBusy(bool forceFree=false) 
        {
            busycounter--;
            if (busycounter <= 0 || forceFree)
            {
                Mouse.OverrideCursor = null;
                OutputWindowsMouseBusyShowHide(false);//make Scintilla control mouse free
                busycounter = 0;//in case busycounter goes below 0, we need to set it to 0
            }
        }

        //Show or Hide mouse busy based on value of bool argument passed
        // true : make mouse busy
        // false: make mouse free
        static public void ShowHideBusy(bool makebusy)
        {
            if (makebusy)
            {
                busycounter++;
                if (busycounter == 1)
                {
                    defaultcursor = Mouse.OverrideCursor;
                    Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    // windowsFormsHost1.IsEnabled = false;
                    OutputWindowsMouseBusyShowHide(makebusy);
                }
            }
            else
            {
                busycounter--;
                if (busycounter <= 0)
                {
                    Mouse.OverrideCursor = null;
                    // windowsFormsHost1.IsEnabled = true;
                    OutputWindowsMouseBusyShowHide(makebusy);
                    busycounter = 0;//in case busycounter goes below 0, we need to set it to 0
                }
            }
            //OutputWindowsMouseBusyShowHide(makebusy, bkey);//commented because this must be executed after setting busykey and before unsetting busykey.
        }

        ////Mouse busy for Scintilla(Windows Form). This will try to show/hide mouse busy on WindowsFormHost
        // true : shows mouse busy
        // false: hides mouse busy
        // This should also handle mouse busy on all the output windows those are open right now.
        static OutputWindowContainer owc = (LifetimeService.Instance.Container.Resolve<IOutputWindowContainer>()) as OutputWindowContainer;//new line
        static public void OutputWindowsMouseBusyShowHide(bool makebusy)
        {
            foreach (KeyValuePair<string, IOutputWindow> kvp in owc.AllOutputWindows)
            {
                OutputWindow ow = kvp.Value as OutputWindow;
                ow.ScintillaMouseBusyShowHide(makebusy);
            }
        }
    }
}


#nullable enable

using System;
using System.Threading;
using System.Windows.Forms;

namespace QuickSearch;

internal static class Program
{
    private const string MutexName = @"Local\QuickSearch_SingleInstance";
    private const string SignalName = @"Local\QuickSearch_ShowExisting";

    private static Mutex? singleInstanceMutex;

    internal static EventWaitHandle? ShowExistingInstanceEvent { get; private set; }

    [STAThread]
    private static void Main()
    {
        // Create the event before acquiring the mutex.
        // This avoids a startup race when another process launches immediately.
        ShowExistingInstanceEvent = new EventWaitHandle(
            false,
            EventResetMode.AutoReset,
            SignalName);

        bool createdNew;

        singleInstanceMutex = new Mutex(
            true,
            MutexName,
            out createdNew);

        if (!createdNew)
        {
            // Another QuickSearch instance is already running.
            try
            {
                ShowExistingInstanceEvent.Set();
            }
            catch
            {
                // The existing instance may be shutting down.
            }

            ShowExistingInstanceEvent.Dispose();
            ShowExistingInstanceEvent = null;
            singleInstanceMutex.Dispose();
            singleInstanceMutex = null;
            return;
        }

        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
        finally
        {
            ShowExistingInstanceEvent?.Dispose();
            ShowExistingInstanceEvent = null;

            singleInstanceMutex?.ReleaseMutex();
            singleInstanceMutex?.Dispose();
            singleInstanceMutex = null;
        }
    }
}


using System;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DeskWidgets;

public partial class App : Application
{
    private const string MutexName =
        @"Local\DeskWidgets.SingleInstance";

    private const string ActivationEventName =
        @"Local\DeskWidgets.ShowManager";

    private Mutex? _instanceMutex;
    private EventWaitHandle? _activationEvent;
    private RegisteredWaitHandle? _activationRegistration;

    private bool _ownsMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        bool silentStartup = e.Args.Contains(
            "--startup",
            StringComparer.OrdinalIgnoreCase);

        _instanceMutex = new Mutex(
            initiallyOwned: true,
            name: MutexName,
            createdNew: out bool isFirstInstance);

        if (!isFirstInstance)
        {
            if (!silentStartup)
                SignalExistingInstance();

            Shutdown();
            return;
        }

        _ownsMutex = true;

        InitializeActivationListener();

        var manager = new MainWindow();

        MainWindow = manager;

        manager.InitializeWidgets();

        if (!silentStartup)
            manager.Show();
    }

    private void InitializeActivationListener()
    {
        _activationEvent = new EventWaitHandle(
            initialState: false,
            mode: EventResetMode.AutoReset,
            name: ActivationEventName);

        _activationRegistration =
            ThreadPool.RegisterWaitForSingleObject(
                _activationEvent,
                (_, timedOut) =>
                {
                    if (timedOut ||
                        Dispatcher.HasShutdownStarted ||
                        Dispatcher.HasShutdownFinished)
                    {
                        return;
                    }

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (MainWindow is DeskWidgets.MainWindow manager)
                            manager.ShowWidgetManager();
                    }));
                },
                state: null,
                millisecondsTimeOutInterval: Timeout.Infinite,
                executeOnlyOnce: false);
    }

    private static void SignalExistingInstance()
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            try
            {
                using var activationEvent =
                    EventWaitHandle.OpenExisting(
                        ActivationEventName);

                activationEvent.Set();
                return;
            }
            catch (WaitHandleCannotBeOpenedException)
            {
                Thread.Sleep(100);
            }
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _activationRegistration?.Unregister(null);
        _activationRegistration = null;

        _activationEvent?.Dispose();
        _activationEvent = null;

        if (_ownsMutex)
        {
            _instanceMutex?.ReleaseMutex();
            _ownsMutex = false;
        }

        _instanceMutex?.Dispose();
        _instanceMutex = null;

        base.OnExit(e);
    }
}

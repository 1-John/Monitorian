﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Monitorian.Models;
using Monitorian.Models.Monitor;

namespace Monitorian.ViewModels
{
	public class MenuWindowViewModel : ViewModelBase
	{
		private readonly MainController _controller;
		public Settings Settings => _controller.Settings;

		public MenuWindowViewModel(MainController controller)
		{
			this._controller = controller ?? throw new ArgumentNullException(nameof(controller));
		}

		#region License

		private const string LicenseFileName = "LICENSE.txt";

		public void OpenLicense()
		{
			Task.Run(() =>
			{
				var licenseFileContent = DocumentService.ReadEmbeddedFile($"Resources.{LicenseFileName}");
				var licenseFilePath = DocumentService.SaveAsHtml(LicenseFileName, licenseFileContent);

				Process.Start(licenseFilePath);
			});
		}

		#endregion

		#region Probe

		private int _count = 0;
		private const int CountDivider = 3;

		public void EnableProbe()
		{
			if (!CanProbe && (++_count % CountDivider == 0))
				CanProbe = true;
		}

		public bool CanProbe
		{
			get => _canProbe;
			private set => SetPropertyValue(ref _canProbe, value);
		}
		private bool _canProbe;

		public void PerformProbe()
		{
			CanProbe = false;

			Task.Run(async () =>
			{
				var log = await MonitorManager.ProbeMonitorsAsync();
				LogService.RecordProbe(log);
			});
		}

		#endregion

		#region Startup

		public bool CanRegister => _controller.StartupAgent.CanRegister();

		public bool IsRegistered
		{
			get
			{
				if (!_isRegistered.HasValue)
				{
					_isRegistered = _controller.StartupAgent.IsRegistered();
				}
				return _isRegistered.Value;
			}
			set
			{
				if (_isRegistered == value)
					return;

				if (value)
				{
					_controller.StartupAgent.Register();
				}
				else
				{
					_controller.StartupAgent.Unregister();
				}
				_isRegistered = value;
				RaisePropertyChanged();
			}
		}
		private bool? _isRegistered;

		#endregion

		public event EventHandler CloseAppRequested;

		/// <summary>
		/// Closes this application.
		/// </summary>
		public void CloseApp() => CloseAppRequested?.Invoke(this, EventArgs.Empty);

		#region IDisposable

		private bool _isDisposed = false;

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				CloseAppRequested = null;
			}

			_isDisposed = true;

			base.Dispose(disposing);
		}

		#endregion
	}
}
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using Amazon.Runtime.Internal;

using Epsagon.Dotnet.Core;
using Epsagon.Dotnet.Core.Configuration;
using Epsagon.Dotnet.Instrumentation.EFCore;
using Epsagon.Dotnet.Instrumentation.HttpClients;
using Epsagon.Dotnet.Tracing.OpenTracingJaeger;


namespace Epsagon.Dotnet.Instrumentation {
    public static class EpsagonBootstrap {
        private static readonly IConfigurationService configurationService = new ConfigurationService();

        public static void Bootstrap(bool useOpenTracingCollector = false, IEpsagonConfiguration configuration = null) {
            if ((Environment.GetEnvironmentVariable("DISABLE_EPSAGON") ?? "").ToUpper() != "TRUE") {
                if (configuration != null) { Utils.RegisterConfiguration(configuration); } else { Utils.RegisterConfiguration(LoadConfiguration()); }
                CustomizePipeline();
                SetupDiagnosticListeners();

                // Use either legacy tracer or opentracing tracer
                if (useOpenTracingCollector) {
                    Utils.DebugLogIfEnabled("remote");
                    JaegerTracer.CreateRemoteTracer();
                } else
                    JaegerTracer.CreateTracer();
                Utils.DebugLogIfEnabled("finished bootstraping epsagon");
            }
        }

        private static void CustomizePipeline() {
            Utils.DebugLogIfEnabled("customizing AWSSDK pipeline - START");
            RuntimePipelineCustomizerRegistry.Instance.Register(new EpsagonPipelineCustomizer());
            Utils.DebugLogIfEnabled("customizing AWSSDK pipeline - FINISHED");
        }

        private static void SetupDiagnosticListeners() {
            Utils.AddDisposable(DiagnosticListener.AllListeners.Subscribe(new DbDiagnosticsListener()));
            Utils.AddDisposable(DiagnosticListener.AllListeners.Subscribe(new HttpDiagnosticsListener()));
        }

        private static IEpsagonConfiguration LoadConfiguration() {
            Utils.DebugLogIfEnabled("loading epsagon configuration - START");
            var config = configurationService.GetConfig();

            Utils.DebugLogIfEnabled("loading epsagon configuration - FINISHED");
            Utils.DebugLogIfEnabled("loaded configuration: {@config}", config);
            return config;
        }
    }
}

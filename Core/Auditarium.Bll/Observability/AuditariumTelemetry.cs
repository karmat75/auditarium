// SPDX-License-Identifier: MIT
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Auditarium.Bll.Observability;

public static class AuditariumTelemetry
{
    public const string ServiceName = "Auditarium";
    public static readonly ActivitySource ActivitySource = new(ServiceName);
    public static readonly Meter Meter = new(ServiceName);
    public static readonly Counter<long> CqrsRequests = Meter.CreateCounter<long>("auditarium.cqrs.requests");
    public static readonly Histogram<double> CqrsRequestDuration = Meter.CreateHistogram<double>("auditarium.cqrs.request.duration", "ms");
    public static readonly Counter<long> CqrsErrors = Meter.CreateCounter<long>("auditarium.cqrs.errors");
}

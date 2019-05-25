namespace CheckoutPaymentGateway.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Prometheus;

    public class GatewayControllerBase : ControllerBase
    {
        private static readonly string[] ControllerMetricLabelNames = { "Route", "HttpVerb" };

        protected static readonly Counter RequestsReceivedCounter = Metrics.CreateCounter("paymentgateway_requests_received_total",
                                                                                          "Number of requests that were received.",
                                                                                          new CounterConfiguration
                                                                                          {
                                                                                              LabelNames = ControllerMetricLabelNames
                                                                                          });

        protected static readonly Counter RequestsProcessedCounter =
            Metrics.CreateCounter("paymentgateway_requests_processed_total", "Number of requests that were processed successfully.",
                                  new CounterConfiguration { LabelNames = ControllerMetricLabelNames });

        protected static readonly Summary RequestProcessingDurationSummary =
            Metrics.CreateSummary("paymentgateway_request_processing_duration_seconds",
                                  "Summary of request processing durations over last 10 minutes.",
                                  new SummaryConfiguration { LabelNames = ControllerMetricLabelNames });
    }
}